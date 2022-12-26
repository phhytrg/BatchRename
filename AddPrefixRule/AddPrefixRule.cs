using Contract;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace AddPrefixRule
{
    public class AddPrefixRule : IRuleWithParameters
    {
        public string Prefix { get; set; }

        public string RuleType => "AddPrefix";

        public bool HasParameter => true;

        public ImmutableList<string> Keys => new List<string> { "Prefix"}.ToImmutableList();

        public override string ToString()
        {
            string toString = "";
            for(int i = 0; i < Keys.Count; i++)
            {
                toString += Keys[i];
                toString += "=";
                toString += Values[i];
                if(i >= Keys.Count - 1)
                {
                    break;
                }
                toString += ", ";
            }
            return toString;
        }

        public List<string> Values { 
            get 
            { 
                return new List<string> { Prefix }; 
            }
            set 
            {
                Prefix = value[0];
            } 
        }


        public AddPrefixRule()
        {
            Prefix = "";
        }

        public string Rename(string origin)
        {
            var builder = new StringBuilder();
            builder.Append(Prefix);
            builder.Append(" ");
            builder.Append(origin);

            string result = builder.ToString();
            return result;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public IRule Parse(string line)
        {
            var tokens = line.Split(new string[] { " " },
                StringSplitOptions.None);
            var data = tokens[1];

            var pairs = data.Split(new string[] { "=" },
                StringSplitOptions.None);

            var rule = new AddPrefixRule();
            rule.Prefix = pairs[1];
            return rule;
        }

        IRule? IRule.Parse(string data)
        {
            throw new NotImplementedException();
        }
    }
}
