using Contract;
using System;

namespace ToLowerCase
{
    public class ToLowerCase : IRule
    {
        public string RuleType => "LowerCase";

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
            return new ToLowerCase();
        }

        public string Rename(string origin)
        {
            return origin.ToLower();
        }
    }
}
