using Contract;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
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
            var tokens = origin.Split('.');
            //var filename = Path.GetFileName(origin);
            //var extension = Path.GetExtension(origin);
            builder.Append(tokens[0]);
            builder.Append(" ");
            builder.Append(_suffix);
            if (tokens.Length > 1)
            {
                builder.Append('.' + tokens[1]);
            }

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
