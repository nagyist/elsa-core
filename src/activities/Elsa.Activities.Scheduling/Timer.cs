﻿using System;
using System.Collections.Generic;
using Elsa.Attributes;
using Elsa.Contracts;
using Elsa.Models;

namespace Elsa.Activities.Scheduling;

public class Timer : Trigger
{
    [Input] public Input<TimeSpan> Interval { get; set; } = default!;

    protected override IEnumerable<object> GetHashInputs(TriggerIndexingContext context)
    {
        var interval = context.ExpressionExecutionContext.Get(Interval);
        var clock = context.ExpressionExecutionContext.GetRequiredService<ISystemClock>();
        var executeAt = clock.UtcNow.Add(interval);
        return new object[] { executeAt, interval };
    }
}