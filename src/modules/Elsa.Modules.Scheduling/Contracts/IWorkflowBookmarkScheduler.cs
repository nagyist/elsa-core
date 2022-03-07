using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Models;
using Elsa.Persistence.Entities;

namespace Elsa.Modules.Scheduling.Contracts;

/// <summary>
/// Schedules jobs for the specified list of workflow bookmarks.
/// </summary>
public interface IWorkflowBookmarkScheduler
{
    Task ScheduleBookmarksAsync(string workflowInstanceId, IEnumerable<WorkflowBookmark> bookmarks, CancellationToken cancellationToken = default);
    Task UnscheduleBookmarksAsync(string workflowInstanceId, IEnumerable<WorkflowBookmark> bookmarks, CancellationToken cancellationToken = default);
}