using Contract;
using System;
using System.DirectoryServices.ActiveDirectory;
using System.Text;

namespace OnceSpace
{
    public class OneSpace: IRule
    {
        public string RuleType => "OneSpace";

        public bool HasParameter => false;
        public object Clone()
        {
            return MemberwiseClone();
        }

        public IRule? Parse(string data)
        {
            return new OneSpace();
        }

        public override string ToString()
        {
            return "";
        }

        public string Rename(string origin)
        {
            const char Space = ' ';
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < origin.Length; i++)
            {
                char currentChar = origin[i];

                if (i > 0)
                {
                    char previousChar = origin[i - 1];

                    if (currentChar == Space)
                    {
                        if (previousChar != Space)
                        {
                            builder.Append(currentChar);
                        }
                        else { /* Do nothing */}
                    }
                    else
                    {
                        builder.Append(currentChar);
                    }
                }
                else
                {
                    builder.Append(currentChar);
                }
            }

            string result = builder.ToString();
            return result;
        }
    }
}
