
namespace Helper
{
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