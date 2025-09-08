/*
    WingDingFork

    TODO: 
        * Add "password" protected messages with required input bytes
        * ~Detect BrainF*ck vs. Hotkey vs. WingDing code input~
             > Allow for any type of code input when loading from files or pasting
                 . Allows directly pasting wingdings or brainfuck into the interpreter
        * Add menu "mode" to allow for KeyChar / ie: lowercase user hotkey input
            * Remap menu hotkeys from WingDingFork menu
        * Menu file validation
        * Add programmable background music
        * Animate rainbow WingDingFork logo
*/

using System.Text.RegularExpressions;
using System.Text;

namespace dingfork
{
    /*
        Main program namespace
    */
    public class DingFork
    {
        private enum UserInputType //
        {
            hotkey,
            wingding,
            restricted,
            unknown
            // TODO: default Brainf*ck
        };
        // Config variable mapping
        private Dictionary<string, string> configMap = new Dictionary<string, string>();

        // Class for loading and storing data files
        private static DataLoader dataLoader = new DataLoader("default");

        // StringBuilders for user input
        private StringBuilder userCode = new StringBuilder();

        // Current code output
        private string codeOutput = "";

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

        // Hacky list of restricted shortcuts for menus and delimiters
        public List<string> restrictedShortcuts = new List<string> { "^", "|" };

        private UserInputType DetectUserInputType(string userInput)
        {
            if (restrictedShortcuts.Contains(userInput))
            {
                // This is a restricted or menu option
                return UserInputType.restricted;
            }
            else if (dataLoader.keysToWingDings.ContainsKey(userInput))
            {
                // User input is a loaded hotkey
                return UserInputType.hotkey;
            }
            else if (dataLoader.wingDingsToInstructions.ContainsKey(userInput))
            {
                // User input is a loaded wingding
                return UserInputType.wingding;
            }
            // Unknown input type
            return UserInputType.unknown;
        }

        static string InterpretWingDingCode(string userCode)
        {
            // Parse any subroutines
            //      returns the wingding code output

            string parsedCode = dataLoader.ParseSubroutines(userCode);

            var interpreter = new Runner();

            interpreter.LoadInstructionMap(dataLoader.wingDingsToInstructions);

            return interpreter.Run(parsedCode, userCode);
        }

        public void Save()
        {
            // Save the current userCode as a new subroutine
            dataLoader.SaveSubroutine(userCode.ToString(), restrictedShortcuts);
        }
        public void ListHotkeys()
        {
            // Display current key mappings
            dataLoader.PrintKeymap();
        }

        public void ListSubroutines()
        {
            dataLoader.PrintSubroutines();
        }

        public void Quit()
        {
            System.Environment.Exit(1);
        }

        public string Run()
        { // Run the current code
            return InterpretWingDingCode(userCode.ToString());
        }

        public void Clear()
        {
            /*
                Clears the current code and output
            */
            userCode.Clear();
            codeOutput = "";
            Console.Clear();
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
            else
            {
                return;
            }

        }

        public void ChangeConfig()
        {
            /*
                Change the current data configuration 
            */
            FileHelper.print_wdf_header();
            Console.WriteLine("Current config {0} {1}", FileHelper.USER_INPUT_ARROW, dataLoader.dataConfigName);
            Console.Write("\nEnter existing config name {0} ", FileHelper.USER_INPUT_ARROW);
            var newConfig = Console.ReadLine();

            if (newConfig == null) { return; }

            string configDir = String.Format("{0}/{1}", DataLoader.dataDirectory, newConfig);

            if (!Directory.Exists(configDir))
            {
                Console.WriteLine("Config directory not found: {0}", configDir);
                UserOpts.PressAnyKey();
                // Go back to the main menu
                return;
            }

            // TODO: Create method DataLoader.SetConfig
            dataLoader.dataConfigName = newConfig;
            dataLoader.LoadData();

            Console.Clear();
        }

        public void PrintConfig()
        {
            // TODO: print config debug info
            Console.WriteLine("TODO");
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
            FileHelper.print_wdf_header();
            Console.Write("Enter text to convert: ");

            // Get the text to convert and update UserCode
            string inputText = Console.ReadLine();
            string convertedBFCode = BFConverter.ConvertTextToBF(inputText);
            UpdateUserCodeFromBF(convertedBFCode);

            Console.WriteLine("Updated code to:\n {0}", CleanUserCode(userCode.ToString()));
            UserOpts.PressAnyKey("\nPress any key to Run...\n");

            // Start the run loop with the new code
            MainLoop();
        }

