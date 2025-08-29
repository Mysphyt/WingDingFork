/*
    WingDingFork
*/

using System;
using System.Text;
using System.Security.Permissions;
using System.Threading;
using System.Security.Cryptography;
using System.Windows;
using System.Reflection.Metadata.Ecma335;

namespace dingfork
{
    public class WingDings
    {
        // wingding -> key
        public Dictionary<string, string> wingDingsToKeys = new Dictionary<string, string>();
        // key -> wingding
        public Dictionary<string, string> keysToWingDings = new Dictionary<string, string>();
        // wingding -> wingding[]
        public Dictionary<string, string> wingDingSubRoutines = new Dictionary<string, string>();

        public void LoadSubRoutines()
        {
            // Parses subroutines/ folder
            string[] files = Directory.GetFiles("../../../subroutines/", "*", SearchOption.AllDirectories);
            foreach (string f in files)
            {
                string subroutineID = f.Split('/')[^1];
                string subroutineString = File.ReadAllText(f);
                wingDingSubRoutines.Add(subroutineID, subroutineString);
            }
        }

        public string GetSubroutine(string id)
        {
            if (wingDingSubRoutines.ContainsKey(id)) {
                return wingDingSubRoutines[id];
            }
            return "";
        }

        public void LoadDingKeyMap()
        {
            try
            {
                // Hacky csv to dict for wingdings -> keys
                wingDingsToKeys = File.ReadLines("../../../data/keymap.csv").Select(line => line.Split('|')).ToDictionary(line => line[0], line => line[1]);
                foreach (var wingKey in wingDingsToKeys)
                {
                    if (!keysToWingDings.ContainsKey(wingKey.Value))
                    {
                        keysToWingDings.Add(wingKey.Value, wingKey.Key);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error loading dingfork/data/keymap.csv, got: {0}\nPress any key to quit.", e.ToString());
                // Enter any key        
                Console.ReadKey();
                // Exit the program
                System.Environment.Exit(1);
            }
        }

        public string getDing(string wing)
        {
            if (keysToWingDings.ContainsKey(wing))
            {
                return keysToWingDings[wing];
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
            // Log the final code
            Console.WriteLine("\nRunning: {0}", input);
            this.input = input.ToCharArray();
            tape = new byte[30000];
        }

        public void Run(Dictionary<string, string> keymap)
        {
            try
            {
                var unmatchedBracketCounter = 0;
                for (int i = 0; i < input.Length; i++)
                {
                    string instruction = input[i].ToString();
                    if (!keymap.ContainsKey(instruction)) { continue; }
                    if (">" == keymap[instruction])
                    {
                        pointer++;
                    }
                    else if ("<" == keymap[instruction])
                    {
                        pointer--;
                    }
                    else if ("+" == keymap[instruction])
                    {
                        tape[pointer]++;
                    }
                    else if ("-" == keymap[instruction])
                    {
                        tape[pointer]--;
                    }
                    else if ("." == keymap[instruction])
                    {
                        Console.Write(Convert.ToChar(tape[pointer]));
                    }
                    else if ("," == keymap[instruction])
                    {
                        var key = Console.ReadKey();
                        tape[pointer] = (byte)key.KeyChar;
                    }
                    else if ("[" == keymap[instruction])
                    {
                        if (tape[pointer] == 0)
                        {
                            unmatchedBracketCounter++;
                            while ("]" != keymap[instruction] || unmatchedBracketCounter != 0)
                            {
                                i++;

                                if ("[" == keymap[instruction])
                                {
                                    unmatchedBracketCounter++;
                                }
                                else if ("]" == keymap[instruction])
                                {
                                    unmatchedBracketCounter--;
                                }
                            }
                        }
                    }
                    else if ("]" == keymap[instruction])
                    {
                        if (tape[pointer] != 0)
                        {
                            unmatchedBracketCounter++;
                            while ("[" != keymap[instruction] || unmatchedBracketCounter != 0)
                            {
                                i--;

                                if ("]" == keymap[instruction])
                                {
                                    unmatchedBracketCounter++;
                                }
                                else if ("[" == keymap[instruction])
                                {
                                    unmatchedBracketCounter--;
                                }
                            }
                        }
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

        private static WingDings wingDings = new WingDings();

        static string ParseSubroutines(string userCode)
        {
            foreach (var subroutine in wingDings.wingDingSubRoutines)
            {
                string subroutineWingDing = subroutine.Key;
                string subroutineCode = subroutine.Value;
                userCode.Replace(subroutineWingDing, subroutineCode);
            }
            return userCode;
        }

        static void RunWingDingCode(string userCode)
        {
            // Parse any subroutines
            // string parsedCode = ParseSubroutines(userCode.ToString());


            Console.Write("\nOutput:\n ");
            var interpreter = new Interpreter(userCode.ToString());
            interpreter.Run(wingDings.wingDingsToKeys);
        }


        static void MainLoop()
        {

            // Load the data/keymap.csv file
            wingDings.LoadDingKeyMap();

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

                sbDingFork.AppendFormat(INTERFACE_STRING, userCode);

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
                    RunWingDingCode(userCode.ToString());

                    userCode.Clear();

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


                string wingDing = wingDings.getDing(userKey);

                if (wingDing == "")
                {
                    continue;
                }

                userCode.Append(wingDing);

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
            //WingDings wingDings = new WingDings();
            //wingDings.LoadSubRoutines();
        }
    }


}
