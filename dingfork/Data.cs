
namespace dingfork
{
    /*
        Namespace for data related classes
    */

    public class DataLoader
    {
        // HACK: get dingfork/ directory
        // --> remove /app/ reference
        // --> remove path to .net folder in debug
        public static string dingforkDirectory = System.AppDomain.CurrentDomain.BaseDirectory.Split("dingfork")[0].Replace("\\app\\", "") + "\\dingfork\\";
        public static string dataDirectory = dingforkDirectory + "/data/";
        public static string subroutinesFile = dingforkDirectory + "/data/{0}/subroutines";


        public string dataConfigName = "";
        public string keymapFile = "";

        // TODO: Clean up / consolidate these mappings

        // wingding -> key
        public Dictionary<string, string> wingDingsToKeys = new Dictionary<string, string>();
        // key -> wingding
        public Dictionary<string, string> keysToWingDings = new Dictionary<string, string>();
        // wingding -> wingding[] instruction list
        public Dictionary<string, string> wingDingsToCode = new Dictionary<string, string>();
        // wingding -> subroutine name
        public Dictionary<string, string> wingDingsToSubroutine = new Dictionary<string, string>();
        // wingding -> interpreter instruction method name
        public Dictionary<string, string> wingDingsToInstructions = new Dictionary<string, string>();
        // instruction -> wingding
        public Dictionary<string, string> instructionsToWingDings = new Dictionary<string, string>();


        public void PrintKeymap()
        {
            /*
                Print current subroutines
            */
            Console.Clear();
            FileHelper.print_wdf_header();
            Console.WriteLine("\nAvailable key mappings from {0}/keymap:", dataConfigName);
            foreach (var keymap in instructionsToWingDings)
            {
                string hotkey = wingDingsToKeys[keymap.Value];
                Console.WriteLine("{0} {1}:\n  wingding: {2}\n  hotkey: {3}\n", FileHelper.USER_INPUT_ARROW, keymap.Key, keymap.Value, hotkey);
            }
            // Wait for any user input
            UserOpts.PressAnyKey();
        }

       
        public void PrintSubroutines()
        {
            /*
                Print current subroutines
            */
            Console.Clear();
            FileHelper.print_wdf_header();
            Console.WriteLine("\nAvailable Subroutines in {0}:", dataConfigName);
            foreach (var subroutine in wingDingsToSubroutine)
            {
                string hotkey = wingDingsToKeys[subroutine.Key];
                Console.WriteLine("{0} {2}\n wingding: {1}\n hotkey: {3}", FileHelper.USER_INPUT_ARROW, subroutine.Key, subroutine.Value, hotkey);
            }
            // Wait for any user input
            UserOpts.PressAnyKey();
        }

        public void LoadKeymap()
        {
            /*
                Loads/refreshes mappings from dingfork/data/{config}/keymap
            */
            keymapFile = String.Format("{0}/{1}/keymap", dataDirectory, dataConfigName);

            // Temp dictionary for parsing 
            var keymapLines = File.ReadLines(keymapFile);

            Dictionary<string, string> tmpWingDingMap = new Dictionary<string, string>();

            // String to track duplicate hotkeys
            string duplicateHotkeys = "";
            
            foreach (var line in keymapLines)
            {
                // Remove spaces and split on the isntruction delimiter
                var args = line.Split(FileHelper.INSTRUCTION_DELIM);
                if (args.Length != 2)
                {
                    continue;
                }
                string wingding = args[0];
                string mapping = args[1];
                if (tmpWingDingMap.ContainsKey(wingding))
                {
                    // Log duplicate hotkey
                    duplicateHotkeys += "\n" + String.Format("{0} : {1}", wingding, mapping);
                    continue;
                }

                tmpWingDingMap.Add(args[0], args[1]);
            }

            // If there were any duplicates
            if (duplicateHotkeys.Length > 0)
            {
                // Display duplicate hotkey information to the user
                UserOpts.PressAnyKey(String.Format("Duplicate hotkeys found: {0} \nPress any key to continue...", duplicateHotkeys));
            }

            // Reset keymap dictionaries
            wingDingsToInstructions = new Dictionary<string, string>();
            instructionsToWingDings = new Dictionary<string, string>();

            wingDingsToKeys = new Dictionary<string, string>();
            keysToWingDings = new Dictionary<string, string>();

            // Parse the keys and method names from keymap data
            foreach (var keyToMthd in tmpWingDingMap)
            {
                string[] keyMthd = keyToMthd.Value.Split("^");

                string keyName = keyMthd[0];
                string mthdName = keyMthd[1];
                string wingDing = keyToMthd.Key;

                instructionsToWingDings[mthdName] = wingDing;
                wingDingsToInstructions[wingDing] = mthdName;
                wingDingsToKeys[wingDing] = keyName;
            }


            foreach (var wingKey in wingDingsToKeys)
            {
                if (!keysToWingDings.ContainsKey(wingKey.Value))
                {
                    // TODO: allow spaces in keymap input
                    keysToWingDings.Add(wingKey.Value, wingKey.Key);
                }
            }

        }

