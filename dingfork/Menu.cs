/*
    Menu related classes
*/
using System.Runtime.InteropServices;
using System.Text;

namespace dingfork
{
    public class MenuOption()
    {
        // Name of the method this option invokes
        public string optionMethodName { get; set; } = "";

        // Description of the option to be displayed
        public string optionDescription { get; set; } = "";

    }

    public class Menu()
    {
        public string menuHeader { get; set; } = "";

        private string currentOptionSelection = "";
        private System.ConsoleColor optionHighlightColor = System.ConsoleColor.DarkCyan;
        private System.ConsoleColor defaultHighlightColor = System.ConsoleColor.White;
        private Dictionary<string, MenuOption> menuOptions = new Dictionary<string, MenuOption>();
        int longestOptionForKerning = 0;
        public string GetOptionMethodName(string optionHotkey)
        {
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
            int optionHotkeyLength = hotkey.Length;
            if (optionHotkeyLength > longestOptionForKerning) {
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
            // Don't write blank headers
            if(menuHeader !="") {
                Console.WriteLine("", menuHeader);
            }
            // HACK: Start with 1 and move 0 to the end of the list to match keyboard layout order
            foreach(var menuOption in menuOptions)
            {
                StringBuilder optionOutput = new StringBuilder();

                // Append kerning as the difference between the current and longest menu option hotkey
                for(int kerning = longestOptionForKerning - menuOption.Key.Length; kerning >= 0; kerning--) {
                    optionOutput.Append(" ");
                }

                optionOutput.Append(menuOption.Key+FileHelper.USER_INPUT_ARROW+" "+menuOption.Value.optionDescription);

                if (menuOption.Key == currentOptionSelection)
                {
                    Console.ForegroundColor = optionHighlightColor;
                    Console.WriteLine(optionOutput);
                    Console.ForegroundColor = defaultHighlightColor;
                }
                else
                {
                    Console.WriteLine(optionOutput);
                }
            }
        }
    }
}