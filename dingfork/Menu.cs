/*
    Menu and MenuOption classes
*/
using System.Text;

namespace dingfork
{
    public class MenuOption()
    {
        /*
            A single menu option
        */

        // Name of the method this option invokes
        public string optionMethodName { get; set; } = "";

        // Description of the option to be displayed
        public string optionDescription { get; set; } = "";

    }

    public class Menu()
    {
        /*
            Stores and prints menu information     
        */
        public string menuHeader { get; set; } = "";

        // Current menu hotkey selection
        private string currentOptionSelection = "";

        // Highlight and default colors
        private System.ConsoleColor optionHighlightColor = System.ConsoleColor.DarkCyan;
        private System.ConsoleColor defaultHighlightColor = System.ConsoleColor.White;

        // Menu options <hotkey, option>
        private Dictionary<string, MenuOption> menuOptions = new Dictionary<string, MenuOption>();

        // Store the longest option hotkey length for use in kerning options to line up vertically
        int longestOptionForKerning = 0;

        public bool HasHotkey(string hotkey)
        {
            return menuOptions.ContainsKey(hotkey);
        }

        public string GetOptionMethodName(string optionHotkey)
        {
            /*
                Returns the method name associated with the given option hotkey
            */
            if (menuOptions.ContainsKey(optionHotkey) && currentOptionSelection == optionHotkey)
            {
                return menuOptions[optionHotkey].optionMethodName;
            }
            else
            {
                currentOptionSelection = optionHotkey;
                return "";
            }
        }

        public void AddOption(string optionMethodName, string optionDescription, string hotkey)
        {
            /*
                Adds an option to the menu
            */
            int optionHotkeyLength = hotkey.Length;
            if (optionHotkeyLength > longestOptionForKerning) {
                // Update the current longest option hotkey value
                longestOptionForKerning = optionHotkeyLength;
            }
            menuOptions.Add(
                hotkey,
                new MenuOption
                {
                    optionMethodName = optionMethodName,
                    optionDescription = optionDescription,
                });
        }

        public void PrintMenu()
        {
            /*
                Prints the menu
            */

            // Don't write blank headers
            if(menuHeader !="") {
                Console.WriteLine(menuHeader);
            }
            // HACK: Start with 1 and move 0 to the end of the list to match keyboard layout order
            foreach(var menuOption in menuOptions)
            {
                StringBuilder optionOutput = new StringBuilder();

                // Append kerning as the difference between the current and longest menu option hotkey
                for(int kerning = longestOptionForKerning - menuOption.Key.Length; kerning >= 0; kerning--) {
                    optionOutput.Append(" ");
                }

                // Append the option string
                optionOutput.Append(menuOption.Key+FileHelper.USER_INPUT_ARROW+menuOption.Value.optionDescription);

                // Highlight the option if it is the current selection
                if (menuOption.Key == currentOptionSelection)
                {
                    Console.ForegroundColor = optionHighlightColor;
                    Console.WriteLine(optionOutput);
                    // Set highlighting back to default
                    Console.ForegroundColor = defaultHighlightColor;
                }
                else
                {
                    // Write the non-highlighted option
                    Console.WriteLine(optionOutput);
                }
            }
        }
    }
}