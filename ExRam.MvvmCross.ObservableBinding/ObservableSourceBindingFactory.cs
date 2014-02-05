// ExRam.MvvmCross.ObservableBinding (c) ExRam GmbH & Co. KG http://www.exram.de
// ExRam.MvvmCross.ObservableBinding is licensed using Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using Cirrious.MvvmCross.Binding.Bindings.Source;
using Cirrious.MvvmCross.Binding.Bindings.Source.Construction;
using Cirrious.MvvmCross.Binding.Parse.PropertyPath.PropertyTokens;

namespace ExRam.MvvmCross.ObservableBinding
{
    public class ObservableSourceBindingFactory : IMvxSourceBindingFactory, IMvxSourceBindingFactoryExtensionHost
    {
        private readonly IMvxSourceBindingFactory _baseSourceBindingFactory;
        private readonly IList<IMvxSourceBindingFactoryExtension> _extensionHostExtensions;

        public ObservableSourceBindingFactory(IMvxSourceBindingFactory baseFactory)
        {
            Contract.Requires(baseFactory != null);

            this._baseSourceBindingFactory = baseFactory;
            this._extensionHostExtensions = ((baseFactory is IMvxSourceBindingFactoryExtensionHost) ? ((IMvxSourceBindingFactoryExtensionHost)baseFactory).Extensions : (new List<IMvxSourceBindingFactoryExtension>()));

            this._extensionHostExtensions.Add(new ObservableMvxPropertySourceBindingFactoryExtension());
        }

        public IMvxSourceBinding CreateBinding(object source, string combinedPropertyName)
        {
            return this.Check(this._baseSourceBindingFactory.CreateBinding(source, combinedPropertyName));
        }

        public IMvxSourceBinding CreateBinding(object source, IList<MvxPropertyToken> tokens)
        {
            return this.Check(this._baseSourceBindingFactory.CreateBinding(source, tokens));
        }

        private IMvxSourceBinding Check(IMvxSourceBinding binding)
        {
            if (typeof(IObservable<object>).GetTypeInfo().IsAssignableFrom(binding.SourceType.GetTypeInfo()))
                return new ObservableMvxSourceBinding(new BindingToObservableWrapper(binding), null);

            return binding;
        }

        public IList<IMvxSourceBindingFactoryExtension> Extensions
        {
            get
            {
                return this._extensionHostExtensions;
            }
        }
    }
}
