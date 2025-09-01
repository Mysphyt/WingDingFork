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

        // TODO: make headers

        const string MAINLOOP_HEADER = """

        __        ___             ____  _             _____          _    
        \ \      / (_)_ __   __ _|  _ \(_)_ __   __ _|  ___|__  _ __| | __
         \ \ /\ / /| | '_ \ / _` | | | | | '_ \ / _` | |_ / _ \| '__| |/ /
          \ V  V / | | | | | (_| | |_| | | | | | (_| |  _| (_) | |  |   < 
           \_/\_/  |_|_| |_|\__, |____/|_|_| |_|\__, |_|  \___/|_|  |_|\_\
                            |___/               |___/                     

            Loaded configuration: {0}

            <1>: New WingDing
            <2>: Change configuration
            <3>: Configuration info
            <4>: run TestLoop
            <0>: Exit

        """;
        // Indexed Method names -- should match the order of above options
        // TODO: make this dynamic
        public string[] mainMthdOptions = ["Quit", "RunLoop", "ChangeConfig", "PrintConfig", "TestLoop"];


        /*
            TODO: validate that loaded keymap does not overlap this instruction set
        */
        const string RUNLOOP_HEADER = """

        code ⮚ {0}

        <1>: Run [code]
        <2>: Delete last instruction
        <3>: Clear all instructions
        <4>: Save as subroutine
        <5>: Main Menu
        <0>: Quit

        """;
        // Indexed Method names -- should match the order of above options
        // TODO: make this dynamic
        public string[] runMthdOptions = ["Quit", "Run", "Pop", "Clear", "Save", "MainLoop"];

        private Dictionary<string, string> configMap = new Dictionary<string, string>();

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
                    subroutineCode = subroutine.Value.Replace("  ", " ").Replace("   ", " ").Replace(" ", "|");

                    userCode = userCode.Replace(subroutineWingDing, subroutineCode);
                }
            }
            return userCode;
        }

        private static string CleanUserCode(string userCode)
        {
            string cleanUserCode = userCode;

            cleanUserCode = cleanUserCode.Replace("|", " ");

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

        // TODO: clean up messy control flow
        public void Run()
        { // Run the current code
            InterpretWingDingCode(userCode.ToString());

            userCode.Clear();

            Console.Write("\n\nNew WingDing? (y/n): ");

            bool yesNo = UserOpts.YesNoOpt();

            // User decides to exit or run another program
            if (yesNo)
            {
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
            Console.WriteLine("Enter new config name: ");
            string newConfig = Console.ReadLine();
            // TODO: validate the new config
            dataLoader = new DataLoader(newConfig);
        }

        public void PrintConfig()
        {
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
                StringBuilder sbDingFork = new StringBuilder(String.Format(MAINLOOP_HEADER, dataLoader.dataConfigName));

                Console.WriteLine(sbDingFork);

                string userKey = Console.ReadKey().KeyChar.ToString();
                Console.Clear();

                // If the user entered an available option [0..mthdOptions.Length]
                if (int.TryParse(userKey, out int option))
                {
                    if (option <= mainMthdOptions.Length)
                    {
                        // Invoke the option method
                        GetType().GetMethod(mainMthdOptions[option]).Invoke(this, []);
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

            // Main loop for multiple code entries
            Console.Clear();

            while (true)
            {
                // StringBuilder for 
                StringBuilder sbDingFork = new StringBuilder(String.Format(RUNLOOP_HEADER, CleanUserCode(userCode.ToString())));

                Console.WriteLine(sbDingFork);

                string userKey = Console.ReadKey().KeyChar.ToString();
                Console.Clear();

                // If the user entered an available option [0..mthdOptions.Length]
                if (int.TryParse(userKey, out int option))
                {
                    if (option <= runMthdOptions.Length)
                    {
                        // HACK: Return instead of recursievly calling MainLoop()
                        if (runMthdOptions[option] == "MainLoop") { return; }

                        // Invoke the option method
                        GetType().GetMethod(runMthdOptions[option]).Invoke(this, []);
                    }
                }
                else
                {
                    Console.WriteLine("{0} is not a recognized option", userKey);
                }

                string wingDing = dataLoader.GetDing(userKey);

                if (wingDing == "")
                {
                    continue;
                }

                // Use | as delimeter
                // --> certain characters have a Length of 2, ie 👇.Length, 👍
                //  can't iterate one string length at a time and uncertainty of user input length.
                userCode.Append(wingDing + "|");

                Console.Clear();
            }
        }

        public void TestLoop()
        {
            // Create a character array.
            string gkNumber = Char.ConvertFromUtf32(0x10154);
            char[] chars = new char[] { 'z', 'a', '\u0306', '\u01FD', '\u03B2',
                                  gkNumber[0], gkNumber[1] };

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
            Console.WriteLine();

            Console.WriteLine("\nPress any key to continue... ");
            Console.ReadKey();
        }
    }



    class MainClass // Runs DingFork.MainLoop
    {
        static void Main()
        {
            // Allows unicode characters to be printed to the console 
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            // Start the Main program loop
            DingFork df = new DingFork();

            df.MainLoop();
        }
    }

}
