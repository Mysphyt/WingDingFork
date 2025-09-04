/*
    WingDingFork
*/

using System.Text;
using Interpreter;
using Data;
using Helper;
using Translator;

namespace dingfork
{
    /*
        Main program namespace
    */
    public class DingFork
    {

        const string MAINLOOP_HEADER = """

            Loaded configuration ⮚ {0}

            1 ⮚ Code New wingding
            2 ⮚ Convert text to wingding
            3 ⮚ Paste new wingding
            4 ⮚ Load BrainF*ck from file
            5 ⮚ Change configuration
            6 ⮚ Configuration info
            0 ⮚ Exit

        """;
        // Indexed Method names -- should match the order of above options
        // TODO: make this dynamic
        public string[] mainMthdOptions = ["Quit",
            "RunLoop",
            "ConvertText",
            "PasteCode",
            "LoadCode",
            "ChangeConfig",
            "PrintConfig"
        ];


        /*
            TODO: validate that loaded keymap does not overlap this instruction set
        */
        const string RUNLOOP_HEADER = """
        
        enter your code ⮚ {0}

        1 ⮚ Run
        2 ⮚ Delete last instruction
        3 ⮚ Clear all instructions
        4 ⮚ Save as subroutine
        5 ⮚ List available instructions
        6 ⮚ Main Menu
        0 ⮚ Quit

        """;
        // Indexed Method names -- should match the order of above options
        // TODO: make this dynamic
        public string[] runMthdOptions = [
             "Quit",
             "Run",
             "Pop",
             "Clear",
             "Save",
             "ListHotkeys",
             "MainLoop"
       ];

        private Dictionary<string, string> configMap = new Dictionary<string, string>();

        // Class for loading and storing data files
        private static DataLoader dataLoader = new DataLoader("default");

        // StringBuilders for user input
        private StringBuilder userCode = new StringBuilder();

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

        static void InterpretWingDingCode(string userCode)
        {
            // Parse any subroutines
            string parsedCode = dataLoader.ParseSubroutines(userCode);

            var interpreter = new Runner();

            interpreter.LoadInstructionMap(dataLoader.wingDingsToInstructions);

            interpreter.Run(parsedCode, userCode);
        }

