using System.Text;

namespace Translator
{
    static class BFConverter
    {
        public static string ConvertTextToBF(string text)
        {
            // Simplest text to BF converter using no loops
            StringBuilder bf = new StringBuilder();
            int prev = 0;

            foreach (char c in text)
            {
                int diff = c - prev;

                if (diff > 0)
                    bf.Append(new string('+', diff));
                else
                    bf.Append(new string('-', -diff));

                bf.Append('.');
                prev = c;
            }

            return bf.ToString();
        }
    }
}