        public void PasteCode()
        {
            /*
                DEPRECATED
                Get WingDing code from a pasted string
            */
            FileHelper.print_wdf_header();
            Console.WriteLine("\n*code must be | delimited wingdings*\nPaste your code, then hit enter:\n");
            string pastedCode = Console.ReadLine();

            // TODO: Parse/sanitize pasted code for available instructions
            //       Allow for non-delmited code or code in keybindings (+-^<>... etc)
            userCode = new StringBuilder(pastedCode);
            Run();
            MainLoop();
        }

        public void LoadCode()
        {
            /*
                Loads wingdings or hotkeys from a file
                *Note: currently only loads hotkey instructions
                TODO: allow loading non-delimited wingding code
            */
            FileHelper.print_wdf_header();
            Console.Write("Enter file path: ");

            string codeFilepath = Console.ReadLine();
            if (!File.Exists(codeFilepath))
            {
                UserOpts.PressAnyKey(String.Format("\nFile {0} does not exist.\nPress any key to continue...", codeFilepath));
            }

            string bfCode = File.ReadAllText(codeFilepath);
            UpdateUserCodeFromBF(bfCode);

            Console.WriteLine("Updated code to:\n {0}", CleanUserCode(userCode.ToString()));
            UserOpts.PressAnyKey("\nPress any key to Run...\n");

            // Start the run loop with the new code
            Run();
            MainLoop();
        }



        // String to format for user code output
        static string unformattedCodeOutput = """

            code   ⮚ {0}

            output ⮚ {1}

        """;

        public void MainLoop()
        {
            // New menu for this loop
            Menu mainMenu = new Menu
            {
                menuHeader = ""
            };
            string[] mainMenuOptions = File.ReadAllLines(String.Format("{0}/menus/{1}", DataLoader.dataDirectory, "mainmenu"));

            // Reset the restricted shortcuts
            restrictedShortcuts = new List<string> { "^", FileHelper.INSTRUCTION_DELIM };

            // Populate main menu options
            for (int optionIt = 0; optionIt < mainMenuOptions.Length; optionIt++)
            {
                string[] optionArgs = mainMenuOptions[optionIt].Split("|");
                mainMenu.AddOption(optionArgs[0], optionArgs[1], optionArgs[2]);
                restrictedShortcuts.Add(optionArgs[2]);
            }
            // Load config.yml (fake yml read for now)
            string configPath = String.Format("{0}/config.yml", DataLoader.dataDirectory);

            configMap = FileHelper.ParseYAML(configPath);

            if (configMap.ContainsKey("dataConfigName"))
            {
                dataLoader = new DataLoader(configMap["dataConfigName"]);
            }

            while (true)
            {
                // Print mainMenu info
                FileHelper.print_wdf_header();
                Console.WriteLine(String.Format(unformattedCodeOutput, CleanUserCode(userCode.ToString()), codeOutput, dataLoader.dataConfigName));
                mainMenu.PrintMenu();

                ConsoleKey userKey = Console.ReadKey().Key;

                // Cast to lowercase for now. Need to re-write input logic because ConsoleKeys are always cast to capital letters
                //      Using KeyChar was allowing lowercase letters, but is always an empty string for special keys
                string userKeyString = userKey.ToString();

                if (Regex.IsMatch(userKeyString, @"^(d|D)\d+$"))
                {
                    // Trim the leading "D" from numeric Key input
                    userKeyString = userKeyString.Substring(1, userKeyString.Length - 1);
                }
                else {
                    userKeyString = userKeyString.ToLower();
                }

                Console.Clear();

                // If the user entered an available option [0..mthdOptions.Length]
                if (restrictedShortcuts.Contains(userKeyString))
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
                        UserOpts.PressAnyKey(String.Format("\nOption {0} failed with:\n{1}\n\nPress any key to continue...", userKeyString, e.ToString()));
                    }
                }
                else
                {
                    string wingDing = "";

                    // Parse the type of input the user entered
                    UserInputType inputType = DetectUserInputType(userKeyString);
                    if (inputType == UserInputType.hotkey)
                    {
                        wingDing = dataLoader.GetDing(userKeyString);
                        if (wingDing == "")
                        {
                            continue;
                        }
                    }
                    else if (inputType == UserInputType.restricted)
                    {
                        continue;
                    }
                    else if (inputType == UserInputType.wingding)
                    {
                        wingDing = userKeyString;
                    }

                    // Use | as delimeter
                    // --> certain characters have a Length of 2, ie 👇.Length, 👍
                    //  can't iterate one string length at a time and uncertainty of user input length.
                    userCode.Append(wingDing + FileHelper.INSTRUCTION_DELIM);
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