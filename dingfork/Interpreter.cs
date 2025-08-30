/*
    WingDingFork
*/
using System.Reflection;
using System.Diagnostics;
using System.Text;

namespace Interpreter
{
    public class Runner
    {
        public byte[] tape;
        public int pointer;
        public int unmatchedBracketCounter = 0;
        public string[] instructions;

        StringBuilder output = new StringBuilder();

        public int i = 0;

        Dictionary<string, string> instructionMap = new Dictionary<string, string>();

        public Runner()
        {
            tape = new byte[60000];
        }

        public void LoadInstructionMap(string filePath)
        {
            instructionMap = File.ReadLines(filePath + "instructionmap.csv").Select(line => line.Replace(" ", "").Split('|')).ToDictionary(line => line[0], line => line[1]);
        }

        public void inc_data()
        {
            pointer++;
        }

        public void dec_data()
        {
            pointer--;
        }

        public void inc_byte()
        {
            tape[pointer]++;
        }

        public void dec_byte()
        {
            tape[pointer]--;
        }

        public void out_byte()
        {
            output.Append(Convert.ToChar(tape[pointer]));
        }

        public void inp_byte()
        {
            var key = Console.ReadKey();
            tape[pointer] = (byte)key.KeyChar;
        }

        public void loop_bgn()
        {
            if (tape[pointer] == 0)
            {
                unmatchedBracketCounter++;
                while (instructionMap[instructions[i]] != "loop_end" || unmatchedBracketCounter != 0)
                {
                    i++;

                    if (instructionMap[instructions[i]] == "loop_bgn")
                    {
                        unmatchedBracketCounter++;
                    }
                    else if (instructionMap[instructions[i]] == "loop_end")
                    {
                        unmatchedBracketCounter--;
                    }
                }
            }

        }

        public void loop_end()
        {
            if (tape[pointer] != 0)
            {
                unmatchedBracketCounter++;
                while (instructionMap[instructions[i]] != "loop_bgn" || unmatchedBracketCounter != 0)
                {
                    i--;

                    if (instructionMap[instructions[i]] == "loop_end")
                    {
                        unmatchedBracketCounter++;
                    }
                    else if (instructionMap[instructions[i]] == "loop_bgn")
                    {
                        unmatchedBracketCounter--;
                    }
                }
            }
        }

        public void cls_tape()
        {
            unmatchedBracketCounter = 0;
            tape = new byte[6000];
        }


        public void Run(Dictionary<string, string> keymap, string input)
        {
            /*
                TODO: update to validate keymap value set contains required 8 instructions
            */

            //...

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Trim whitespace
            input = input.Replace(" ", "");
            Type thisType = this.GetType();

            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;

                // Remove whitespace in input string
                input = input.Remove(input.Length - 1).Replace(" ", "");

                instructions = input.Split("|");

                // HACK: remove the trailing empty instruction
                // foreach (var instruction in instructionMap) { Console.Write("* "+instruction.Key); }

                Console.WriteLine("Running Program... \n");
                for (i = 0; i < instructions.Length; i++)
                {
                    string instruction = instructions[i];
                    // Console.WriteLine(instruction+i);

                    if (!instructionMap.ContainsKey(instruction))
                    {
                        Console.WriteLine("{-1}  !", instruction);
                        continue;
                    }
                    // DEBUG
                    // Console.WriteLine("Running: " + instruction + " = " + pointer + " ~ " + i);

                    // Parse the method name to MethodInfo
                    MethodInfo theMethod = thisType.GetMethod(instructionMap[instruction]);

                    // Invoke the instruction
                    theMethod.Invoke(this, []);

                    stopwatch.Stop();
                    float elapsed_time = stopwatch.ElapsedMilliseconds;

                    if (elapsed_time > 5000) // Kill the program after 10 seconds, assume infinite loop
                    {
                        throw new Exception("Uh-oh! Infinite loop detected... program took longer than 10 seconds to execute");
                    }

                    stopwatch.Start();
                }
                Console.WriteLine("Output: {0}: ", output.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("\nError in [code]: {0}\n", e.ToString());
            }
        }

    }


}
