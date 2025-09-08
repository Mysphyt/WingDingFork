
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
        public Dictionary<string, string> wingDingsToKeys = [];
        // key -> wingding
        public Dictionary<string, string> keysToWingDings = [];
        // wingding -> wingding[] instruction list
        public Dictionary<string, string> wingDingsToCode = [];
        // wingding -> subroutine name
        public Dictionary<string, string> wingDingsToSubroutine = [];
        // wingding -> interpreter instruction method name
        public Dictionary<string, string> wingDingsToInstructions = [];
        // instruction -> wingding
        public Dictionary<string, string> instructionsToWingDings = [];
        // instruction -> hotkey
        public Dictionary<string, string> instructionsToKeys = [];


        public void PrintKeymap()
        {
            /*
                Print current subroutines
            */
            Console.Clear();
            FileHelper.printWdfHeader();
            Console.WriteLine("\nAvailable key mappings from {0}/keymap:", dataConfigName);
            foreach (var keymap in instructionsToWingDings)
            {
                string hotkey = wingDingsToKeys[keymap.Value];
                Console.WriteLine("{0} {1} {2} [{3}]", hotkey, FileHelper.USER_INPUT_ARROW, keymap.Value, keymap.Key);
            }
            // Wait for any user input
            UserOpts.PressAnyKey();
        }

        public void LoadKeymap()
        {
            /*
                Loads/refreshes mappings from dingfork/data/{config}/keymap
            */
            keymapFile = string.Format("{0}/{1}/keymap", dataDirectory, dataConfigName);

            // Temp dictionary for parsing 
            var keymapLines = File.ReadLines(keymapFile);

            Dictionary<string, string> tmpWingDingMap = [];

            // String to track duplicate hotkeys
            string duplicateHotkeys = "";
            
            foreach (var line in keymapLines)
            {
                // Remove spaces and split on the isntruction delimiter
                var args = line.Split(FileHelper.INSTRUCTION_DELIM);
                if (args.Length != 3)
                {
                    // Invalid keymap args
                    continue;
                }
                string wingding = args[0];
                string mapping = args[1] + FileHelper.INSTRUCTION_DELIM + args[2];
                if (tmpWingDingMap.ContainsKey(wingding))
                {
                    // Log duplicate hotkey
                    duplicateHotkeys += "\n" + string.Format("{0} : {1}", wingding, mapping);
                    continue;
                }

                tmpWingDingMap.Add(wingding, mapping);
            }

            // If there were any duplicates
            if (duplicateHotkeys.Length > 0)
            {
                // Display duplicate hotkey information to the user
                UserOpts.PressAnyKey(string.Format("Duplicate hotkeys found: {0} \nPress any key to continue...", duplicateHotkeys));
            }

            // Reset keymap dictionaries
            wingDingsToInstructions = [];
            instructionsToWingDings = [];
            instructionsToKeys = [];
            wingDingsToKeys = [];
            keysToWingDings = [];

            // Parse the keys and method names from keymap data
            foreach (var keyToMthd in tmpWingDingMap)
            {
                string[] keyMthd = keyToMthd.Value.Split(FileHelper.INSTRUCTION_DELIM);

                string keyName = keyMthd[0];
                string mthdName = keyMthd[1];
                string wingDing = keyToMthd.Key;

                instructionsToWingDings[mthdName] = wingDing;
                instructionsToKeys[mthdName] = keyName;
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
            if (!File.Exists(string.Format(subroutinesFile, dataConfigName)))
            {
                return;
            }

            // Reset the code and name map for subroutines
            wingDingsToCode = new Dictionary<string, string>();
            wingDingsToSubroutine = new Dictionary<string, string>();

            // Load subroutines
            foreach (string unparsedSubroutine in File.ReadLines(string.Format(subroutinesFile, dataConfigName)))
            {
                try
                {
                    // Index of the first delimiter
                    int delimIndex = unparsedSubroutine.IndexOf(FileHelper.INSTRUCTION_DELIM);
                    // Read subroutine name from the first index of the delimited string
                    string subroutineName = unparsedSubroutine.Substring(0, delimIndex);
                    // Make sure there is a key mapping for this subroutine
                    if (!instructionsToWingDings.ContainsKey(subroutineName))
                    {
                        continue;
                    }
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
                    Console.WriteLine("\nFailed to parse subroutine: {0}\n\nError: {1}", string.Format(subroutinesFile, dataConfigName), e.ToString());
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
                        subroutineCode += string.Format("|{0}", instructionsToWingDings["cls_tape"]);
                    }

                    userCode = userCode.Replace(subroutineWingDing, subroutineCode);
                }
            }
            return userCode;
        }
        public DataLoader(string _dataConfigName)
        {
            dataConfigName = _dataConfigName;

            try // Try to load data files
            {
                LoadData();
            }
            catch (Exception e)
            {
                Console.WriteLine("!!!!!!!! Error loading subroutines !!!!!!!!\n got: {0}", e.ToString());
                // Wait for user input
                UserOpts.PressAnyKey();
                // Exit the program
                System.Environment.Exit(1);
            }
        }

        public void SaveSubroutine(string userCode, List<string> reservedShortcuts)
        {
            /*
                Save a new subroutine 

                args: 
                    userCode: Code (in wingdings) to save.

                TODO:
                    - Accept wingding symbol and key binding args
                    - Use dynamic menus
            */

            // Display the wingdingfork header
            FileHelper.printWdfHeader();

            string subroutineName;
            string subroutineWingDing;

            // Get the subroutine name from the user
            Console.WriteLine("Saving code: {0}", userCode.Replace(FileHelper.INSTRUCTION_DELIM, ""));
            Console.Write("\nEnter subroutine name: ");
            subroutineName = Console.ReadLine();

            // Get The subroutine wingding from the user
            Console.Write("\nEnter subroutine WingDing: ", subroutineName);
            subroutineWingDing = Console.ReadLine();

            // Make sure there is an existing key mapping
            if (!wingDingsToKeys.ContainsKey(subroutineWingDing))
            {
                // Create a new mapping
                string shortcut = "";
                for (int tries = 3; tries >= 0; tries--)
                {
                    Console.Write("\nEnter shortcut key for {0}: ", subroutineName);

                    // Readline, but only takes the first character. Prevents errors from pasted input when using readkey.
                    shortcut = Console.ReadLine();

                    if (shortcut == "")
                    {
                        continue;
                    }
                    else
                    {
                        shortcut = shortcut.Substring(0, 1);
                    }
                    
                    if (reservedShortcuts.Contains(shortcut) || keysToWingDings.ContainsKey(shortcut))
                    {
                        Console.WriteLine("{0} is already a keymap or menu option.", shortcut);
                    }
                    else
                    {
                        break; // Accepted shortcut
                    }
                }
                if (shortcut != "")
                {
                    using (StreamWriter sw = File.AppendText(keymapFile))
                    {
                        sw.Write("\n" + subroutineWingDing + FileHelper.INSTRUCTION_DELIM + shortcut + FileHelper.INSTRUCTION_DELIM + subroutineName);
                    }
                }
                else
                {
                    UserOpts.PressAnyKey("No valid shortcut was entered. Press any key to continue...");
                    return;
                }
            }
            // Write the subroutine 
            string subroutinePath = string.Format(subroutinesFile, dataConfigName);

            // Allow spaces and special characters
            subroutineName = string.Format(@"{0}", subroutineName);

            if (instructionsToWingDings.ContainsKey(subroutineName))
            {
                UserOpts.PressAnyKey(string.Format("Subroutine {0} already exists in {1}.\nPress any key to continue...", subroutineName, subroutinePath));
                return;
            }

            // File.WriteAllText(subroutinePath, userCode.ToString());
            using (StreamWriter sw = File.AppendText(subroutinePath))
            {
                sw.Write("\n" + subroutineName + FileHelper.INSTRUCTION_DELIM + userCode.ToString());
            }

            // Wait for user input
            UserOpts.PressAnyKey(string.Format("Current code saved to: subroutines:{0}\nPress any key to continue...", subroutineName));

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