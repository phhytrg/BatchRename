using Contract;
using System;
using System.Text;

namespace PascalCase
{
    public class PascalCase : IRule
    {
        public string RuleType => "PascalCase";

        public bool HasParameter => false;

        public object Clone()
        {
            return MemberwiseClone();
        }

        public IRule? Parse(string data)
        {
            return new PascalCase();
        }

        public override string ToString()
        {
            return "";
        }

        public string Rename(string origin)
        {
            var tokens = origin.Split(' ');
            var builder = new StringBuilder();
            builder.Append(tokens[0].Substring(0,1).ToLower());
            builder.Append(tokens[0].Remove(0,1));

            for(int i = 1; i < tokens.Length; i++)
            {
                builder.Append(tokens[i].Substring(0, 1).ToUpper());
                builder.Append(tokens[i].Remove(0, 1));
            }
            return builder.ToString();
        }
    }
}
