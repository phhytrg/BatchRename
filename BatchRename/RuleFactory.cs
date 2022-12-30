using Contract;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace BatchRename
{
    public class RuleFactory
    {
        static Dictionary<string, IRule> _prototypes = new Dictionary<string, IRule>();
        public static bool Register(IRule prototype)
        {
            return _prototypes.TryAdd(prototype.RuleType, prototype);
        }

        private static RuleFactory? _instance = null;
        public static RuleFactory Instance()
        {
            if(_instance == null)
            {
                _instance = new RuleFactory();
            }
            return _instance;
        }

        private RuleFactory() { }

        public IRule? Parse(string line)
        {
            const string Space = " ";

            var tokens = line.Split(
                new string[] { Space }, StringSplitOptions.None
            );

            var keyword = tokens[0];
            IRule? result = null;

            if (_prototypes.ContainsKey(keyword))
            {
                IRule prototype = _prototypes[keyword];
                if (tokens.Length > 1)
                {
                    result = prototype.Parse(tokens[1]);
                }
                else
                {
                    result = prototype.Parse(tokens[0]);
                }
            }

            return result;
        }
        public IRule ParseRuleFromJObj(RuleJObj jRule)
        {
            var ruleType = jRule.RuleType;
            IRule? rule = null;
            if (jRule.HasParameter)
            {
                if (_prototypes.ContainsKey(ruleType))
                {
                    rule = (IRuleWithParameters)_prototypes[ruleType].Clone();
                    var serializedParent = JsonConvert.SerializeObject(jRule);
                    RuleWithParametterJObj ruleWithParameters = JsonConvert.DeserializeObject<RuleWithParametterJObj>(serializedParent)!;
                    ((IRuleWithParameters)rule).Values = ruleWithParameters.Values;
                }
            }
            else
            {
                if (_prototypes.ContainsKey(ruleType))
                {
                    rule = (IRule)_prototypes[ruleType].Clone();
                }
            }
            return rule!;
        }

        public IRule Builder(string keyword)
        {
            IRule? result = null;
            if (_prototypes.ContainsKey(keyword))
            {
                result = (IRule?)_prototypes[keyword].Clone();
            }
            return result!;
        }

        public List<string> GetRulesType()
        {
            return _prototypes.Keys.ToList();
        }

        public void UnRegisterAll()
        {
            _prototypes.Clear();
        }
    }
}
