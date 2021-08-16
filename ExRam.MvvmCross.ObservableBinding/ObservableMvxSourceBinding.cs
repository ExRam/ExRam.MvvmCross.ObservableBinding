// ExRam.MvvmCross.ObservableBinding (c) ExRam GmbH & Co. KG http://www.exram.de
// ExRam.MvvmCross.ObservableBinding is licensed using Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using MvvmCross.Base;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Source;
using MvvmCross.Binding.Parse.PropertyPath.PropertyTokens;

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

        public ObservableMvxSourceBinding(IObservable<T> source, Type sourceType, IMvxMainThreadAsyncDispatcher mainThreadDispatcher, IList<MvxPropertyToken> remainingTokens)
        {
            _sourceType = sourceType;

            if (mainThreadDispatcher != null)
                source = source.ObserveOn(mainThreadDispatcher);

            _sourceSubscription = source
                .Subscribe(value =>
                {
                    _currentValue = value;

                    var subBinding = (remainingTokens != null && remainingTokens.Count > 0)
                        ? MvxSingleton<IMvxBindingSingletonCache>.Instance.SourceBindingFactory.CreateBinding(value, remainingTokens)
                        : null;

                    if (subBinding != null)
                    {
                        _currentSubBinding?.Dispose();
                        _currentSubBindingSubscription?.Dispose();

                        _currentSubBinding = subBinding;
                        _currentSubBindingSubscription = Observable
                            .FromEventPattern(
                                eh => subBinding.Changed += eh,
                                eh => subBinding.Changed -= eh)
                            .StartWith(default(EventPattern<object>))
                            .Select(_ =>
                            {
                                var bindingValue = subBinding.GetValue();
                                return bindingValue as IObservable<object> ?? Observable.Return(bindingValue);
                            })
                            .Switch()
                            .Subscribe(_ =>
                            {
                                Changed?.Invoke(this, EventArgs.Empty);
                            });
                    }
                    else
                        Changed?.Invoke(this, EventArgs.Empty);
                });
        }

        public object GetValue()
        {
            return _currentSubBinding != null
                ? _currentSubBinding.GetValue()
                : _currentValue;
        }

        public void SetValue(object value)
        {
        }

        public void Dispose()
        {
            _currentSubBinding?.Dispose();
            _currentSubBindingSubscription?.Dispose();
            _sourceSubscription.Dispose();
        }

        public Type SourceType
        {
            get
            {
                return _currentSubBinding != null ? _currentSubBinding.SourceType : _sourceType;
            }
        }
    }
}
