

using System.Data;
using Helper;

namespace Data
{
    public class DataLoader
    {
        // HACK: get dingfork/ directory
        // --> remove /app/ reference
        // --> remove path to .net folder in debug
        public static string dingforkDirectory = System.AppDomain.CurrentDomain.BaseDirectory.Split("dingfork")[0].Replace("\\app\\", "") + "\\dingfork\\";
        public static string dataDirectory = dingforkDirectory + "/data/";
        public static string subroutinesDirectory = dingforkDirectory + "/data/{0}/subroutines/";

        public string dataConfigName;
        public string keymapFile;

        // wingding -> key
        public Dictionary<string, string> wingDingsToKeys = new Dictionary<string, string>();
        // key -> wingding
        public Dictionary<string, string> keysToWingDings = new Dictionary<string, string>();
        // wingding -> wingding[]
        public Dictionary<string, string> wingDingSubRoutines = new Dictionary<string, string>();
        // wingding -> interpreter instruction method name
        public Dictionary<string, string> instructionMap = new Dictionary<string, string>();

        public DataLoader(string _dataConfigName)
        {
            // TODO: break out load methods for each Map
            dataConfigName = _dataConfigName;

            try // Try to load data files
            {
                // Parses subroutines/ folder
                string subroutinesConfigDirectory = String.Format(subroutinesDirectory, dataConfigName);
                string[] files = Directory.GetFiles(subroutinesConfigDirectory, "*", SearchOption.AllDirectories);

                Dictionary<string, string> subroutineArgs = new Dictionary<string, string>();
                // Load subroutines
                foreach (string subroutineFile in files)
                {
                    try
                    {
                        subroutineArgs = FileHelper.ParseYAML(subroutineFile);

                        string subroutineName = subroutineFile.Split('/')[^1];
                        string subroutineWingDing = subroutineArgs["wingding"];
                        string subroutineCode = subroutineArgs["code"];

                        wingDingSubRoutines.Add(subroutineWingDing, subroutineCode);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("\nFailed to parse subroutine: {0}\n\nError: {1}", subroutineFile, e.ToString());
                        continue;
                    }
                }

                // HACK: csv to dict for wingdings -> key^method_name
                // TODO: make this readable
                keymapFile = String.Format("{0}/{1}/keymap", dataDirectory, dataConfigName);

                // Temp dictionary for parsing 
                Dictionary<string, string> tmpWingDingMap = File.ReadLines(keymapFile).Select(line => line.Replace(" ", "").Split('|')).ToDictionary(line => line[0], line => line[1]);

                // Parse the keys and method names from keymap data
                foreach (var keyToMthd in tmpWingDingMap)
                {
                    string[] keyMthd = keyToMthd.Value.Split("^");

                    string keyName = keyMthd[0];
                    string mthdName = keyMthd[1];
                    string wingDing = keyToMthd.Key;

                    instructionMap.Add(wingDing, mthdName);
                    wingDingsToKeys.Add(wingDing, keyName);
                }

                foreach (var wingKey in wingDingsToKeys)
                {
                    if (!keysToWingDings.ContainsKey(wingKey.Value))
                    {
                        keysToWingDings.Add(wingKey.Value.Replace(" ", ""), wingKey.Key.Replace(" ", ""));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error loading dingfork/data/\n got: {0}\nPress any key to quit.", e.ToString());
                // Enter any key        
                Console.ReadKey();
                // Exit the program
                System.Environment.Exit(1);
            }
        }

        public void SaveSubroutine(string userCode)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string subroutineUnformatted = """
            wingding: {0}
            code: {1}
            """;

            string subroutineName = "";
            string subroutineWingDing = "";

            // Get the subroutine name from the user
            Console.Write("Saving code: {0}\nEnter subroutine name: ", userCode);
            subroutineName = Console.ReadLine();

            Console.Write("\nConfirm (y/n) subroutine name: {0} ? ", subroutineName);

            if (!UserOpts.YesNoOpt()) // Get a y/n from the user
            {
                Console.Clear();
                return;
            }

            // Get The subroutine wingding from the user
            Console.Write("\nSaving subroutine: {0}\n\nEnter subroutine WingDing: ", subroutineName);
            subroutineWingDing = Console.ReadLine();

            Console.Write("\nConfirm (y/n) subroutine WingDing: {0} ? ", subroutineWingDing);

            if (!UserOpts.YesNoOpt()) // Get a y/n from the user
            {
                Console.Clear();
                return;
            }

            // Make sure there is an existing key mapping
            if (!wingDingsToKeys.ContainsKey(subroutineName))
            {
                // Create a new mapping
                Console.Write("\nNo existing shortcut found in keymap...\nEnter shortcut key for {0}: ", subroutineName);
                string shortcut = Console.ReadLine();

                using (StreamWriter sw = File.AppendText(keymapFile))
                {
                    sw.WriteLine("\n" + subroutineName + "|" + shortcut + "^sb_instr");
                }
                return;
            }

            // Write the subroutine 
            string subroutinePath = String.Format(subroutinesDirectory, dataConfigName) + subroutineName;
            File.WriteAllText(subroutinePath, String.Format(subroutineWingDing, userCode.ToString()));

            Console.WriteLine("\nCurrent code saved to: subroutines/{0}\n\nPress any key to continue...", subroutineName);
            Console.ReadLine();
        }

        public string GetSubroutine(string id)
        {
            if (wingDingSubRoutines.ContainsKey(id))
            {
                return wingDingSubRoutines[id];
            }
            return "";
        }

      
        public string GetDing(string wing)
        {
            if (keysToWingDings.ContainsKey(wing))
            {
                return keysToWingDings[wing];
            }

            return "";
        }
    }
}