        public void LoadSubroutines()
        {
            /*
                Loads subroutine mappings from keymap and the dingfork/data/{config}/subroutines folder
            */
            if (!File.Exists(String.Format(subroutinesFile, dataConfigName)))
            {
                return;
            }

            // Reset the code and name map for subroutines
            wingDingsToCode = new Dictionary<string, string>();
            wingDingsToSubroutine = new Dictionary<string, string>();

            // Load subroutines
            foreach (string unparsedSubroutine in File.ReadLines(String.Format(subroutinesFile, dataConfigName)))
            {
                try
                {
                    // Index of the first delimiter
                    int delimIndex = unparsedSubroutine.IndexOf(FileHelper.INSTRUCTION_DELIM);
                    // Read subroutine name from the first index of the delimited string
                    string subroutineName = unparsedSubroutine.Substring(0, delimIndex);
                    // Read code from the remainder of the line
                    int codeLength = unparsedSubroutine.Length - (subroutineName.Length + 1);
                    string subroutineCode = unparsedSubroutine.Substring(delimIndex, codeLength);

                    // Look up the wingding for this subroutine
                    string subroutineWingDing = instructionsToWingDings[subroutineName];

                    wingDingsToCode.Add(subroutineWingDing, subroutineCode);
                    wingDingsToSubroutine.Add(subroutineWingDing, subroutineName);
                }
                catch (Exception e)
                {
                    Console.WriteLine("\nFailed to parse subroutine: {0}\n\nError: {1}", String.Format(subroutinesFile, dataConfigName), e.ToString());
                    UserOpts.PressAnyKey();
                    continue;
                }

            }

        }

        public void LoadData()
        {
            /*
                Calls DataLoader load methods
            */
            LoadKeymap();
            LoadSubroutines();
        }
        public string ParseSubroutines(string userCode, bool delimSubroutinesFlag = true)
        {
            /*
                Parses subroutine instructions in userCode
                    
                Args:
                    userCode: Input code that may include subroutines to be rendered/replaced with subroutine code

                TODO: Make this a single subroutines file?
            */
            string subroutineCode = userCode;
            string prevSubroutineCode = "";

            // While there are still subroutines
            while (subroutineCode != prevSubroutineCode)
            {
                prevSubroutineCode = subroutineCode;
                foreach (var subroutine in wingDingsToCode)
                {
                    string subroutineWingDing = subroutine.Key;

                    subroutineCode = subroutine.Value;

                    // Flag to append a cls_tape instruction to subroutines
                    if (delimSubroutinesFlag)
                    {
                        subroutineCode += String.Format("|{0}", instructionsToWingDings["cls_tape"]);
                    }

                    userCode = userCode.Replace(subroutineWingDing, subroutineCode);
                }
            }
            return userCode;
        }
        public DataLoader(string _dataConfigName)
        {
            // TODO: break out load methods for each Map
            dataConfigName = _dataConfigName;

            try // Try to load data files
            {
                // Load subroutines folder
                LoadKeymap();
                LoadSubroutines();
            }
            catch (Exception e)
            {
                Console.WriteLine("!!!!!!!! Error loading subroutines !!!!!!!!\n got: {0}", e.ToString());
                // Wait for user input
                UserOpts.PressAnyKey();
                // Exit the program
                System.Environment.Exit(1);
            }
            try
            {
                // Load keymap file
                LoadKeymap();
            }
            catch (Exception e)
            {
                Console.WriteLine("!!!!!!! Error loading keymap !!!!!!!!\n got: {0}", e.ToString());
                UserOpts.PressAnyKey();
                // Exit the program
                System.Environment.Exit(1);
            }
        }

