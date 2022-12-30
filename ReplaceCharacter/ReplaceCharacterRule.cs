using Contract;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace ReplaceCharacter
{
    public class ReplaceCharacter : IRuleWithParameters
    {
        private char _oldChar;
        private char _newChar;
        private string _errors = "";
        public ReplaceCharacter() { }
        public ReplaceCharacter(char oldChar, char newChar)
        {
            _oldChar = oldChar;
            _newChar = newChar;
        }

        public ImmutableList<string> Keys => new List<string>{ "OldChar","NewChar" }.ToImmutableList();

        public List<string> Values {
            get
            {
                return new List<string> { _oldChar.ToString(), _newChar.ToString() };
            } 
            set 
            {
                for (int i = 0; i < value.Count; i++)
                {
                    if (value[i].Length > 1)
                    {
                        _errors += Keys[i] + " must be a character\n";
                    }
                }
                if (_errors != "")
                {
                    return;
                }
                _oldChar = value[0].ToCharArray()[0];
                _newChar = value[1].ToCharArray()[0];
            } 
        }

        public string RuleType => "ReplaceCharacter";

        public bool HasParameter => true;

        public string Errors => _errors;

        public object Clone()
        {
            return MemberwiseClone();
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
                toString += ",";
            }
            return toString;
        }

        public IRule? Parse(string data)
        {
            var tokens = data.Split(',');
            char oldChar = '\0';
            char newChar = '\0';
            foreach(var token in tokens)
            {
                var pairs = token.Split('=');
                if (pairs[0] == Keys[0])
                {
                    oldChar = pairs[1][0];
                }
                else if (pairs[0] == Keys[1])
                {
                    newChar = pairs[1][0];
                }
            }
            if(oldChar == '\0' || newChar == '\0')
            {
                return null;
            }
            return new ReplaceCharacter(oldChar, newChar);
        }

        public string Rename(string origin)
        {
            return origin.Replace(_oldChar, _newChar);
        }
    }
}
