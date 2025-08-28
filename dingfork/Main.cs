/*
    WingDingFork

    Next:
        * Clean up TODOs
        
        * Update to use the WingDings as the actual instructions in code
            - Create mapping file instead of hard-coding instruction dict

                [keymap.csv]
                    "]", "🗀  "
                    "[", "🗁  "
                    "<", "👈︎ "
                    ">", "👉︎ "
                    ".", "🖳  "
                    ",", "✂  "
                    "+", "👆︎ "
                    "-", "👇︎ "
            
        * Add sub-routines as text files with WingDing names
            - Add ability to save current code as a new sub-routine

                WingDingFork/subroutines/
                    🕿.txt
                    🖏.txt
                    
                [🕿.txt]
                    >++++++++[<+++++++++>-]<.
                    >++++[<+++++++>-]<+.
                    +++++++..
                    +++.
                    >>++++++[<+++++++>-]<++.
                    ------------.>++++++[<+++++++++>-]<+.
                    <.+++.------.
                    --------.
                    >>>++++[<++++++++>-]<+.
            
*/

using System;
using System.Text;
using System.Security.Permissions;
using System.Threading;
using System.Security.Cryptography;
using System.Windows;

namespace dingfork
{
    public class WingDings
    {
        public Dictionary<string, string> wingDingDict = new Dictionary<string, string>()
        {
            // Includes " " kerning
            { "]", "🗀  "},
            {"[", "🗁  "},
            {"<", "👈︎ "},
            {">", "👉︎ "},
            {".", "🖳  "},
            {",", "✂  "},
            {"+", "👆︎ "},
            {"-", "👇︎ "}
        };

        public string getDing(string wing)
        {
            if (wingDingDict.ContainsKey(wing))
            {
                return wingDingDict[wing];
            }

            return "";
        }
    }

    public class Interpreter
    {
        public byte[] tape;
        public int pointer;
        public char[] input;

        public Interpreter(string input)
        {
            this.input = input.ToCharArray();
            tape = new byte[30000];
        }

        public void Run()
        {
            try
            {
                var unmatchedBracketCounter = 0;
                for (int i = 0; i < input.Length; i++)
                {
                    switch (input[i])
                    {
                        case '>':
                            pointer++;
                            break;
                        case '<':
                            pointer--;
                            break;
                        case '+':
                            tape[pointer]++;
                            break;
                        case '-':
                            tape[pointer]--;
                            break;
                        case '.':
                            Console.Write(Convert.ToChar(tape[pointer]));
                            break;
                        case ',':
                            var key = Console.ReadKey();
                            tape[pointer] = (byte)key.KeyChar;
                            break;
                        case '[':
                            if (tape[pointer] == 0)
                            {
                                unmatchedBracketCounter++;
                                while (input[i] != ']' || unmatchedBracketCounter != 0)
                                {
                                    i++;

                                    if (input[i] == '[')
                                    {
                                        unmatchedBracketCounter++;
                                    }
                                    else if (input[i] == ']')
                                    {
                                        unmatchedBracketCounter--;
                                    }
                                }
                            }
                            break;
                        case ']':
                            if (tape[pointer] != 0)
                            {
                                unmatchedBracketCounter++;
                                while (input[i] != '[' || unmatchedBracketCounter != 0)
                                {
                                    i--;

                                    if (input[i] == ']')
                                    {
                                        unmatchedBracketCounter++;
                                    }
                                    else if (input[i] == '[')
                                    {
                                        unmatchedBracketCounter--;
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nError in [code]: {0}\n", e.ToString());
            }
        }
        
    }

    public class MainClass
    {

        // TODO: make headers

        const string WINGDINGFORK_HEADER = """
        👇︎ 👆︎ ✂  🖳  👈︎ 👉︎ 🗁  🗀   WingDingFork 🗀  🗁  👈︎ 👉︎ 🖳  ✂  👆︎ 👇︎ 
        """;

        const string INTERFACE_STRING = """


        code ⮚ {0}

        <r>: Run [code]
        <d>: Delete last instruction
        <x>: Clear all instructions
        <q>: Quit

        """;


        static void RunWingDingCode(string wingDingUserCode, string userCode)
        {

            Console.WriteLine("\nRunning: {0}", wingDingUserCode.ToString());

            Console.Write("\nOutput:\n ");
            var interpreter = new Interpreter(userCode.ToString());

            interpreter.Run();
        }

        static void Main()
        {
            /*
                TODO: 
                    - replace ReadKey with async key events
            */

            WingDings wingDings = new WingDings();

            // Allows unicode characters to be printed to the console 
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Main loop for multiple code entries
            Console.Clear();

            // StringBuilders for user input
            StringBuilder userCode = new StringBuilder();
            StringBuilder wingDingUserCode = new StringBuilder();

            while (true)
            {
                // StringBuilder for 
                StringBuilder sbDingFork = new StringBuilder(WINGDINGFORK_HEADER);

                sbDingFork.AppendFormat(INTERFACE_STRING, wingDingUserCode);

                Console.WriteLine(sbDingFork);

                string userKey = Console.ReadKey().KeyChar.ToString();
                Console.Clear();

                if (userKey == "q")
                { // quit the program
                    System.Environment.Exit(1);
                }

                // TODO: clean up messy control flow
                if (userKey == "r")
                { // Run the current code
                    RunWingDingCode(wingDingUserCode.ToString(), userCode.ToString());

                    userCode.Clear();
                    wingDingUserCode.Clear();

                    Console.WriteLine("\n\nNew WingDing? (Y/N)\n");

                    string optKey = Console.ReadKey().KeyChar.ToString();
                    while (true)
                    {
                        if (optKey == "y")
                        {
                            break;
                        }
                        else if (optKey == "n")
                        {
                            System.Environment.Exit(1);
                        }
                    }
                }

                if (userKey == "x") // Clear the current code
                {
                    userCode.Clear();
                    wingDingUserCode.Clear();
                }
                else if (userKey == "d") // Delete one character
                {
                    if (userCode.Length == 0) { continue; } // Nothing to remove

                    userCode.Remove(userCode.Length - 1, 1);

                    // Hacky, need to re-render the WingDingUserCode due to kerning
                    wingDingUserCode.Clear();
                    foreach (char c in userCode.ToString()) {
                        wingDingUserCode.Append(wingDings.getDing(c.ToString()));
                    } 
                    continue;
                }
    
                string wingDing = wingDings.getDing(userKey);

                if (wingDing == "")
                {
                    continue;
                }

                userCode.Append(userKey);
                wingDingUserCode.Append(wingDing);

                Console.Clear();
            }

        }
    }


}