        public void SaveSubroutine(string userCode)
        {
            /*
                Save a new subroutine 

                args: 
                    userCode: Code (in wingdings) to save.

                TODO:
                    - Accept wingding symbol and key binding args
                    - Use dynamic menus
            */

            string subroutineName = "";
            string subroutineWingDing = "";

            FileHelper.print_wdf_header();

            // Get the subroutine name from the user
            Console.WriteLine("Saving code: {0}", userCode.Replace(FileHelper.INSTRUCTION_DELIM, ""));
            Console.Write("\n\nEnter subroutine name: ");
            subroutineName = Console.ReadLine();

            // Get The subroutine wingding from the user
            Console.Write("\n\nSaving subroutine: {0}\n\nEnter subroutine WingDing: ", subroutineName);
            subroutineWingDing = Console.ReadLine();

            // Make sure there is an existing key mapping
            if (!wingDingsToKeys.ContainsKey(subroutineWingDing))
            {
                // Create a new mapping
                Console.Write("\n\nNo existing shortcut found in keymap...\n");
                // Hacky way of getting a non-restricted shortcut from the user
                string[] restrictedShortcuts = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "^", "|"];
                string shortcut = "";
                while (true)
                {
                    Console.Write("\nEnter shortcut key for {0}: ", subroutineName);
                    // Readline, but only takes the first character. Prevents errors from pasted input when using readkey.
                    shortcut = Console.ReadLine().Substring(0, 1);
                    if (restrictedShortcuts.Contains(shortcut) || keysToWingDings.ContainsKey(shortcut))
                    {
                        Console.WriteLine("{0} is already a keymap or menu option.", shortcut);
                    }
                    else
                    {
                        break; // Accepted shortcut
                    }
                }

                using (StreamWriter sw = File.AppendText(keymapFile))
                {
                    sw.Write("\n" + subroutineWingDing + FileHelper.INSTRUCTION_DELIM + shortcut + "^" + subroutineName);
                }
            }

            // Allow spaces and special characters
            subroutineName = String.Format(@"{0}", subroutineName);

            // Write the subroutine 
            string subroutinePath = String.Format(subroutinesFile, dataConfigName);

            // File.WriteAllText(subroutinePath, userCode.ToString());
            using (StreamWriter sw = File.AppendText(subroutinePath))
            {
                sw.Write("\n" + subroutineName + FileHelper.INSTRUCTION_DELIM + userCode.ToString());
            }

            Console.WriteLine("\n\nCurrent code saved to: subroutines:{0}\n", subroutineName);
            // Wait for user input
            UserOpts.PressAnyKey();

            // Re-load the subroutines and keymap
            LoadData();
        }

        public string GetSubroutine(string id)
        {
            if (wingDingsToCode.ContainsKey(id))
            {
                return wingDingsToCode[id];
            }
            return "";
        }


        public string GetDing(string key)
        {
            if (keysToWingDings.ContainsKey(key))
            {
                return keysToWingDings[key];
            }
            else if (keysToWingDings.ContainsValue(key))
            {
                return key;
            }

            return "";
        }
    }
}