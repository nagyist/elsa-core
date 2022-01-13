﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Elsa.Attributes;
using Elsa.Management.Models;
using Elsa.Models;

namespace Elsa.Activities.Http;

public class HttpTrigger : TriggerActivity
{
    [Input] public Input<string> Path { get; set; } = default!;

    [Input(
        Options = new[] { "GET", "POST", "PUT" },
        UIHint = InputUIHints.CheckList
    )]
    public Input<ICollection<string>> SupportedMethods { get; set; } = new(new[] { HttpMethod.Get.Method });

    [Output] public Output<HttpRequestModel>? Result { get; set; }

    protected override IEnumerable<object> GetHashInputs(TriggerIndexingContext context) => GetHashInputs(context.ExpressionExecutionContext);
    protected override void Execute(ActivityExecutionContext context) => context.SetBookmarks(GetHashInputs(context.ExpressionExecutionContext));

    private IEnumerable<object> GetHashInputs(ExpressionExecutionContext context)
    {
        // Generate a bookmark hash for path and selected methods.
        var path = context.Get(Path);
        var methods = context.Get(SupportedMethods);
        return methods!.Select(x => (path!.ToLowerInvariant(), x.ToLowerInvariant())).Cast<object>().ToArray();
    }
}