/*
    WingDingFork
*/

using System.Text;
using Interpreter;
using Data;
using Helper;

namespace dingfork
{
    public class DingFork
    {

        // TODO: make headers

        const string WINGDINGFORK_HEADER = """

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
            <0>: Exit

        """;
        /*
            TODO: validate that loaded keymap does not overlap this instruction set
        */
        const string INTERFACE_STRING = """

        code ⮚ {0}

        <1>: Run [code]
        <2>: Delete last instruction
        <3>: Clear all instructions
        <4>: Save as subroutine
        <0>: Quit

        """;
        private Dictionary<string, string> configMap = new Dictionary<string, string>();

        private static DataLoader dataLoader = new DataLoader("default");
        // StringBuilders for user input
        private StringBuilder userCode = new StringBuilder();

        static string ParseSubroutines(string userCode)
        {
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
            string[] mthdOptions = ["Quit", "RunLoop", "ChangeConfig", "PrintConfig"];

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
                StringBuilder sbDingFork = new StringBuilder(String.Format(WINGDINGFORK_HEADER, dataLoader.dataConfigName));

                Console.WriteLine(sbDingFork);

                string userKey = Console.ReadKey().KeyChar.ToString();
                Console.Clear();

                // If the user entered an available option [0..mthdOptions.Length]
                if (int.TryParse(userKey, out int option))
                {
                    if (option <= mthdOptions.Length)
                    {
                        GetType().GetMethod(mthdOptions[option]).Invoke(this, []);
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
            string[] mthdOptions = ["Quit", "Run", "Pop", "Clear","Save" ];
            
            // Main loop for multiple code entries
            Console.Clear();

            while (true)
            {
                // StringBuilder for 
                StringBuilder sbDingFork = new StringBuilder(String.Format(INTERFACE_STRING, CleanUserCode(userCode.ToString())));

                Console.WriteLine(sbDingFork);

                string userKey = Console.ReadKey().KeyChar.ToString();
                Console.Clear();

                // If the user entered an available option [0..mthdOptions.Length]
                if (int.TryParse(userKey, out int option))
                {
                    if (option <= mthdOptions.Length)
                    {
                        GetType().GetMethod(mthdOptions[option]).Invoke(this, []);
                    }
                }
                else
                {
                    Console.WriteLine("{0} is not a recognized option", userKey);
                }
 
                string wingDing = dataLoader.getDing(userKey);

                if (wingDing == "")
                {
                    continue;
                }

                // Use | as delimeter
                // --> certain characters have a Length of 2, ie 👇.Length,
                //  can't iterate one string length at a time and uncertainty of user input length.
                userCode.Append(wingDing + "|");

                Console.Clear();
            }
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
