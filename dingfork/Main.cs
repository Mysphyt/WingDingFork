// See https://aka.ms/new-console-template for more information

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
            Console.Clear();
            Console.WriteLine("Uh-oh! Unrecognized input.");
            // TODO: write user input and newDing function

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
    }

    public class MainClass
    {

        static void Main()
        {

            WingDings wingDings = new WingDings();

            // Allows unicode characters to be printed to the console 
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Main loop for multiple code entries
            while (true)
            {
                Console.Clear();

                // StringBuilders for user input
                StringBuilder userCode = new StringBuilder();
                StringBuilder wingDingUserCode = new StringBuilder();

                while (true)
                {
                    // StringBuilder for 
                    StringBuilder sbDingFork = new StringBuilder("☺ DingFork ☺");

                    sbDingFork.AppendFormat("\n code ⮚ {0}", wingDingUserCode);

                    Console.WriteLine(sbDingFork);

                    string userKey = Console.ReadKey().KeyChar.ToString();

                    if (userKey == "q")
                    {
                        break;
                    }

                    string wingDing = wingDings.getDing(userKey);

                    if (wingDing == "")
                    {
                        continue;
                    }

                    userCode.Append(userKey);
                    wingDingUserCode.Append(wingDing);

                    Console.Clear();

                    Thread.Sleep(10);

                }
                Console.Clear();

                Console.WriteLine("Running: {0}", wingDingUserCode.ToString());

                Console.Write("\nOutput:\n ");
                var interpreter = new Interpreter(userCode.ToString());

                interpreter.Run();

                string userKey2 = Console.ReadKey().KeyChar.ToString();
                if (userKey2 == "q")
                {
                    break;
                }
            }

        }
    }


}