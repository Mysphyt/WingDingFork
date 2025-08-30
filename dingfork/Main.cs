/*
    WingDingFork
*/

using System.Text;
using Interpreter;
using Data;

namespace dingfork
{
    public class MainClass
    {

        // TODO: make headers

        const string WINGDINGFORK_HEADER = """

        __        ___             ____  _             _____          _    
        \ \      / (_)_ __   __ _|  _ \(_)_ __   __ _|  ___|__  _ __| | __
         \ \ /\ / /| | '_ \ / _` | | | | | '_ \ / _` | |_ / _ \| '__| |/ /
          \ V  V / | | | | | (_| | |_| | | | | | (_| |  _| (_) | |  |   < 
           \_/\_/  |_|_| |_|\__, |____/|_|_| |_|\__, |_|  \___/|_|  |_|\_\
                            |___/               |___/                     

        """;
        /*
            TODO: validate that loaded keymap does not overlap this instruction set
        */
        const string INTERFACE_STRING = """

        code ⮚ {0}

        <r>: Run [code]
        <d>: Delete last instruction
        <x>: Clear all instructions
        <s>: Save as subroutine
        <q>: Quit

        """;

        private static DataLoader dataLoader = new DataLoader();

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

        static void RunWingDingCode(string userCode)
        {
            // Parse any subroutines
            string parsedCode = ParseSubroutines(userCode);

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var interpreter = new Runner();

            interpreter.LoadInstructionMap(dataLoader.instructionMap);

            interpreter.Run(dataLoader.wingDingsToKeys, parsedCode);
        }

        static void MainLoop()
        {

            // Allows unicode characters to be printed to the console 
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Main loop for multiple code entries
            Console.Clear();

            // StringBuilders for user input
            StringBuilder userCode = new StringBuilder();

            while (true)
            {
                // StringBuilder for 
                StringBuilder sbDingFork = new StringBuilder(WINGDINGFORK_HEADER);

                sbDingFork.AppendFormat(INTERFACE_STRING, CleanUserCode(userCode.ToString()));

                Console.WriteLine(sbDingFork);

                string userKey = Console.ReadKey().KeyChar.ToString();
                Console.Clear();

                if (userKey == "s") // Save as a new subroutine
                {
                    dataLoader.SaveSubroutine(CleanUserCode(userCode.ToString()));
                }

                else if (userKey == "q")
                { // quit the program
                    System.Environment.Exit(1);
                }

                // TODO: clean up messy control flow
                else if (userKey == "r")
                { // Run the current code
                    RunWingDingCode(userCode.ToString());

                    userCode.Clear();

                    Console.Write("\n\nNew WingDing? (y/n): ");

                    // User decides to exit or run another program
                    while (true)
                    {
                        string optKey = Console.ReadKey().KeyChar.ToString().ToLower();

                        if (optKey == "y")
                        {
                            break;
                        }
                        else if (optKey == "n")
                        {
                            System.Environment.Exit(1);
                        }
                        else
                        {
                            Console.Write("\nInvalid Key - please enter (y/n): ");
                        }
                    }
                    Console.Clear();
                }

                if (userKey == "x") // Clear the current code
                {
                    userCode.Clear();
                }
                else if (userKey == "d") // Delete one character
                {
                    if (userCode.Length == 0) { continue; } // Nothing to remove

                    // Trim kerning whitespace
                    while (userCode[userCode.Length - 1] == ' ')
                    {
                        userCode.Remove(userCode.Length - 1, 1);
                    }
                    // Remove the wingding
                    userCode.Remove(userCode.Length - 1, 1);
                }


                string wingDing = dataLoader.getDing(userKey);

                if (wingDing == "")
                {
                    continue;
                }

                // Use | as delimeter
                // --> certain characters have a Length of 2, ie 👇.Length,
                //  can't iterate one string length at a time and uncertainty of user input length.
                userCode.Append(wingDing+"|");

                Console.Clear();
            }
        }

        static void Main()
        {
            /*
                TODO: 
                    - replace ReadKey with async key events
            */

            MainLoop();
            //WingDings dataLoader = new WingDings();
            //Console.OutputEncoding = System.Text.Encoding.UTF8;

            // string testString = "👉︎👉︎👉︎👉︎";
            // char testChar = testString[0];
            // string testChar = testString.Substring(0, 2);
            // Console.WriteLine(testString + "|" +  testChar + "|" + testChar.ToString());
            // Console.WriteLine("👉︎👉︎👉︎👉".Substring(4, 2));
        }
    }


}
