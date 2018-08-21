using System.Diagnostics.Contracts;
using MvvmCross.Base;

namespace System.Reactive.Linq
{
    public static class ObservableExtensions
    {
        public static IObservable<T> ObserveOn<T>(this IObservable<T> source, IMvxMainThreadDispatcher dispatcher)
        {
            Contract.Requires(source != null);
            Contract.Requires(dispatcher != null);

            return Observable.Create<T>(observer => source.Subscribe(
                value => dispatcher.RequestMainThreadAction(() => observer.OnNext(value)),
                ex => dispatcher.RequestMainThreadAction(() => observer.OnError(ex)),
                () => dispatcher.RequestMainThreadAction(observer.OnCompleted)));
        }
    }
}
