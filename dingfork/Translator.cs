using System.Text;

namespace Translator
{
    class BrainfuckConverter
    {
        static string CharToBF(char c)
        {
            StringBuilder buffer = new StringBuilder("[-]>[-]<");
            int ascii = (int)c;

            buffer.Append(new string('+', ascii / 10));
            buffer.Append("[>++++++++++<-]>");
            buffer.Append(new string('+', ascii % 10));
            buffer.Append(".<");

            return buffer.ToString();
        }

        static string DeltaToBF(int delta)
        {
            StringBuilder buffer = new StringBuilder();

            buffer.Append(new string('+', Math.Abs(delta) / 10));

            if (delta > 0)
                buffer.Append("[>++++++++++<-]>");
            else
                buffer.Append("[>----------<-]>");

            buffer.Append(new string(delta > 0 ? '+' : '-', Math.Abs(delta) % 10));
            buffer.Append(".<");

            return buffer.ToString();
        }

        static string StringToBF(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            StringBuilder buffer = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                if (i == 0)
                {
                    buffer.Append(CharToBF(input[i]));
                }
                else
                {
                    int delta = input[i] - input[i - 1];
                    buffer.Append(DeltaToBF(delta));
                }
            }

            return buffer.ToString();
        }

        public static void Translate()
        {
            Console.Write("Enter a string to convert to Brainfuck: ");
            string input = Console.ReadLine();

            string bfCode = StringToBF(input);
            Console.WriteLine("\nGenerated Brainfuck code:\n");
            Console.WriteLine(bfCode);
        }
    }
}