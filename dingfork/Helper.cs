
namespace Helper
{
    public static class FileHelper
    {
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