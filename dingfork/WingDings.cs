/*

*/
namespace WingDings
{
    public class WingDingDecoder
    {
        // HACK: get dingfork/ directory
        // --> remove /app/ reference
        // --> remove path to .net folder in debug
        public static string dingforkDirectory = System.AppDomain.CurrentDomain.BaseDirectory.Split("dingfork")[0].Replace("\\app\\", "") + "\\dingfork\\";
        public static string subroutinesDirectory = dingforkDirectory + "/subroutines/";
        public static string dataDirectory = dingforkDirectory + "/data/";

        // wingding -> key
        public Dictionary<string, string> wingDingsToKeys = new Dictionary<string, string>();
        // key -> wingding
        public Dictionary<string, string> keysToWingDings = new Dictionary<string, string>();
        // wingding -> wingding[]
        public Dictionary<string, string> wingDingSubRoutines = new Dictionary<string, string>();

        public void LoadSubRoutines()
        {
            // Parses subroutines/ folder
            string[] files = Directory.GetFiles(subroutinesDirectory, "*", SearchOption.AllDirectories);
            foreach (string f in files)
            {
                string subroutineID = f.Split('/')[^1];
                string subroutineString = File.ReadAllText(f);
                wingDingSubRoutines.Add(subroutineID, subroutineString);
            }
        }

        public void SaveSubroutine(string userCode)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.Write("Saving code: {0}\nEnter subroutine name: ", userCode);
            string subroutineName = Console.ReadKey().KeyChar.ToString();
            string subroutinePath = WingDingDecoder.subroutinesDirectory + subroutineName;
            File.WriteAllText(subroutinePath, userCode.ToString());
            Console.WriteLine("\nCurrent code saved to: subroutines/{0}\n\nPress any key to continue...", subroutineName);
            Console.ReadKey();
        }

        public string GetSubroutine(string id)
        {
            if (wingDingSubRoutines.ContainsKey(id))
            {
                return wingDingSubRoutines[id];
            }
            return "";
        }

        public void LoadDingKeyMap()
        {
            try
            {

                Console.OutputEncoding = System.Text.Encoding.UTF8;

                // HACK: csv to dict for wingdings -> keys
                /* 
                        1. Read keymap CSV
                        2. For each line, trim the whitespace and split on "|"
                        3. Convert the split values to dictionary key/value pairs
                */
                wingDingsToKeys = File.ReadLines(dataDirectory + "keymap.csv").Select(line => line.Replace(" ", "").Split('|')).ToDictionary(line => line[0], line => line[1]);

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
}