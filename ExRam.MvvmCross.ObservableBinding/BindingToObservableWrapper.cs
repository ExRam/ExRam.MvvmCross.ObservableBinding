// ExRam.MvvmCross.ObservableBinding (c) ExRam GmbH & Co. KG http://www.exram.de
// ExRam.MvvmCross.ObservableBinding is licensed using Microsoft Public License (Ms-PL)

using System;
using System.Reactive.Linq;
using Cirrious.MvvmCross.Binding.Bindings.Source;

namespace ExRam.MvvmCross.ObservableBinding
{
    internal sealed class BindingToObservableWrapper : IObservable<object>
    {
        private readonly IMvxSourceBinding _baseBinding;
        private readonly IObservable<object> _innerObservable;

        public BindingToObservableWrapper(IMvxSourceBinding baseBinding)
        {
            this._baseBinding = baseBinding;

            this._innerObservable = Observable
                .Defer(() => Observable.Return(baseBinding.GetValue()))
                .Concat(Observable
                    .FromEventPattern((eh) => this._baseBinding.Changed += eh, (eh) => this._baseBinding.Changed -= eh))
                .Select(x => (baseBinding.GetValue() as IObservable<object>) ?? Observable.Return<IObservable<object>>(null))
                .Switch();
        }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            return this._innerObservable.Subscribe(observer);
        }

        public Type SourceType
        {
            get
            {
                return this._baseBinding.SourceType;
            }
        }
    }
}
