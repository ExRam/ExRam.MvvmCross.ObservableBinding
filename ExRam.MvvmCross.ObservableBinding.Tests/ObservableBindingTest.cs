// ExRam.MvvmCross.ObservableBinding (c) ExRam GmbH & Co. KG http://www.exram.de
// ExRam.MvvmCross.ObservableBinding is licensed using Microsoft Public License (Ms-PL)

using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Bindings.Source.Construction;
using Cirrious.MvvmCross.Binding.Parse.PropertyPath;
using Cirrious.MvvmCross.Test.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExRam.MvvmCross.ObservableBinding
{
    [TestClass]
    public class ObservableBindingTest : MvxIoCSupportingTest
    {
        #region Bar
        private sealed class Bar
        {
            public string BarProperty
            {
                get
                {
                    return "Hello";
                }
            }
        }
        #endregion

        #region Foo
        private sealed class Foo
        {
            public IObservable<string> StringObservable
            {
                get
                {
                    return Observable.Return("Hello");
                }
            }

            public IObservable<bool> BoolObservable
            {
                get
                {
                    return Observable.Return(true);
                }
            }

            public IObservable<object> BoxedBoolObservable
            {
                get
                {
                    return Observable.Return(true).Select(x => (object)x);
                }
            }

            public IObservable<object> BoxedDynamicIntObservable
            {
                get
                {
                    return Observable.Interval(TimeSpan.FromSeconds(1)).Select(x => (object)(int)x);
                }
            }

            public IObservable<Bar> NestedBarObservable
            {
                get
                {
                    return Observable.Return(new Bar());
                }
            }
        }
        #endregion

        #region Target
        private sealed class Target
        {
            private readonly Subject<int> _intPropertySetSubject = new Subject<int>();
            private int _currentIntProperty;

            public int IntProperty
            {
                get
                {
                    return this._currentIntProperty;
                }

                set
                {
                    this._currentIntProperty = value;
                    this._intPropertySetSubject.OnNext(value);
                }
            }

            public IObservable<int> BoolPropertySetObservable
            {
                get
                {
                    return this._intPropertySetSubject;
                }
            }
        }
        #endregion

        public ObservableBindingTest()
        {
            base.Setup();

            var factory = new MvxSourceBindingFactory()
            {
                Extensions = 
                {
                    new ObservableMvxPropertySourceBindingFactoryExtension(),
                    new MvxPropertySourceBindingFactoryExtension() 
                }
            };

            base.Ioc.RegisterSingleton<IMvxSourceBindingFactory>(factory);
            base.Ioc.RegisterSingleton<IMvxSourcePropertyPathParser>(new MvxSourcePropertyPathParser());

            var cache = new MvxBindingSingletonCache();
        }

        [TestMethod]
        public void Binding_to_Foo_StringObservable_succeeds()
        {
            var factory = Mvx.Resolve<IMvxSourceBindingFactory>();
            var binding = factory.CreateBinding(new Foo(), "StringObservable");

            Assert.AreEqual(typeof(string), binding.SourceType);
            Assert.AreEqual("Hello", binding.GetValue());
        }

        [TestMethod]
        public void Binding_to_Foo_BoolObservable_does_not_succeed()
        {
            var factory = Mvx.Resolve<IMvxSourceBindingFactory>();
            var binding = factory.CreateBinding(new Foo(), "BoolObservable");

            Assert.AreEqual(typeof(IObservable<bool>), binding.SourceType);
        }

        [TestMethod]
        public void Binding_to_Foo_BoxedBoolObservable_succeeds()
        {
            var factory = Mvx.Resolve<IMvxSourceBindingFactory>();
            var binding = factory.CreateBinding(new Foo(), "BoxedBoolObservable");

            Assert.AreEqual(typeof(object), binding.SourceType);
            Assert.IsTrue(binding.GetValue() is bool);
            Assert.AreEqual(true, binding.GetValue());
        }

        [TestMethod]
        public void Binding_to_Foo_NestedBarObservable_succeeds()
        {
            var factory = Mvx.Resolve<IMvxSourceBindingFactory>();
            var binding = factory.CreateBinding(new Foo(), "NestedBarObservable.BarProperty");

            Assert.AreEqual(typeof(string), binding.SourceType);
            Assert.AreEqual("Hello", binding.GetValue());
        }

        [TestMethod]
        public void Covariance_and_contravariance_test()
        {
            Assert.IsTrue(typeof(IObservable<string>).IsGenericType);

            Assert.IsTrue(typeof(IObservable<string>).GetGenericTypeDefinition() == typeof(IObservable<>));

            Assert.IsTrue(typeof(IObservable<object>).IsAssignableFrom(typeof(IObservable<string>)));
            Assert.IsFalse(typeof(IObservable<object>).IsAssignableFrom(typeof(IObservable<bool>)));

            Assert.IsTrue(typeof(IObserver<string>).IsAssignableFrom(typeof(IObserver<object>)));
            Assert.IsFalse(typeof(IObserver<bool>).IsAssignableFrom(typeof(IObserver<object>)));
        }
    }
}
