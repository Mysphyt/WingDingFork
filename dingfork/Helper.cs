
namespace Helper
{
    public static class FileHelper
    {
        public static string USER_INPUT_ARROW = " ⮚ ";
        public static string INSTRUCTION_DELIM = "|";

        public static string WING_DING_FORK = """
        __        ___             ____  _             _____          _    
        \ \      / (_)_ __   __ _|  _ \(_)_ __   __ _|  ___|__  _ __| | __
         \ \ /\ / /| | '_ \ / _` | | | | | '_ \ / _` | |_ / _ \| '__| |/ /
          \ V  V / | | | | | (_| | |_| | | | | | (_| |  _| (_) | |  |   < 
           \_/\_/  |_|_| |_|\__, |____/|_|_| |_|\__, |_|  \___/|_|  |_|\_\
                            |___/               |___/                     

        """;
        static public Dictionary<string, string> ParseYAML(string filepath)
        {

            // Simple parser for key value pairs for now
            Dictionary<string, string> ymlDict = new Dictionary<string, string>();

            using (StreamReader reader = new StreamReader(filepath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // ignore lines without a mapping
                    if (!line.Contains(':'))
                    {
                        continue;
                    }
                    string[] kvp = line.Split(":");

                    ymlDict.Add(kvp[0], kvp[1].Replace(" ", ""));
                }

                // Update the data loader config
            }

            return ymlDict;
        }
    }
    // Namespace for 
    static class UserOpts
    {

        static public void PressAnyKey()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        static public bool YesNoOpt()
        {
            while (true)
            {
                string optKey = Console.ReadKey().KeyChar.ToString().ToLower();

                if (optKey == "y")
                {
                    return true;
                }
                else if (optKey == "n")
                {
                    return false;
                }
                else
                {
                    Console.Write("\nInvalid Key - please enter (y/n): ");
                }
            }
        }

    }
}