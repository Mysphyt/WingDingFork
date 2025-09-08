using System.Text;

namespace dingfork
{
    static class BFConverter
    {
        public static string ConvertTextToBF(string text, Dictionary<string, string> baseInstructions)
        {
            // Simplest text to BF converter using no loops
            StringBuilder bf = new();
            int prev = 0;

            foreach (char c in text)
            {
                int diff = c - prev;

                if (diff > 0)
                    bf.Append(new string(baseInstructions["inc_byte"][0], diff));
                else
                    bf.Append(new string(baseInstructions["dec_byte"][0], -diff));

                bf.Append(baseInstructions["out_byte"]);
                prev = c;
            }

            return bf.ToString();
        }
    }
}