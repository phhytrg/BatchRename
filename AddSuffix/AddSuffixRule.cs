using Contract;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace AddSuffix
{
    public class AddSuffix: IRuleWithParameters
    {
        public string _suffix = "";

        public string RuleType => "AddSuffix";

        public bool HasParameter => true;

        public ImmutableList<string> Keys => new List<string> { "Suffix" }.ToImmutableList();
        public AddSuffix() { }
        public AddSuffix(string suffix)
        {
            _suffix = suffix;
        }

        public override string ToString()
        {
            string toString = "";
            for (int i = 0; i < Keys.Count; i++)
            {
                toString += Keys[i];
                toString += "=";
                toString += Values[i];
                if (i >= Keys.Count - 1)
                {
                    break;
                }
                toString += ", ";
            }
            return toString;
        }

        public List<string> Values
        {
            get
            {
                return new List<string> { _suffix };
            }
            set
            {
                _suffix = value[0];
            }
        }

        public string Errors => "";

        public string Rename(string origin)
        {
            var builder = new StringBuilder();
            builder.Append(origin);
            builder.Append(" ");
            builder.Append(_suffix);

            string result = builder.ToString();
            return result;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public IRule? Parse(string data)
        {
            var pairs = data.Split(new string[] { "=" },
                StringSplitOptions.None);

            var suffix = pairs[1];

            var rule = new AddSuffix(suffix);
            return rule;
        }
    }
}
