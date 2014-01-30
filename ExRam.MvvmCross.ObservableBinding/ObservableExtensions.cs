using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.CrossCore.Core;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<T> ObserveOn<T>(this IObservable<T> source, IMvxMainThreadDispatcher dispatcher)
        {
            Contract.Requires(source != null);
            Contract.Requires(dispatcher != null);

            return Observable.Create<T>((observer) => source.Subscribe(
                (value) => dispatcher.RequestMainThreadAction(() => observer.OnNext(value)),
                (ex) => dispatcher.RequestMainThreadAction(() => observer.OnError(ex)),
                () => dispatcher.RequestMainThreadAction(observer.OnCompleted)));
        }
    }
}
