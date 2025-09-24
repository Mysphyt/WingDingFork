/* WingDingFork

    TODO: 
        * Reserved menu shortcuts
        * Break out main methods, getting too long
        * Convert save option to a menu?
        * Add "password" protected messages with required input bytes
        * Allow for modified hotkeys, ctrl+, alt+
        * Add programmable background music
        * Animate rainbow WingDingFork logo
*/

using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace dingfork
{
    /*
        Main program namespace
    */
    public class DingFork
    {
        // Config variable mapping
        private Dictionary<string, string> configMap = new Dictionary<string, string>();

        // Class for loading and storing data files
        private static DataLoader dataLoader = new DataLoader("default");

        // StringBuilders for user input
        private StringBuilder userCode = new StringBuilder();

        // Current code output
        private string codeOutput = "";
        // Store the current menu 

        private static string CleanUserCode(string userCode)
        {
            /*
                Currently just replaces instruction delimiters with empty strings
                    Used for printing without delimiters

                Args:
                    userCode: Code to be "cleaned" for printing
            */
            string cleanUserCode = userCode;

            cleanUserCode = cleanUserCode.Replace(FileHelper.INSTRUCTION_DELIM, "");

            return cleanUserCode;
        }

        static string InterpretWingDingCode(string userCode)
        {
            /*
                Interprets and runs userCode
            */

            string parsedCode = dataLoader.ParseSubroutines(userCode);

            var interpreter = new Runner();

            interpreter.LoadInstructionMap(dataLoader.wingdingsToInstructions);

            return interpreter.Run(parsedCode, userCode);
        }
        public void PasteCode()
        {
            /*
                Prompt user to paste code to run
            */
            Console.Clear();
            FileHelper.PrintWdfHeader();
            Console.Write("\npaste code " + FileHelper.USER_INPUT_ARROW);
            string pastedCode = Console.ReadLine();
            userCode = new StringBuilder(pastedCode);
        }
        public void Save()
        {
            /*
                Saves the current userCode to a new subroutine
            */
            if (userCode.ToString() == "")
            {
                // No code, nothing to do
                return;
            }
            Console.Clear(); 
            // Save the current userCode as a new subroutine
            dataLoader.SaveSubroutine(userCode.ToString());
        }

        public void DeleteHotkey(string hotkey)
        {
            dataLoader.DeleteHotkey(hotkey);
        }

        public void EditHotkey(string hotkey)
        {
            // TODO
            Console.Clear();
            FileHelper.PrintWdfHeader();
            Console.Write("Enter new hotkey to replace {0} ", hotkey);
            string newHotkey = GetUserInput();

            dataLoader.EditHotkey(hotkey, newHotkey);
        }

        public void EditWingding(string wingding)
        {
            // TODO
            Console.Clear();
            FileHelper.PrintWdfHeader();
            Console.Write("Enter new wingding to replace {0} ", wingding);
            string newWingding = Console.ReadLine();  //GetUserInput();

            dataLoader.EditWingding(wingding, newWingding);
        }

        public void EditInstruction(string hotkey)
        {
            // TODO

        }

        public void ListHotkeys()
        {
            /*
                Lists currently available hotkeys from keymap
            */
            Console.Clear();
            List<string> hotkeyMenuOptions = [];
            foreach (var keyWing in dataLoader.keysToWingDings)
            {
                string instruction = dataLoader.wingdingsToInstructions[keyWing.Value];
                hotkeyMenuOptions.Add(string.Format("{0}|{1}|{2}", instruction, instruction+" "+keyWing.Value, keyWing.Key));
            }
            hotkeyMenuOptions.Add("Back|Back|0");
            Menu hotkeyListMenu = CreateMenu("", "View/Edit Hotkeys", hotkeyMenuOptions);
            while (true)
            {
                Console.Clear();
                FileHelper.PrintWdfHeader();
                hotkeyListMenu.PrintMenu();
                string userInput = GetUserInput();
                string optionInstruction = hotkeyListMenu.GetOptionMethodName(userInput);
                if (optionInstruction == "Back")
                {
                    break;
                }
                else if (optionInstruction != "")
                {
                    // Hotkey selected
                    string editMenuHeader = string.Format(
                        "\n    name{1}{2}\n  hotkey{1}{0}\nwingding{1}{3}\n",
                         userInput, FileHelper.USER_INPUT_ARROW, optionInstruction, dataLoader.keysToWingDings[userInput]);
                    string hotkey = userInput;
                    Menu hotkeyEditMenu = CreateMenu("editmenu", editMenuHeader, []);
                    while (true)
                    {
                        Console.Clear();
                        FileHelper.PrintWdfHeader();
                        hotkeyEditMenu.PrintMenu();
                        userInput = GetUserInput();
                        string editInstruction  = hotkeyEditMenu.GetOptionMethodName(userInput);
                        if (editInstruction == "Back")
                        {
                            break;
                        }
                        else
                        {
                            GetType().GetMethod(editInstruction)?.Invoke(this, [hotkey]);
                            // Update the menu with new hotkey data
                            if (editInstruction == "DeleteHotkey" || editInstruction == "EditHotkey")
                            {
                                hotkeyMenuOptions = [];
                                foreach (var keyWing in dataLoader.keysToWingDings)
                                {
                                    string instruction = dataLoader.wingdingsToInstructions[keyWing.Value];
                                    hotkeyMenuOptions.Add(string.Format("{0}|{1}|{2}", instruction, instruction+" "+keyWing.Value, keyWing.Key));
                                }
                                hotkeyMenuOptions.Add("Back|Back|0");
                                hotkeyListMenu = CreateMenu("", "View/Edit Hotkeys", hotkeyMenuOptions);
                                break;    
                            }
                        }
                    }
                }
            }
            // Display current key mappings

        }

        public void Quit()
        {
            /*
                Exits the program
            */
            System.Environment.Exit(1);
        }

        public string Run()
        {
            /*
                Runs the current userCode
            */
            return InterpretWingDingCode(userCode.ToString());
        }

        public void Clear()
        {
            /*
                Clears the current code and output
            */
            Console.Clear();
            userCode.Clear();
            codeOutput = "";
        }

        public void Pop()
        {
            /*
                Removes the last instruction from userCode()
            */
            // Split instructions by the delim 
            string[] instructions = userCode.ToString().Split(FileHelper.INSTRUCTION_DELIM);

            if (instructions.Length > 1)
            {
                // Re-build the code without the trailing instruction
                // Add trailing delimiter
                string newCode = string.Join(FileHelper.INSTRUCTION_DELIM, instructions.Take(instructions.Length - 2)) + FileHelper.INSTRUCTION_DELIM;
                userCode = new StringBuilder(newCode);
            }
        }

        public void ChangeConfig()
        {
            /*
                Change the current data configuration 
            */
            Console.Clear();
            FileHelper.PrintWdfHeader();
            Console.WriteLine("Current config {0} {1}", FileHelper.USER_INPUT_ARROW, dataLoader.dataConfigName);
            Console.Write("\nEnter existing config name {0} ", FileHelper.USER_INPUT_ARROW);
            var newConfig = Console.ReadLine();

            if (newConfig == "") { return; }

            string configDir = string.Format("{0}/{1}", DataLoader.dataDirectory, newConfig);

            if (!Directory.Exists(configDir))
            {
                Console.WriteLine("Config directory not found: {0}", configDir);
                UserOpts.PressAnyKey();
                // Go back to the main menu
                return;
            }

            // Load the config if it does not match the current
            if (dataLoader.dataConfigName != newConfig)
            {
                dataLoader.dataConfigName = newConfig;
                dataLoader.LoadData();
            }

            Console.Clear();
        }

        public void UpdateUserCodeFromBF(string bfCode)
        {
            /*
                Convert BrainF*ck code to wingding code
                *Note: Assumes current configuration is set up with default BF instructions as hotkeys
            */
            userCode = new StringBuilder();
            foreach (char bfInstruction in bfCode)
            {
                string instruction = dataLoader.GetDing(bfInstruction.ToString()) + FileHelper.INSTRUCTION_DELIM;
                userCode.Append(instruction);
            }
        }

        public void ConvertText()
        {
            /*
                Converts text to wingding BrainF*ck instructions
            */
            Console.Clear();
            FileHelper.PrintWdfHeader();
            Console.Write("Enter text to convert: ");

            // Get the text to convert and update UserCode
            string inputText = Console.ReadLine();
            string convertedBFCode = BFConverter.ConvertTextToBF(inputText, dataLoader.instructionsToKeys);
            UpdateUserCodeFromBF(convertedBFCode);

            Console.WriteLine("Updated code to:\n {0}", CleanUserCode(userCode.ToString()));
            UserOpts.PressAnyKey("\nPress any key to Run...\n");
        }

        public void LoadCode()
        {
            /*
                Loads wingdings or hotkeys from a file
                *Note: currently only loads hotkey instructions
                TODO: allow loading non-delimited wingding code
            */
            Console.Clear();
            FileHelper.PrintWdfHeader();
            Console.Write("Enter file path "+FileHelper.USER_INPUT_ARROW);

            string codeFilepath = Console.ReadLine();
            if (!File.Exists(codeFilepath))
            {
                UserOpts.PressAnyKey(string.Format("\nFile {0} does not exist.\nPress any key to continue...", codeFilepath));
                return;
            }

            string fileCode = File.ReadAllText(codeFilepath);
            fileCode = dataLoader.ParseSubroutines(fileCode);

            Console.WriteLine("Updated code to:\n {0}", CleanUserCode(userCode.ToString()));
            UserOpts.PressAnyKey("\nPress any key to Run...\n");
        }

        public string GetUserInput()
        {
            /*
                Waits for the user to input a key and returns a translated string representation 
            */
            string userKey = "";
            ConsoleKeyInfo userKeyInfo = Console.ReadKey();

            // Key.ToString parses above special keys but does not work for lowercase letters and other chars like ({}\!@#$%)
            string[] specialKeys = ["backspace", "enter", "escape", "control"];
            string userKeyToString = userKeyInfo.Key.ToString().ToLower();

            // Check if user input is among the above special keys
            if (specialKeys.Contains(userKeyToString))
            {
                // Special key, use ConsoleKey string
                userKey = userKeyToString;
            }
            else
            {
                // Not a special key, use KeyChar
                userKey = userKeyInfo.KeyChar.ToString();
            }
            return userKey;
        }

        // String to format for user code output
        static string unformattedCodeOutput = """

              code ⮚ {0}

            output ⮚ {1}

        """;

        public Menu CreateMenu(string menuName, string menuHeader, List<string> menuOptions)
        {
            // New menu for this loop
            Menu menu = new Menu
            {
                menuHeader = menuHeader
            };
            string menuConfig = string.Format("{0}/menus/{1}", DataLoader.dataDirectory, menuName);
            // Parse menu options if none were passed
            if (menuOptions.Count == 0 && File.Exists(menuConfig))
            {
                menuOptions = new List<string>(File.ReadAllLines(menuConfig));
            }

            // Populate main menu options
            for (int optionIt = 0; optionIt < menuOptions.Count; optionIt++)
            {
                string[] optionArgs = menuOptions[optionIt].Split("|");
                menu.AddOption(optionArgs[0], optionArgs[1], optionArgs[2]);
                // reservedhotkeys.Add(optionArgs[2]);
            }
            return menu;
        }

        public void MainLoop()
        {
            /*
                Main program loop
            */

            // New menu for this loop
            Menu mainMenu = CreateMenu("mainmenu", "", []);

            // Load config.yml (fake yml read for now)
            string configPath = string.Format("{0}/config.yml", DataLoader.dataDirectory);

            configMap = FileHelper.ParseYAML(configPath);

            if (configMap.ContainsKey("dataConfigName"))
            {
                dataLoader = new DataLoader(configMap["dataConfigName"]);
            }

            while (true)
            {
                // Refresh the console output for other potential option menus
                Console.Clear();

                // Print menu info
                FileHelper.PrintWdfHeader();
                Console.WriteLine(string.Format(unformattedCodeOutput, CleanUserCode(userCode.ToString()), codeOutput, dataLoader.dataConfigName));
                mainMenu.PrintMenu();

                // Cast to lowercase for now. Need to re-write input logic because ConsoleKeys are always cast to capital letters
                //      Using KeyChar was allowing lowercase letters, but is always an empty string for special keys
                string userKeyString = GetUserInput();

                // If the user entered an available option [0..mthdOptions.Length]
                if (mainMenu.HasHotkey(userKeyString))
                {
                    try
                    {
                        // Invoke the option method
                        var optionOutput = GetType().GetMethod(mainMenu.GetOptionMethodName(userKeyString))?.Invoke(this, []);
                        // Invoke the option method

                        if (optionOutput is string && mainMenu.GetOptionMethodName(userKeyString) == "Run")
                        {
                            codeOutput = Convert.ToString(optionOutput);
                        }
                    }
                    catch (Exception e)
                    {
                        UserOpts.PressAnyKey(string.Format("\nOption {0} failed with:\n{1}\n\nPress any key to continue...", userKeyString, e.ToString()));
                    }
                }
                else
                {
                    string wingding = "";

                    // Parse the type of input the user entered
                    wingding = dataLoader.GetDing(userKeyString);

                    userCode.Append(wingding + FileHelper.INSTRUCTION_DELIM);
                }
           }
        }

    }



    class MainClass // Runs DingFork.MainLoop
    {
        /*
            Main method
        */
        static void Main()
        {
            // Allows unicode characters to be printed to the console 
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.InputEncoding = Encoding.Unicode;

            // Start the Main program loop
            DingFork df = new DingFork();

            df.MainLoop();
        }
    }

}