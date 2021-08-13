using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using MvvmCross.Base;
using MvvmCross.Binding.Bindings.Source;
using MvvmCross.Binding.Bindings.Source.Construction;
using MvvmCross.Binding.Parse.PropertyPath.PropertyTokens;

namespace ExRam.MvvmCross.ObservableBinding
{
    public class ObservableMvxPropertySourceBindingFactoryExtension : IMvxSourceBindingFactoryExtension
    {
        // ReSharper disable once NotAccessedField.Local
        private static readonly Type[] BindingTypes;
        private static readonly object[] EmptyObjectArray = new object[0];

        internal static readonly IObservable<object> NullObservable = Observable.Return<object>(null);

        private static readonly ConcurrentDictionary<Type, Type> ImplementedObservableInterfaces = new ConcurrentDictionary<Type, Type>();
        private static readonly ConcurrentDictionary<Type, Func<object, Type, IMvxMainThreadAsyncDispatcher, List<MvxPropertyToken>, IMvxSourceBinding>> BindingFactories = new ConcurrentDictionary<Type, Func<object, Type, IMvxMainThreadAsyncDispatcher, List<MvxPropertyToken>, IMvxSourceBinding>>();

        private readonly IMvxMainThreadAsyncDispatcher _mainThreadDispatcher;
        
        static ObservableMvxPropertySourceBindingFactoryExtension()
        {
            BindingTypes = new[]
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

        public ObservableMvxPropertySourceBindingFactoryExtension(IMvxMainThreadAsyncDispatcher mainThreadDispatcher)
        {
            _mainThreadDispatcher = mainThreadDispatcher;
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
                    if (source is IObservable<object> observable)
                    {
                        result = new ObservableMvxSourceBinding<object>(observable, typeof(object), _mainThreadDispatcher, remainingTokens);

                        return true;
                    }

                    bindingSourceType = bindingTypeParameter = ImplementedObservableInterfaces
                        .GetOrAdd(
                            source.GetType(),
                            closureType => source
                                .GetType()
                                .GetTypeInfo()
                                .ImplementedInterfaces
                                .FirstOrDefault(iface => iface.IsConstructedGenericType && iface.GetGenericTypeDefinition() == typeof(IObservable<>))
                                ?.GenericTypeArguments[0]);
                }
                else
                {
                    if (currentToken is MvxPropertyNamePropertyToken propertyNameToken)
                    {
                        var propertyInfo = source
                            .GetType()
                            .GetRuntimeProperty(propertyNameToken.PropertyName);

                        if (propertyInfo != null)
                        {
                            var propertyTypeInfo = propertyInfo.PropertyType.GetTypeInfo();
                            if (propertyTypeInfo.IsGenericType && propertyTypeInfo.GetGenericTypeDefinition() == typeof(IObservable<>))
                            {
                                source = propertyInfo.GetValue(source, EmptyObjectArray);

                                if (source != null)
                                {
                                    bindingSourceType = propertyTypeInfo.GenericTypeArguments[0];
                                    bindingTypeParameter = bindingSourceType.GetTypeInfo().IsValueType
                                        ? bindingSourceType
                                        : typeof(object);
                                }
                            }
                        }
                    }
                }

                if (bindingTypeParameter != null)
                {
                    var factory = BindingFactories.GetOrAdd(
                        bindingTypeParameter,
                        GetFactory);

                    result = factory(source, bindingSourceType, _mainThreadDispatcher, remainingTokens);

                    return true;
                }
            }

            result = null;

            return false;
        }

        private static Func<object, Type, IMvxMainThreadAsyncDispatcher, List<MvxPropertyToken>, IMvxSourceBinding> GetFactory(Type type)
        {
            var constructor = typeof(ObservableMvxSourceBinding<>)
                .MakeGenericType(type)
                .GetConstructors()[0];

            var arg1 = Expression.Parameter(typeof(object));
            var arg2 = Expression.Parameter(typeof(Type));
            var arg3 = Expression.Parameter(typeof(IMvxMainThreadAsyncDispatcher));
            var arg4 = Expression.Parameter(typeof(List<MvxPropertyToken>));

            return Expression
                .Lambda<Func<object, Type, IMvxMainThreadAsyncDispatcher, List<MvxPropertyToken>, IMvxSourceBinding>>(
                    Expression.New(
                        constructor,
                        Expression.Convert(arg1, typeof(IObservable<>).MakeGenericType(type)),
                        arg2,
                        arg3,
                        arg4),
                    arg1,
                    arg2,
                    arg3,
                    arg4)
                .Compile();
        }
    }
}