        public void Save()
        {
            dataLoader.SaveSubroutine(userCode.ToString());
        }
        public void ListHotkeys()
        {
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

        public void Run()
        { // Run the current code
            InterpretWingDingCode(userCode.ToString());
            UserOpts.PressAnyKey();
        }

        public void Clear()
        {
            /*
                Clears the current userCode
            */ 
            userCode.Clear();
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
                string newCode = string.Join(FileHelper.INSTRUCTION_DELIM, instructions.Take(instructions.Length - 2))+FileHelper.INSTRUCTION_DELIM;
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
            Console.WriteLine(FileHelper.WING_DING_FORK);
            Console.WriteLine("{0} Enter config name: ", FileHelper.USER_INPUT_ARROW);
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
            userCode = new StringBuilder();
            foreach (char c in bfCode)
            {
                string instruction = dataLoader.GetDing(c.ToString()) + FileHelper.INSTRUCTION_DELIM;
                userCode.Append(instruction);
            }
        }

        public void ConvertText()
        {
            Console.WriteLine(FileHelper.WING_DING_FORK);
            Console.Write("Enter text to convert: ");

            // Get the text to convert and update UserCode
            string inputText = Console.ReadLine();
            string convertedBFCode = BFConverter.ConvertTextToBF(inputText);
            UpdateUserCodeFromBF(convertedBFCode); 

            Console.WriteLine("Updated code to:\n {0}", userCode.ToString());
            UserOpts.PressAnyKey("\nPress any key to Run...\n");

            // Start the run loop with the new code
            RunLoop();
        }

        public void PasteCode()
        {
            /*
                Get WingDing code from a pasted string
            */
            Console.WriteLine(FileHelper.WING_DING_FORK);
            Console.WriteLine("\n*code must be | delimited wingdings*\nPaste your code, then hit enter:\n");
            string pastedCode = Console.ReadLine();

            // TODO: Parse/sanitize pasted code for available instructions
            //       Allow for non-delmited code or code in keybindings (+-^<>... etc)
            userCode = new StringBuilder(pastedCode);
            RunLoop();
        }

        public void LoadCode()
        {
            Console.Write(FileHelper.WING_DING_FORK);
            Console.Write("Enter file path: ");

            string codeFilepath = Console.ReadLine();
            if (!File.Exists(codeFilepath))
            {
                UserOpts.PressAnyKey(String.Format("\nFile {0} does not exist.\nPress any key to continue...", codeFilepath));
            }

            string bfCode = File.ReadAllText(codeFilepath);
            UpdateUserCodeFromBF(bfCode); 

            Console.WriteLine("Updated code to:\n {0}", userCode.ToString());
            UserOpts.PressAnyKey("\nPress any key to Run...\n");

            // Start the run loop with the new code
            RunLoop();
        }


        public void MainLoop()
        {

            // Load config.yml (fake yml read for now)
            string configPath = String.Format("{0}/config.yml", DataLoader.dataDirectory);

            configMap = FileHelper.ParseYAML(configPath);

            if (configMap.ContainsKey("dataConfigName"))
            {
                dataLoader = new DataLoader(configMap["dataConfigName"]);
            }

            while (true)
            {
                // StringBuilder for 
                StringBuilder sbDingFork = new StringBuilder(FileHelper.WING_DING_FORK);
                sbDingFork.Append(String.Format(MAINLOOP_HEADER, dataLoader.dataConfigName));

                Console.WriteLine(sbDingFork);

                Console.Write(FileHelper.USER_INPUT_ARROW);
                string userKey = Console.ReadKey().KeyChar.ToString();
                Console.Clear();

                // If the user entered an available option [0..mthdOptions.Length]
                if (int.TryParse(userKey, out int option))
                {
                    if (option <= mainMthdOptions.Length)
                    {
                        try
                        {
                            // Invoke the option method
                            GetType().GetMethod(mainMthdOptions[option])?.Invoke(this, []);
                        }
                        catch (Exception e)
                        {
                            UserOpts.PressAnyKey(String.Format("\nOption {0} failed with:\n{1}\n\nPress any key to continue...", option, e.ToString()));
                        }
                    }
                }
                else
                {
                    Console.WriteLine("{0} is not a recognized option", userKey);
                }
            }
        }

        public void RunLoop()
        {
            /*
                Loop for running WingDing code

                   loadedCode: passed in the case of code from the clipboard or a file
            */

            // Refresh userCode and clear the console
            Console.Clear();
            while (true)
            {
                // StringBuilder for 
                StringBuilder sbDingFork = new StringBuilder(FileHelper.WING_DING_FORK);
                sbDingFork.Append(String.Format(RUNLOOP_HEADER, CleanUserCode(userCode.ToString())));

                Console.WriteLine(sbDingFork);

                string userKey = Console.ReadKey().KeyChar.ToString();
                Console.Clear();

                // TODO: validate keymap doesn't contain option menu numeric keys
                // If the user entered an available option [0..mthdOptions.Length]
                if (int.TryParse(userKey, out int option))
                {
                    if (option <= runMthdOptions.Length)
                    {
                        // HACK: Return instead of recursievly calling MainLoop()
                        if (runMthdOptions[option] == "MainLoop") { return; }

                        // Invoke the option method
                        GetType().GetMethod(runMthdOptions[option])?.Invoke(this, []);
                    }
                    else // Not in the options range
                    {
                        Console.WriteLine("{0} is not a recognized option", userKey);
                    }
                }

                string wingDing = dataLoader.GetDing(userKey);

                if (wingDing == "")
                {
                    continue;
                }

                // Use | as delimeter
                // --> certain characters have a Length of 2, ie 👇.Length, 👍
                //  can't iterate one string length at a time and uncertainty of user input length.
                userCode.Append(wingDing + FileHelper.INSTRUCTION_DELIM);

                Console.Clear();
            }
        }

        public void TestLoop()
        {
            /*
                Method for running unit tests
            */

            // TODO: break out unit tests

            /*
            Console.OutputEncoding = Encoding.Unicode;

            while (true)
            {
                string s = Console.ReadLine();

                if (!string.IsNullOrEmpty(s))
                {
                    Debug.WriteLine(s);

                    Console.WriteLine(s);
                }
            }
            */
            /*
            UTF CONVERSION/INPUT TEST:

                Comparing 👇 to user input (ReadLine)
                _____________________________
                Testing: 👇
                Original UTF-16 code units:
                3D D8 47 DC

                Exact number of bytes required: 4
                Maximum number of bytes required: 9

                UTF-8-encoded code units:
                F0 9F 91 87

                _____________________________
                Testing: ??
                Original UTF-16 code units:
                3F 00 3F 00

                Exact number of bytes required: 2
                Maximum number of bytes required: 9

                UTF-8-encoded code units:
                3F 3F
            */
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = Encoding.Unicode;

            Console.WriteLine("""
            === TESTING UTF8 CONVERSION & RENDERING ===
            """);
            Console.WriteLine("Copy and Paste test string:\n👇");
            var utfUserInput = Console.ReadLine();
            string[] inputStrings = ["👇", utfUserInput];
            Console.WriteLine("\nComparing 👇 to user input (ReadLine)");
            foreach (string utfInput in inputStrings)
            {
                Console.WriteLine("\n\n_____________________________");
                Console.WriteLine("Testing: {0}", utfInput);
                // Create a character array.

                char[] chars = utfInput.ToCharArray();

                // Get UTF-8 and UTF-16 encoders.
                Encoding utf8 = Encoding.UTF8;
                Encoding utf16 = Encoding.Unicode;

                // Display the original characters' code units.
                Console.WriteLine("Original UTF-16 code units:");
                byte[] utf16Bytes = utf16.GetBytes(chars);
                foreach (var utf16Byte in utf16Bytes)
                    Console.Write("{0:X2} ", utf16Byte);
                Console.WriteLine();

                // Display the number of bytes required to encode the array.
                int reqBytes = utf8.GetByteCount(chars);
                Console.WriteLine("\nExact number of bytes required: {0}",
                              reqBytes);

                // Display the maximum byte count.
                int maxBytes = utf8.GetMaxByteCount(chars.Length);
                Console.WriteLine("Maximum number of bytes required: {0}\n",
                                  maxBytes);

                // Encode the array of chars.
                byte[] utf8Bytes = utf8.GetBytes(chars);

                // Display all the UTF-8-encoded bytes.
                Console.WriteLine("UTF-8-encoded code units:");
                foreach (var utf8Byte in utf8Bytes)
                    Console.Write("{0:X2} ", utf8Byte);

            }
            // Wait for user input
            UserOpts.PressAnyKey();
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
            // BrainfuckConverter.Translate();
        }
    }

}
