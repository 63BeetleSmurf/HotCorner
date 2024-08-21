//using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Hot_Corner
{
    internal static class Program
    {

        // Import Windows API function
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        // Constants for keyboard buttons used in application
        private const byte VK_CONTROL = 0x11;
        private const byte VK_TAB = 0x09;
        private const byte VK_LWIN = 0x5B;

        // Enum used with KeyEvent() to specify which key action should be applied
        private enum KeyAction { Down, Up, Press }

        // Use these constants to configure sensitivity of the hot corner
        // Size of the hot corner
        private const int HOT_CORNER_SIZE = 2;
        // Time mouse should be in corner before activating
        private const int DEBOUNCE_TIME = 25;
        // Time between checks
        private const int CHECK_DELAY = 100;

        static void Main()
        {
            // Variable to set hot corner as active when mouse enters
            // -- Used to prevent continually activating while mouse is "hot"
            bool hotCornerActive = false;

            // Loop forever
            while (true)
            {
                // Slow loop down by adding specified delay between checks
                Thread.Sleep(CHECK_DELAY);

                // If the mouse enters a hot corner
                if (IsCursorHot())
                {
                    // Wait for specified time to prevent accidental activation
                    Thread.Sleep(DEBOUNCE_TIME);
                    // If the mouse has left the hot corner
                    if (!IsCursorHot())
                        // return to the start of the loop
                        continue;

                    // If the hot corner has not been activated
                    if (!hotCornerActive)
                    {
                        // If no mouse buttons as held
                        // -- Checks mouse buttons to prevent activation when snapping windows or using snip tool
                        if (Control.MouseButtons == MouseButtons.None) {
                            // If CTRL + Shift are held
                            if (Control.ModifierKeys == (Keys.Control | Keys.Shift))
                            {
                                // Exit loop, terminating application
                                break;
                            }
                            // ISSUE: Following does not seem to work anymore, was temperamental at best when it did.
                            //// If CTRL is held
                            //else if (Control.ModifierKeys == Keys.Control)
                            //{
                            //    // Launch explorer.exe with shell GUID to activate Task View and wait to exit
                            //    // -- This allows the hot corner to activate Task View on the host system only
                            //    Process taskViewProcess = Process.Start("explorer.exe", "shell:::{3080F90E-D7AD-11D9-BD98-0000947B0257}");
                            //    taskViewProcess.WaitForExit();
                            //    Debug.WriteLine(taskViewProcess.ExitCode);
                            //}
                            // If mouse has entered hot corner with no mouse or keyboard buttons held
                            else
                            {
                                // Activate Task View by simulating keyboard button presses
                                // -- This allows the hot corner to activate Task View on remote machine when using Remote Desktop

                                // Hold WIN, press tab then release WIN button
                                KeyEvent(VK_LWIN, KeyAction.Down);
                                KeyEvent(VK_TAB, KeyAction.Press);
                                KeyEvent(VK_LWIN, KeyAction.Up);

                                // If executing this code when CTRL is held, use the following
                                // to release CTRL temporally while Win+Tab is pressed
                                //KeyEvent(VK_CONTROL, KeyAction.Up);
                                //...Press Win+Tab
                                //KeyEvent(VK_CONTROL, KeyAction.Down);
                            }
                        }
                        // Set hot corner as active so it is not continually triggered
                        // If a mouse button has been held still set as active so corener if not triggered upon release
                        hotCornerActive = true;
                    }
                }
                // If the mouse has left the hot corner after activating it
                else if (hotCornerActive)
                {
                    // Set hot corner as inactive
                    hotCornerActive = false;
                }
            }
        }

        // Function to check is curser is in the top left corner of any screen
        static bool IsCursorHot()
        {
            // Get the screen the mouse cursor is currently on
            Screen currentScreen = Screen.FromPoint(Cursor.Position);

            // If the cursor is at the top left within the specified hot corner size
            if (Cursor.Position.X >= currentScreen.WorkingArea.Left
                && Cursor.Position.X <= currentScreen.WorkingArea.Left + HOT_CORNER_SIZE
                && Cursor.Position.Y >= currentScreen.WorkingArea.Top
                && Cursor.Position.Y <= currentScreen.WorkingArea.Top + HOT_CORNER_SIZE
            )
                return true;
            return false;
        }

        // Function to simulate keyboard button presses
        private static void KeyEvent(byte virtualKey, KeyAction action)
        {
            // If key should be held down or pressed
            if (action == KeyAction.Down || action == KeyAction.Press)
                // Call API to hold key down
                keybd_event(virtualKey, 0, 0, 0);

            // If key should be released or a button is being pressed
            if (action == KeyAction.Up || action == KeyAction.Press)
                // Call API to release key
                // -- Used const KEYEVENTF_KEYUP = 0x0002, but no point declaring for single use
                keybd_event(virtualKey, 0, 0x0002, 0);
        }
    }
}
