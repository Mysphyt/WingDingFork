using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace dingfork
{
    /*
        Namespace for Interpreter classes
    */
    public class Runner
    {
        /*
            WingDing code executor

            TODO: break out Runner class
        */
        public byte[] tape;
        public int pointer;
        public int unmatchedBracketCounter = 0;
        public string[] instructions;
        Stopwatch stopwatch = new Stopwatch();
 
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
            // Stop the stopwatch for user input
            stopwatch.Stop();
            // Get input byte from the user
            Console.Clear();
            FileHelper.printWdfHeader();
            Console.Write("Input byte for instruction# {0}: ", globalInstructionIt);
            var key = Console.ReadKey();
            // Make sure the byte input is numeric
            if (Regex.IsMatch(key.KeyChar.ToString(), @"^\d+$"))
            {
                tape[pointer] = (byte)key.KeyChar;
            }
            else
            {
                UserOpts.PressAnyKey("\nError: cannot set pointer value to non-numeric byte.\nPress any key to continue...");
            }
            // Re-start the stopwatch
            stopwatch.Start();
        }

        public void loop_bgn()
        {
            if (tape[pointer] == 0)
            {
                unmatchedBracketCounter++;
                while (instructionMthdMap[instructions[globalInstructionIt]] != "loop_bgn" || unmatchedBracketCounter != 0)
                {
                    globalInstructionIt++;
                    if (globalInstructionIt >= instructions.Length)
                    {
                        return;   
                    }

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
                    if (globalInstructionIt < 0)
                    {
                        return;   
                    }
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


        public string Run(string parsedCode, string userCode)
        {
            /*
                Method for running wingding code

                Args:
                    input: Code string to interpret and run.

            */

            if (parsedCode == "") { return ""; } // Nothing to run

            // Reset and start the stopwatch
            stopwatch = new Stopwatch();
            stopwatch.Start();

            // Trim whitespace
            parsedCode = parsedCode.Replace(" ", "");

            // Remove whitespace in input string
            parsedCode = parsedCode.Remove(parsedCode.Length - 1).Replace(" ", "");

            try
            {
                instructions = parsedCode.Split(FileHelper.INSTRUCTION_DELIM);

                // Nothing to do
                if (instructions.Length == 0) { return ""; }

                // Time in seconds until the program is killed
                int timeout = 5;

                for (globalInstructionIt = 0; globalInstructionIt < instructions.Length; globalInstructionIt++)
                {
                    // Log current runtime

                    string instruction = instructions[globalInstructionIt];

                    // Make sure this instruction exists
                    if (!instructionMthdMap.ContainsKey(instruction) || !availInstructions.Contains(instructionMthdMap[instruction]))
                    {
                        continue;
                    }

                    // Call the instruction method
                    GetType().GetMethod(instructionMthdMap[instruction]).Invoke(this, []);

                    float elapsed_time = stopwatch.ElapsedMilliseconds;
                    if (elapsed_time > timeout * 1000) // Kill the program after [timeout] seconds, assume infinite loop
                    {
                        throw new Exception(string.Format("Uh-oh! Infinite loop detected... program took longer than 3 seconds to execute\nFailed at: {0} | {1}", instruction, globalInstructionIt));
                    }
                }
                return output.ToString();
            }
            catch (Exception e)
            {
                return e.ToString();  //Console.WriteLine("\nError in code: {0}\n", e.ToString());
            }
            finally
            {
                stopwatch.Stop();
            }
        }

    }


}
