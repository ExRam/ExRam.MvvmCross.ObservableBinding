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
        private bool _skipThisExtension = false;

        public bool TryCreateBinding(object source, MvxPropertyToken propertyToken, List<MvxPropertyToken> remainingTokens, out IMvxSourceBinding result)
        {
            var observable = source as IObservable<object>;

            if ((observable == null) && (this._skipThisExtension))
            {
                //source isn't an IObservable, and we may skip this extension anyway.
                //Reset _skipThisExtension and leave.
                result = null;
                this._skipThisExtension = false;

                return false;
            }
            else
            {
                //source is IObservable -OR- we may not skip this extension.

                //Insert propertyToken back into remainingTokens. This is done because
                //Either
                // - source is an IObservable. Then, propertyToken must not be looked up on source itself
                //   but rather on the elements that come out of the IObservable.
                // - source is not an IObservable. Then IMvxSourceBindingFactory.CreateBinding needs
                //   the list of [propertyToken | remainingTokens].
                if (!(propertyToken is MvxEmptyPropertyToken))
                {
                    remainingTokens = new List<MvxPropertyToken>(remainingTokens);
                    remainingTokens.Insert(0, propertyToken);
                }

                if (observable != null)
                    result = new ObservableMvxSourceBinding(observable, remainingTokens);
                else
                {
                    //Recursively call IMvxSourceBindingFactory.CreateBinding(source, remainingTokens).
                    //This will call into this extension again! To avoid an infinite loop, set a variable
                    //to signal that this extension is to be skipped next time.

                    //It is this kind of workaround that I would like to solve differently.
                    //The problem to solve is: How can an extension use IMvxSourceBindingFactory.CreateBinding
                    //such that the IMvxSourceBindingFactory excludes the extension once?

                    //Suggestion: Change IMvxSourceBindingFactory.CreateBinding to
                    //IMvxSourceBinding CreateBinding(object source, IList<Cirrious.MvvmCross.Binding.Parse.PropertyPath.PropertyTokens.MvxPropertyToken> tokens, params IMvxSourceBindingFactoryExtension[] extensionsToSkip);
                    this._skipThisExtension = true;

                    result = Mvx.Resolve<IMvxSourceBindingFactory>().CreateBinding(source, remainingTokens);

                    if (typeof(IObservable<object>).GetTypeInfo().IsAssignableFrom(result.SourceType.GetTypeInfo()))
                        result = new ObservableMvxSourceBinding(new BindingToObservableWrapper(result), null);
                }

                return true;
            }
        }
    }
}
