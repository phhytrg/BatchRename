using Contract;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;

namespace ChangeExtension
{
    public class ChangeExtension : IRuleWithParameters
    {
        private string _extension = "";

        public ChangeExtension() 
        {

        }
        public ChangeExtension(string extension)
        {
            this._extension = extension;
        }

        public string RuleType => "ChangeExtension";

        public bool HasParameter => true;

        public ImmutableList<string> Keys => new List<string> { "Extension" }.ToImmutableList();

        public List<string> Values { 
            get
            {
                return new List<string> { _extension};
            }
            set
            {
                _extension = value[0];
            }
        }
        public string Errors => "";

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

        public object Clone()
        {
            return MemberwiseClone();
        }

        public IRule? Parse(string data)
        {
            var pairs = data.Split(new string[] { "=" },
                StringSplitOptions.None);
            var extension = pairs[1];
            var rule = new ChangeExtension(extension);
            return rule;
        }

        public string Rename(string origin)
        {
            var builder = new StringBuilder();
            var tokens = origin.Split('.');
            builder.Append(tokens[0]);
            if(tokens.Length > 1)
            {
                builder.Append(".");
                builder.Append(_extension);
            }
            return builder.ToString();
        }
    }
}
