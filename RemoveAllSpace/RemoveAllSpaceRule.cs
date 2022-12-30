using Contract;
using System;
using System.Text;

namespace RemoveAllSpace
{
    public class RemoveAllSpace: IRule
    {
        public string RuleType => "RemoveAllSpace";

        public bool HasParameter => false;
        public object Clone()
        {
            return MemberwiseClone();
        }
        public override string ToString()
        {
            return "";
        }
        public IRule? Parse(string data)
        {
            return new RemoveAllSpace();
        }

        public string Rename(string origin)
        {
            const char space = ' ';
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < origin.Length; i++)
            {
                char currentChar = origin[i];

                if(currentChar != space)
                {
                    builder.Append(currentChar);
                }
            }

            string result = builder.ToString();
            return result;
        }
    }
}
