using System;
using System.Collections.Generic;
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
                if (currentToken is MvxEmptyPropertyToken)
                {
                    var observable = source as IObservable<object>;
                    if (observable != null)
                    {
                        result = new ObservableMvxSourceBinding<object>(observable, typeof(object), remainingTokens);
                        return true;
                    }
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
                                var rawValue = propertyInfo.GetValue(source, ObservableMvxPropertySourceBindingFactoryExtension.EmptyObjectArray);

                                if (rawValue != null)
                                {
                                    var typeParameter = propertyTypeInfo.GenericTypeArguments[0];
                                    var bindingTypeParameter = (typeParameter.GetTypeInfo().IsValueType)
                                        ? typeParameter
                                        : typeof(object);

                                    result = (IMvxSourceBinding)Activator.CreateInstance(
                                        typeof(ObservableMvxSourceBinding<>).MakeGenericType(bindingTypeParameter), 
                                        rawValue,
                                        typeParameter,
                                        remainingTokens);

                                    return true;
                                }
                            }
                        }
                    }
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