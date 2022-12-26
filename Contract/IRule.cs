using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Contract
{
    public interface IRule: ICloneable
    {
        string Rename(string origin);
        IRule? Parse(string data);
        string RuleType { get; }
        bool HasParameter { get; }
    }

    public interface IRuleWithParameters: IRule
    {
        ImmutableList<string> Keys { get; }
        List<string> Values { get; set; }
    }
}
