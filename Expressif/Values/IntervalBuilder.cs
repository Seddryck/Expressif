﻿using Expressif.Predicates.Text;
using Expressif.Values;
using Expressif.Values.Casters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values;

internal class IntervalBuilder
{
    public IInterval Create(char lowerBoundChar, string lowerBound, string upperBound, char upperBoundChar)
    {
        var lowerBoundType = lowerBoundChar == ']' ? IntervalType.Open : IntervalType.Closed;
        var upperBoundType = upperBoundChar == '[' ? IntervalType.Open : IntervalType.Closed;

        if (new MatchesNumeric().Evaluate(lowerBound) && new MatchesNumeric().Evaluate(upperBound))
        {
            var caster = new NumericCaster();
            return new Interval<decimal>(caster.Cast(lowerBound), caster.Cast(upperBound), lowerBoundType, upperBoundType);
        }
        else if(
            (new MatchesDateTime().Evaluate(lowerBound) && new MatchesDateTime().Evaluate(upperBound))
            || (new MatchesDate().Evaluate(lowerBound) && new MatchesDate().Evaluate(upperBound))
        )
        {
            var caster = new DateTimeCaster();
            return new Interval<DateTime>(caster.Cast(lowerBound), caster.Cast(upperBound), lowerBoundType, upperBoundType);
        }
        throw new InvalidOperationException();
    }
}
