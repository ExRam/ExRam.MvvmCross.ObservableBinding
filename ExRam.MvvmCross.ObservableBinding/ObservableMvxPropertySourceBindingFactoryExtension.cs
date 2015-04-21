using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using Cirrious.MvvmCross.Binding.Bindings.Source;
using Cirrious.MvvmCross.Binding.Bindings.Source.Construction;
using Cirrious.MvvmCross.Binding.Parse.PropertyPath.PropertyTokens;

namespace ExRam.MvvmCross.ObservableBinding
{
    public class ObservableMvxPropertySourceBindingFactoryExtension : IMvxSourceBindingFactoryExtension
    {
        private static readonly object[] EmptyObjectArray = new object[0];

        public bool TryCreateBinding(object source, MvxPropertyToken currentToken,
                                     List<MvxPropertyToken> remainingTokens, out IMvxSourceBinding result)
        {
            if (source != null)
            {
                Type bindingSourceType = null;
                Type observableTypeParameter = null;

                if (currentToken is MvxEmptyPropertyToken)
                {
                    var observable = source as IObservable<object>;
                    if (observable != null)
                    {
                        result = new ObservableMvxSourceBinding<object>(observable, typeof(object), remainingTokens);
                        return true;
                    }

                    var implementedInterface = source
                        .GetType()
                        .GetTypeInfo()
                        .ImplementedInterfaces
                        .FirstOrDefault(iface => ((iface.IsConstructedGenericType) && (iface.GetGenericTypeDefinition() == typeof(IObservable<>))));
                       
                    if (implementedInterface != null)
                        bindingSourceType = observableTypeParameter = implementedInterface.GenericTypeArguments[0];
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
                            if ((propertyTypeInfo.IsGenericType) && (propertyTypeInfo.GetGenericTypeDefinition() == typeof (IObservable<>)))
                            {
                                source = propertyInfo.GetValue(source, ObservableMvxPropertySourceBindingFactoryExtension.EmptyObjectArray);

                                if (source != null)
                                {
                                    bindingSourceType = propertyTypeInfo.GenericTypeArguments[0];
                                    observableTypeParameter = (bindingSourceType.GetTypeInfo().IsValueType)
                                        ? bindingSourceType
                                        : typeof(object);
                                }
                            }
                        }
                    }
                }

                if (observableTypeParameter != null)
                {
                    result = (IMvxSourceBinding)Activator.CreateInstance(
                                typeof(ObservableMvxSourceBinding<>).MakeGenericType(observableTypeParameter),
                                source,
                                bindingSourceType,
                                remainingTokens);

                    return true;
                }
            }

            result = null;
            return false;
        }

        protected PropertyInfo FindPropertyInfo(object source, string name)
        {
            var propertyInfo = source.GetType()
                .GetRuntimeProperty(name);

            return propertyInfo;
        }
    }
}