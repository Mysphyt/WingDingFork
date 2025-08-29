/*
    WingDingFork
*/
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Interpreter
{

    public class Runner
    {
        public byte[] tape;
        public int pointer;

        public Runner()
        {
            tape = new byte[30000];
        }

        public void Run(Dictionary<string, string> keymap, string input)
        {
            /*
                TODO: update to validate keymap value set contains required 8 instructions
            */
            // Trim whitespace
            input = input.Replace(" ", "");

            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;

                /*
                string testString = "👉︎👉︎👉︎👉︎";
                char testChar = testString[0];
                string testString2 = testString.Substring(0, 1);
                Console.WriteLine(testString + "|" +  testString2 + "|" + testChar + "|" + testChar.ToString());
                */

                var unmatchedBracketCounter = 0;

                /*
                foreach (var kvp in keymap)
                {
                    Console.WriteLine(kvp.Key + "|" + kvp.Value);
                }
                */
                // Remove whitespace in input string
                input = input.Remove(input.Length - 1).Replace(" ", "");
                // Console.WriteLine(input);
                // Console.WriteLine(input.Length);
                // Console.WriteLine("👇".Length);
                string[] instructions = input.Split("|");
                // HACK: remove the trailing empty instruction
                
                for (int i = 0; i < instructions.Length; i++)
                {
                    string instruction = instructions[i];

                    if (!keymap.ContainsKey(instruction))
                    {
                        // Console.WriteLine("{0}  !", instruction);
                        continue;
                    }
                    // else { Console.WriteLine("{0}  ~", instruction); }

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
                                instruction = instructions[i];

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
                                instruction = instructions[i];

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


}
