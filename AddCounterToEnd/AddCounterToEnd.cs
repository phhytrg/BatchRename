using Contract;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace AddCounterToEnd
{
    public class AddCounterToEnd : IRuleWithParameters
    {
        private int _start = 0;
        private int _step = 0;
        private int _noDigits = 0;
        private int _current = 0;
        public ImmutableList<string> Keys => new List<string>() {"Start","Step","No. of digits" }.ToImmutableList();

        public List<string> Values
        {
            get => new List<string> { _start.ToString(), _step.ToString(), _noDigits.ToString() }; 
            set
            {
                _start = int.Parse(value[0]);
                _step = int.Parse(value[1]);
                _noDigits = int.Parse(value[2]);
                _current = _start;
            }
        }

        public string RuleType => "AddCounterToEnd";

        public bool HasParameter => true;

        public object Clone()
        {

            return MemberwiseClone();
        }

        public IRule? Parse(string data)
        {
            throw new NotImplementedException();
        }

        public string Rename(string origin)
        {
            var builder = new StringBuilder();
            string[] tokens = origin.Split('.');
            builder.Append(tokens[0]);
            builder.Append(" ");
            builder.Append(_current.ToString("D" + _noDigits));
            if (tokens.Length >= 2)
            {
                builder.Append(".");
                builder.Append(tokens[1]);
            }

            _current += _step;

            string result = builder.ToString();
            return result;
        }
    }
}
