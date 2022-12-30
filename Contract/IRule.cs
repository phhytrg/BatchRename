using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Contract
{
    public interface IRule: ICloneable
    {
        string Rename(string origin);
        string RuleType { get; }
        IRule? Parse(string data);
        bool HasParameter { get; }
    }

    public interface IRuleWithParameters: IRule
    {
        ImmutableList<string> Keys { get; }
        List<string> Values { get; set; }
        string Errors { get; }
    }
}
