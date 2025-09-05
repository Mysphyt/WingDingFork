/*
    Menu related classes
*/
using System.Runtime.InteropServices;

namespace dingfork
{
    public class MenuOption
    {
        // Name of the method this option invokes
        public string optionMethodName { get; set; }

        // Description of the option to be displayed
        public string optionDescription { get; set; }

        public MenuOption() { }
    }

    public class Menu
    {
        public string menuHeader { get; set; }

        private int currentOptionIt = 0;
        private System.ConsoleColor optionHighlightColor = System.ConsoleColor.DarkCyan;
        private System.ConsoleColor defaultHighlightColor = System.ConsoleColor.White;

        private List<MenuOption> menuOptions = new List<MenuOption>();
        // Default constructor
        public Menu() { }

        public string GetOptionMethodName(int optionIndex)
        {
            if (optionIndex >= menuOptions.Count)
            {
                // Invalid option 
                return "";
            }
            else if (optionIndex == currentOptionIt)
            {
                return menuOptions[optionIndex].optionMethodName;
            }
            else
            {
                currentOptionIt = optionIndex;
                return "";
            }
        }

        public void AddOption(string optionMethodName, string optionDescription)
        {
            menuOptions.Add(new MenuOption { optionMethodName=optionMethodName, optionDescription=optionDescription });
        }

        public void PrintMenu()
        {
            Console.WriteLine(menuHeader);

            int optionIt = 0;
            foreach (MenuOption option in menuOptions)
            {
                string optionOutput = String.Format("{0} {1} {2}", optionIt, FileHelper.USER_INPUT_ARROW, option.optionDescription);
                if (optionIt == currentOptionIt)
                {
                    Console.ForegroundColor = optionHighlightColor;
                    Console.WriteLine(optionOutput);
                    Console.ForegroundColor = defaultHighlightColor;
                }
                else
                {
                    Console.WriteLine(optionOutput);
                } 
                optionIt++;
            }
        }
    }
}