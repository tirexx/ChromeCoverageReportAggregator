using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Base.Logging;

namespace Base.Tasks
{
    public static class TaskUtils
    {
        public static Task WhenAllTasks(this IEnumerable<Task> tasks, ILogger logger,
            CancellationToken cancellationToken = default)
        {
            var allTasks = Task.WhenAll(tasks);
            try
            {
                allTasks.Wait(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return allTasks;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "WhenAllTasks Exception");
            }

            if (allTasks.Exception != null) throw allTasks.Exception;

            return allTasks;
        }

        public static Task<T[]> WhenAllTasks<T>(this IEnumerable<Task<T>> tasks, ILogger logger,
            CancellationToken cancellationToken = default)
        {
            var allTasks = Task.WhenAll(tasks);
            try
            {
                allTasks.Wait(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return allTasks;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "WhenAllTasks Exception");
            }

            if (allTasks.Exception != null) throw allTasks.Exception;

            return allTasks;
        }
    }
}