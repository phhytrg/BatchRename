using Contract;
using System;

namespace ToUpperCase
{
    public class ToUpperCase : IRule
    {
        public string RuleType => "UpperCase";

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
            return new ToUpperCase();
        }

        public string Rename(string origin)
        {
            return origin.ToUpper();
        }
    }
}
