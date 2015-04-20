using System.Diagnostics.Contracts;

namespace System.Reactive.Linq
{
    internal static partial class InternalObservableExtensions
    {
        public static IObservable<object> Box<T>(this IObservable<T> source) where T : struct
        {
            Contract.Requires(source != null);

            return source.Select(x => (object)x);
        }
    }
}
