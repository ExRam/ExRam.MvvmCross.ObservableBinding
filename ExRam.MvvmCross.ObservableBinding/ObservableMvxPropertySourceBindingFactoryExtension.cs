// ExRam.MvvmCross.ObservableBinding (c) ExRam GmbH & Co. KG http://www.exram.de
// ExRam.MvvmCross.ObservableBinding is licensed using Microsoft Public License (Ms-PL)

using System;
using System.Reflection;
using System.Collections.Generic;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.Bindings.Source;
using Cirrious.MvvmCross.Binding.Bindings.Source.Construction;
using Cirrious.MvvmCross.Binding.Parse.PropertyPath.PropertyTokens;

namespace ExRam.MvvmCross.ObservableBinding
{
    public sealed class ObservableMvxPropertySourceBindingFactoryExtension : IMvxSourceBindingFactoryExtension
    {
        private bool inFlight = false;

        public bool TryCreateBinding(object source, MvxPropertyToken propertyToken, List<MvxPropertyToken> remainingTokens, out IMvxSourceBinding result)
        {
            var observable = source as IObservable<object>;

            if ((observable == null) && (this.inFlight))
            {
                result = null;
                return false;
            }
            else
            {
                if (!(propertyToken is MvxEmptyPropertyToken))
                {
                    remainingTokens = new List<MvxPropertyToken>(remainingTokens);
                    remainingTokens.Insert(0, propertyToken);
                }

                if (observable != null)
                {
                    result = new ObservableMvxSourceBinding(observable, remainingTokens);
                    return true;
                }
                else
                {
                    this.inFlight = true;

                    try
                    {
                        result = Mvx.Resolve<IMvxSourceBindingFactory>().CreateBinding(source, remainingTokens);

                        if (typeof(IObservable<object>).GetTypeInfo().IsAssignableFrom(result.SourceType.GetTypeInfo()))
                            result = new ObservableMvxSourceBinding(new BindingToObservableWrapper(result), null);

                        return true;
                    }
                    finally
                    {
                        this.inFlight = false;
                    }
                }
            }
        }
    }
}
