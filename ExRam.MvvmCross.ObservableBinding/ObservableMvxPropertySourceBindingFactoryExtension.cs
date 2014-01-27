// ExRam.MvvmCross.ObservableBinding (c) ExRam GmbH & Co. KG http://www.exram.de
// ExRam.MvvmCross.ObservableBinding is licensed using Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using Cirrious.MvvmCross.Binding.Bindings.Source;
using Cirrious.MvvmCross.Binding.Bindings.Source.Construction;
using Cirrious.MvvmCross.Binding.Parse.PropertyPath.PropertyTokens;

namespace ExRam.MvvmCross.ObservableBinding
{
    internal sealed class ObservableMvxPropertySourceBindingFactoryExtension : IMvxSourceBindingFactoryExtension
    {
        public bool TryCreateBinding(object source, MvxPropertyToken propertyToken, List<MvxPropertyToken> remainingTokens, out IMvxSourceBinding result)
        {
            var observable = source as IObservable<object>;

            if (observable != null)
            {
                var list = new List<MvxPropertyToken>();

                if (!(propertyToken is MvxEmptyPropertyToken))
                    list.Add(propertyToken);

                list.AddRange(remainingTokens);

                result = new ObservableMvxSourceBinding(observable, list);
                return true;
            }

            result = null;
            return false;
        }
    }
}
