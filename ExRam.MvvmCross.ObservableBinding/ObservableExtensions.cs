using System.Diagnostics.Contracts;
using MvvmCross.Base;

namespace System.Reactive.Linq
{
    public static class ObservableExtensions
    {
        public static IObservable<T> ObserveOn<T>(this IObservable<T> source, IMvxMainThreadAsyncDispatcher dispatcher)
        {
            Contract.Requires(source != null);
            Contract.Requires(dispatcher != null);

            return Observable.Create<T>(observer => source.Subscribe(
                value => dispatcher.ExecuteOnMainThreadAsync(() => observer.OnNext(value)),
                ex => dispatcher.ExecuteOnMainThreadAsync(() => observer.OnError(ex)),
                () => dispatcher.ExecuteOnMainThreadAsync(observer.OnCompleted)));
        }
    }
}
