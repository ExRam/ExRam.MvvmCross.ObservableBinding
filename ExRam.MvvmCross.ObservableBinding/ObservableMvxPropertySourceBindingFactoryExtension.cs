using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.Binding.Bindings.Source;
using Cirrious.MvvmCross.Binding.Bindings.Source.Construction;
using Cirrious.MvvmCross.Binding.Parse.PropertyPath.PropertyTokens;

namespace ExRam.MvvmCross.ObservableBinding
{
    public class ObservableMvxPropertySourceBindingFactoryExtension : IMvxSourceBindingFactoryExtension
    {
        // ReSharper disable once NotAccessedField.Local
        private static readonly Type[] BindingTypes;
        private static readonly object[] EmptyObjectArray = new object[0];

        private readonly IMvxMainThreadDispatcher _mainThreadDispatcher;
        
        static ObservableMvxPropertySourceBindingFactoryExtension()
        {
            ObservableMvxPropertySourceBindingFactoryExtension.BindingTypes = new[]
            {
                typeof(ObservableMvxSourceBinding<object>),
                typeof(ObservableMvxSourceBinding<bool>),
                typeof(ObservableMvxSourceBinding<char>),
                typeof(ObservableMvxSourceBinding<byte>),
                typeof(ObservableMvxSourceBinding<sbyte>),
                typeof(ObservableMvxSourceBinding<short>),
                typeof(ObservableMvxSourceBinding<ushort>),
                typeof(ObservableMvxSourceBinding<int>),
                typeof(ObservableMvxSourceBinding<uint>),
                typeof(ObservableMvxSourceBinding<long>),
                typeof(ObservableMvxSourceBinding<ulong>),
                typeof(ObservableMvxSourceBinding<float>),
                typeof(ObservableMvxSourceBinding<double>),
                typeof(ObservableMvxSourceBinding<decimal>)
            };
        }

        public ObservableMvxPropertySourceBindingFactoryExtension()
        {
            
        }

        public ObservableMvxPropertySourceBindingFactoryExtension(IMvxMainThreadDispatcher mainThreadDispatcher)
        {
            Contract.Requires(mainThreadDispatcher != null);

            this._mainThreadDispatcher = mainThreadDispatcher;
        }

        public bool TryCreateBinding(object source, MvxPropertyToken currentToken,
                                     List<MvxPropertyToken> remainingTokens, out IMvxSourceBinding result)
        {
            if (source != null)
            {
                Type bindingSourceType = null;
                Type bindingTypeParameter = null;

                if (currentToken is MvxEmptyPropertyToken)
                {
                    var observable = source as IObservable<object>;
                    if (observable != null)
                    {
                        result = new ObservableMvxSourceBinding<object>(observable, typeof(object), this._mainThreadDispatcher, remainingTokens);
                        return true;
                    }

                    var implementedInterface = source
                        .GetType()
                        .GetTypeInfo()
                        .ImplementedInterfaces
                        .FirstOrDefault(iface => ((iface.IsConstructedGenericType) && (iface.GetGenericTypeDefinition() == typeof(IObservable<>))));
                       
                    if (implementedInterface != null)
                        bindingSourceType = bindingTypeParameter = implementedInterface.GenericTypeArguments[0];
                }
                else
                {
                    var propertyNameToken = currentToken as MvxPropertyNamePropertyToken;
                    if (propertyNameToken != null)
                    {
                        var propertyInfo = this.FindPropertyInfo(source, propertyNameToken.PropertyName);
                        if (propertyInfo != null)
                        {
                            var propertyTypeInfo = propertyInfo.PropertyType.GetTypeInfo();
                            if ((propertyTypeInfo.IsGenericType) && (propertyTypeInfo.GetGenericTypeDefinition() == typeof(IObservable<>)))
                            {
                                source = propertyInfo.GetValue(source, ObservableMvxPropertySourceBindingFactoryExtension.EmptyObjectArray);

                                if (source != null)
                                {
                                    bindingSourceType = propertyTypeInfo.GenericTypeArguments[0];
                                    bindingTypeParameter = (bindingSourceType.GetTypeInfo().IsValueType)
                                        ? bindingSourceType
                                        : typeof(object);
                                }
                            }
                        }
                    }
                }

                if (bindingTypeParameter != null)
                {
                    result = (IMvxSourceBinding)Activator.CreateInstance(
                        typeof(ObservableMvxSourceBinding<>).MakeGenericType(bindingTypeParameter),
                        source,
                        bindingSourceType,
                        this._mainThreadDispatcher,
                        remainingTokens);

                    return true;
                }
            }

            result = null;
            return false;
        }

        protected PropertyInfo FindPropertyInfo(object source, string name)
        {
            Contract.Requires(source != null);

            var propertyInfo = source.GetType()
                .GetRuntimeProperty(name);

            return propertyInfo;
        }
    }
}