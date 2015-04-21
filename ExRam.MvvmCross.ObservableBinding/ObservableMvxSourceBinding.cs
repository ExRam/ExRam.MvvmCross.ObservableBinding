// ExRam.MvvmCross.ObservableBinding (c) ExRam GmbH & Co. KG http://www.exram.de
// ExRam.MvvmCross.ObservableBinding is licensed using Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Bindings.Source;
using Cirrious.MvvmCross.Binding.Parse.PropertyPath.PropertyTokens;

namespace ExRam.MvvmCross.ObservableBinding
{
    internal sealed class ObservableMvxSourceBinding<T> : IMvxSourceBinding
    {
        public event EventHandler Changed;

        private readonly IDisposable _sourceSubscription;
        private readonly Type _sourceType;

        private T _currentValue;
        private IMvxSourceBinding _currentSubBinding;
        private IDisposable _currentSubBindingSubscription;

        public ObservableMvxSourceBinding(IObservable<T> source, Type sourceType, List<MvxPropertyToken> remainingTokens)
        {
            this._sourceType = sourceType;

            this._sourceSubscription = source
                .Subscribe(value =>
                {
                    this._currentValue = value;

                    if (this._currentSubBinding != null)
                    {
                        this._currentSubBinding.Dispose();
                        this._currentSubBinding = null;
                    }

                    if (this._currentSubBindingSubscription != null)
                    {
                        this._currentSubBindingSubscription.Dispose();
                        this._currentSubBindingSubscription = null;
                    }

                    IMvxSourceBinding newSubBinding = null;
                    if ((remainingTokens != null) && (remainingTokens.Count > 0))
                        newSubBinding = MvxBindingSingletonCache.Instance.SourceBindingFactory.CreateBinding(value, remainingTokens);

                    var subBindingObservable = (newSubBinding != null)
                        ? Observable
                            .Return<object>(null)
                            .Concat(Observable
                                .FromEventPattern(eh => newSubBinding.Changed += eh, eh => newSubBinding.Changed -= eh))
                            .Select(x => newSubBinding.GetValue())
                            .Select(x => (x as IObservable<object>) ?? Observable.Return(x))
                            .Switch()
                        : Observable.Return<object>(null);

                    this._currentSubBinding = newSubBinding;

                    this._currentSubBindingSubscription = subBindingObservable.Subscribe((value2 => 
                    {
                        var changed2 = this.Changed;
                        if (changed2 != null)
                            changed2(this, EventArgs.Empty);
                    }));
                });
        }

        public object GetValue()
        {
            return this._currentSubBinding != null
                ? this._currentSubBinding.GetValue()
                : this._currentValue;
        }

        public void SetValue(object value)
        {
        }

        public void Dispose()
        {
            if (this._currentSubBinding != null)
                this._currentSubBinding.Dispose();

            if (this._currentSubBindingSubscription != null)
                this._currentSubBindingSubscription.Dispose();

            this._sourceSubscription.Dispose();
        }

        public Type SourceType
        {
            get
            {
                return this._currentSubBinding != null ? this._currentSubBinding.SourceType : this._sourceType;
            }
        }
    }
}
