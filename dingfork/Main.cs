/*
    WingDingFork
*/

using System.Diagnostics;
using System.Text;
using Interpreter;
using Data;
using Helper;

namespace dingfork
{
    public class DingFork
    {

        const string MAINLOOP_HEADER = """

            Loaded configuration ⮚ {0}

            1 ⮚ Code New WingDing
            2 ⮚ Paste new WingDing
            3 ⮚ Load WingDing from file
            2 ⮚ Change configuration
            3 ⮚ Configuration info
            0 ⮚ Exit

        """;
        // Indexed Method names -- should match the order of above options
        // TODO: make this dynamic
        public string[] mainMthdOptions = ["Quit", "RunLoop", "PasteCode", "LoadCode","ChangeConfig", "PrintConfig"];


        /*
            TODO: validate that loaded keymap does not overlap this instruction set
        */
        const string RUNLOOP_HEADER = """
        
        code ⮚ {0}

        1 ⮚ Run [code]
        2 ⮚ Delete last instruction
        3 ⮚ Clear all instructions
        4 ⮚ Save as subroutine
        5 ⮚ Main Menu
        0 ⮚ Quit

        """;
        // Indexed Method names -- should match the order of above options
        // TODO: make this dynamic
        public string[] runMthdOptions = ["Quit", "Run", "Pop", "Clear", "Save", "MainLoop"];

        private Dictionary<string, string> configMap = new Dictionary<string, string>();

        // Class for loading and storing data files
        private static DataLoader dataLoader = new DataLoader("default");

        // StringBuilders for user input
        private StringBuilder userCode = new StringBuilder();

        static string ParseSubroutines(string userCode)
        {
            /*
                Parses files in the subroutines directory of the current data config, ie: data/default/subroutines/
                    
                    Expects a single key/value pair in the following format:
                        
                        wingding:🕿
                        code:👇 🐵 👇 👇 👇 👉 👆 👈 🐟 👉 👇 👍

                TODO: Make this a single subroutines file
            */
            string subroutineCode = userCode;
            string prevSubroutineCode = "";

            // While there are still subroutines
            while (subroutineCode != prevSubroutineCode)
            {
                prevSubroutineCode = subroutineCode;
                foreach (var subroutine in dataLoader.wingDingSubRoutines)
                {
                    string subroutineWingDing = subroutine.Key;

                    // HACK: Super hacky way of adding | (or re-adding) delimiters and reducing even and odd number of spaces to a single space
                    // TODO: create static string values for delimiters
                    subroutineCode = subroutine.Value.Replace("  ", " ").Replace("   ", " ").Replace(" ", FileHelper.INSTRUCTION_DELIM);

                    userCode = userCode.Replace(subroutineWingDing, subroutineCode);
                }
            }
            return userCode;
        }

        private static string CleanUserCode(string userCode)
        {
            string cleanUserCode = userCode;

            cleanUserCode = cleanUserCode.Replace(FileHelper.INSTRUCTION_DELIM, " ");

            return cleanUserCode;
        }

        static void InterpretWingDingCode(string userCode)
        {
            // Parse any subroutines
            string parsedCode = ParseSubroutines(userCode);

            var interpreter = new Runner();

            interpreter.LoadInstructionMap(dataLoader.instructionMap);

            interpreter.Run(dataLoader.wingDingsToKeys, parsedCode);
        }

        public void Save()
        {
            dataLoader.SaveSubroutine(CleanUserCode(userCode.ToString()));
        }
        public void Quit()
        { // quit the program
            System.Environment.Exit(1);
        }

        public void Run()
        { // Run the current code
            InterpretWingDingCode(userCode.ToString());

            userCode.Clear();

            Console.Write("\n\nNew WingDing? (y/n): ");

            // User decides to exit or run another program
            if (UserOpts.YesNoOpt())
            {
                Console.Clear();
                return;
            }
            else
            {
                System.Environment.Exit(1);
            }
        }

        public void Clear()
        {
            userCode.Clear();
        }

        public void Pop()
        {
            if (userCode.Length == 0) { return; } // Nothing to remove

            // Trim kerning whitespace
            while (userCode[userCode.Length - 1] == ' ')
            {
                userCode.Remove(userCode.Length - 1, 1);
            }
            // Remove the wingding
            userCode.Remove(userCode.Length - 1, 1);
        }

        public void ChangeConfig()
        {
            /*
                Change the current data configuration 
            */
            Console.WriteLine("Enter config name: ");
            var newConfig = Console.ReadLine();
            if (newConfig == null) { return; }
            // TODO: validate the new config
            // TODO: change the data folder without creating a new DataLoader
            dataLoader = new DataLoader(newConfig);
        }

        public void PrintConfig()
        {
            // TODO: print config debug info
            Console.WriteLine("TODO");
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
            // TODO: Load code from a file then call RunLoop(loadedCode)
            Console.WriteLine("TODO");
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
                        // Invoke the option method
                        GetType().GetMethod(mainMthdOptions[option])?.Invoke(this, []);
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
            Console.WriteLine("\nPress any key to continue... ");
            Console.ReadKey();
        }
    }



    class MainClass // Runs DingFork.MainLoop
    {
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
