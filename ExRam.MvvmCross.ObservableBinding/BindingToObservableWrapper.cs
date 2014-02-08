// ExRam.MvvmCross.ObservableBinding (c) ExRam GmbH & Co. KG http://www.exram.de
// ExRam.MvvmCross.ObservableBinding is licensed using Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.Reactive;
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
                .Return<object>(null)
                .Concat(Observable
                    .FromEventPattern((eh) => baseBinding.Changed += eh, (eh) => baseBinding.Changed -= eh))
                .Select(x => baseBinding.GetValue())
                .Select(x => (x as IObservable<object>) ?? Observable.Return<object>(x))
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
