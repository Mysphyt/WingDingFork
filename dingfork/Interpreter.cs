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

        public int globalInstructionIt = 0;

        // Wingding --> instruction method name
        // HACK: copy of data map for now
        Dictionary<string, string> instructionMthdMap = new Dictionary<string, string>();

        // HACK: Hardcoded for now -- base instruction set
        string[] availInstructions = [
             "inc_data"
            ,"dec_data"
            ,"inc_byte"
            ,"dec_byte"
            ,"out_byte"
            ,"inp_byte"
            ,"loop_bgn"
            ,"loop_end"
            ,"cls_tape"
        ];
        
        public Runner()
        {
            tape = new byte[60000];
        }

        public void LoadInstructionMap(Dictionary<string, string> strInstructionMap)
        {
            // Copy of data.instructionMap for now
            instructionMthdMap = strInstructionMap;
        }

        // TODO: break out instructions

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
                while (instructionMthdMap[instructions[globalInstructionIt]] != "loop_bgn"|| unmatchedBracketCounter != 0)
                {
                    globalInstructionIt++;

                    if (instructionMthdMap[instructions[globalInstructionIt]] == "loop_bgn")
                    {
                        unmatchedBracketCounter++;
                    }
                    else if (instructionMthdMap[instructions[globalInstructionIt]] == "loop_end")
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
                while (instructionMthdMap[instructions[globalInstructionIt]] != "loop_bgn" || unmatchedBracketCounter != 0)
                {
                    globalInstructionIt--;

                    if (instructionMthdMap[instructions[globalInstructionIt]] == "loop_end")
                    {
                        unmatchedBracketCounter++;
                    }
                    else if (instructionMthdMap[instructions[globalInstructionIt]] == "loop_bgn")
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

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Remove whitespace in input string
            input = input.Remove(input.Length - 1).Replace(" ", "");

            try
            {
                instructions = input.Split("|");

                // Nothing to do
                if (instructions.Length == 0) { return; }

                // Time in seconds until the program is killed
                int timeout = 3;

                Console.WriteLine("\nRunning Program... \n\n[timeout: {0}s]\n\n", timeout);

                for (globalInstructionIt = 0; globalInstructionIt < instructions.Length; globalInstructionIt++)
                {
                    // Log current runtime
                    // Console.WriteLine("[{0}s] Running Program... \n", stopwatch.ElapsedMilliseconds / 1000);

                    string instruction = instructions[globalInstructionIt];
                    // Console.WriteLine(instruction+i);

                    if (!instructionMthdMap.ContainsKey(instruction))
                    {
                        Console.WriteLine("{0}  !", instruction);
                        continue;
                    }

                    // Check against avail instructions
                    if (!availInstructions.Contains(instructionMthdMap[instruction]))
                    {
                        Console.WriteLine(instruction);
                        Console.ReadKey();
                        continue;
                    }
                    // Call the instruction method
                    GetType().GetMethod(instructionMthdMap[instruction]).Invoke(this, []);

                    float elapsed_time = stopwatch.ElapsedMilliseconds;
                    if (elapsed_time > timeout * 1000) // Kill the program after 10 seconds, assume infinite loop
                    {
                        throw new Exception(String.Format("Uh-oh! Infinite loop detected... program took longer than 3 seconds to execute\nFailed at: {1}  |  {2}", instruction, globalInstructionIt));
                    }
                }
                Console.WriteLine("=================== Output =================== \n\n{0}\n\n==============================================\n", output.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("\nError in [code]: {0}\n", e.ToString());
            }
            finally
            {
                stopwatch.Stop();
            }
        }

    }


}
