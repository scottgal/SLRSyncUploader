using System;
using System.Threading;
using System.Threading.Tasks;

namespace HashEngine
{

    public sealed class ProgressReporter
    {

        private readonly TaskScheduler _scheduler;


        public ProgressReporter()
        {
        
            _scheduler = TaskScheduler.Current;
        }

        public TaskScheduler Scheduler
        {
            get { return _scheduler; }
        }

        public Task ReportProgressAsync(Action action)
        {
            return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, _scheduler);
        }

        public void ReportProgress(Action action)
        {
            ReportProgressAsync(action).Wait();
        }

        public Task RegisterContinuation(Task task, Action action)
        {
            return task.ContinueWith(_ => action(), CancellationToken.None, TaskContinuationOptions.None, _scheduler);
        }

        public Task RegisterContinuation<TResult>(Task<TResult> task, Action action)
        {
            return task.ContinueWith(_ => action(), CancellationToken.None, TaskContinuationOptions.None, _scheduler);
        }

        public Task RegisterSucceededHandler(Task task, Action action)
        {
            return task.ContinueWith(_ => action(), CancellationToken.None,
                                     TaskContinuationOptions.OnlyOnRanToCompletion, _scheduler);
        }


        public Task RegisterSucceededHandler<TResult>(Task<TResult> task, Action<TResult> action)
        {
            return task.ContinueWith(t => action(t.Result), CancellationToken.None,
                                     TaskContinuationOptions.OnlyOnRanToCompletion, Scheduler);
        }


        public Task RegisterFaultedHandler(Task task, Action<Exception> action)
        {
            return task.ContinueWith(t => action(t.Exception), CancellationToken.None,
                                     TaskContinuationOptions.OnlyOnFaulted, Scheduler);
        }


        public Task RegisterFaultedHandler<TResult>(Task<TResult> task, Action<Exception> action)
        {
            return task.ContinueWith(t => action(t.Exception), CancellationToken.None,
                                     TaskContinuationOptions.OnlyOnFaulted, Scheduler);
        }

        public Task RegisterCancelledHandler(Task task, Action action)
        {
            return task.ContinueWith(_ => action(), CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled,
                                     Scheduler);
        }


        public Task RegisterCancelledHandler<TResult>(Task<TResult> task, Action action)
        {
            return task.ContinueWith(_ => action(), CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled,
                                     Scheduler);
        }
    }
}