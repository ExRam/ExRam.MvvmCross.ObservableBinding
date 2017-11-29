// ExRam.MvvmCross.ObservableBinding (c) ExRam GmbH & Co. KG http://www.exram.de
// ExRam.MvvmCross.ObservableBinding is licensed using Microsoft Public License (Ms-PL)

using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Moq;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Source.Construction;
using MvvmCross.Binding.Parse.PropertyPath;
using MvvmCross.Platform;
using MvvmCross.Test.Core;
using Xunit;

namespace ExRam.MvvmCross.ObservableBinding
{
    public class ObservableBindingTest : MvxIoCSupportingTest
    {
        #region Bar
        private sealed class Bar
        {
            private readonly int _value;

            public Bar(int value)
            {
                this._value = value;
            }

            // ReSharper disable once UnusedMember.Local
            public string BarProperty
            {
                get
                {
                    return "Hello";
                }
            }

            // ReSharper disable once UnusedMember.Local
            public int Value
            {
                get
                {
                    return this._value;
                }
            }

            // ReSharper disable once UnusedMember.Local
            public IObservable<string> StringObservable
            {
                get
                {
                    return Observable
                        .Interval(TimeSpan.FromMilliseconds(100))
                        .Select(x => x.ToString());
                }
            }
        }
        #endregion

        #region Foo
        private sealed class Foo
        {
            // ReSharper disable once UnusedMember.Local
            public IObservable<string> StringObservable
            {
                get
                {
                    return Observable.Return("Hello");
                }
            }

            // ReSharper disable once UnusedMember.Local
            public IObservable<bool> BoolObservable
            {
                get
                {
                    return Observable.Return(true);
                }
            }

            // ReSharper disable once UnusedMember.Local
            public IObservable<object> BoxedBoolObservable
            {
                get
                {
                    return Observable.Return(true).Select(x => (object)x);
                }
            }

            // ReSharper disable once UnusedMember.Local
            public IObservable<object> BoxedDynamicIntObservable
            {
                get
                {
                    return Observable.Interval(TimeSpan.FromSeconds(1)).Select(x => (object)(int)x);
                }
            }

            // ReSharper disable once UnusedMember.Local
            public IObservable<Bar> NestedBarObservable
            {
                get
                {
                    return Observable.Return(new Bar(0));
                }
            }

            // ReSharper disable once UnusedMember.Local
            public IObservable<Bar> DynamicNestedBarObservable
            {
                get
                {
                    return Observable.Interval(TimeSpan.FromMilliseconds(100)).Select(x => new Bar((int)x));
                }
            }
        }
        #endregion

        #region Target
        private sealed class Target
        {
            private readonly Subject<int> _intPropertySetSubject = new Subject<int>();
            private int _currentIntProperty;

            // ReSharper disable once UnusedMember.Local
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

            // ReSharper disable once UnusedMember.Local
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

            var factory = new MvxSourceBindingFactory
            {
                Extensions = 
                {
                    new ObservableMvxPropertySourceBindingFactoryExtension(),
                    new MvxPropertySourceBindingFactoryExtension() 
                }
            };

            base.Ioc.RegisterSingleton<IMvxSourceBindingFactory>(factory);
            base.Ioc.RegisterSingleton<IMvxSourcePropertyPathParser>(new MvxSourcePropertyPathParser());

            // ReSharper disable once UnusedVariable
            var cache = new MvxBindingSingletonCache();
        }

        [Fact]
        public void Binding_to_Foo_StringObservable_succeeds()
        {
            var factory = Mvx.Resolve<IMvxSourceBindingFactory>();
            var binding = factory.CreateBinding(new Foo(), "StringObservable");

            Assert.Equal(typeof(string), binding.SourceType);
            Assert.Equal("Hello", binding.GetValue());
        }

        [Fact]
        public void Binding_directly_to_observable_succeeds()
        {
            var factory = Mvx.Resolve<IMvxSourceBindingFactory>();
            var binding = factory.CreateBinding(Observable.Return("Hello"), "");

            Assert.Equal(typeof(object), binding.SourceType);
            Assert.Equal("Hello", binding.GetValue());
        }

        [Fact]
        public void Binding_directly_to_valuetype_observable_succeeds()
        {
            var factory = Mvx.Resolve<IMvxSourceBindingFactory>();
            var binding = factory.CreateBinding(Observable.Return(true), "");

            Assert.Equal(typeof(bool), binding.SourceType);
            Assert.True((bool)binding.GetValue());
        }

        [Fact]
        public void Disposing_the_binding_unsubscribes_from_source()
        {
            Mock<IDisposable> disposableMock = null;

            var observable = Observable
                .Create<string>(obs =>
                {
                    var subscription = Observable
                        .Return("Hello").Concat(Observable.Never<string>())
                        .Subscribe(obs);

                    disposableMock = new Mock<IDisposable>();
                    disposableMock.Setup(x => x.Dispose()).Callback(subscription.Dispose);

                    return disposableMock.Object;
                });

            var factory = Mvx.Resolve<IMvxSourceBindingFactory>();
            using (var binding = factory.CreateBinding(observable, ""))
            {
                Assert.NotNull(disposableMock);

                Assert.Equal(typeof(object), binding.SourceType);
                Assert.Equal("Hello", binding.GetValue());

                disposableMock.Verify(x => x.Dispose(), Times.Never());
            }

            disposableMock.Verify(x => x.Dispose(), Times.Once());
        }

        [Fact]
        public void Binding_to_Foo_BoolObservable_succeeds()
        {
            var factory = Mvx.Resolve<IMvxSourceBindingFactory>();
            var binding = factory.CreateBinding(new Foo(), "BoolObservable");

            Assert.Equal(typeof(bool), binding.SourceType);
            Assert.True(binding.GetValue() is bool);
            Assert.True((bool)binding.GetValue());
        }

        [Fact]
        public void Binding_to_Foo_BoxedBoolObservable_succeeds()
        {
            var factory = Mvx.Resolve<IMvxSourceBindingFactory>();
            var binding = factory.CreateBinding(new Foo(), "BoxedBoolObservable");

            Assert.Equal(typeof(object), binding.SourceType);
            Assert.True(binding.GetValue() is bool);
            Assert.True((bool)binding.GetValue());
        }

        [Fact]
        public void Binding_to_Foo_NestedBarObservable_succeeds()
        {
            var factory = Mvx.Resolve<IMvxSourceBindingFactory>();
            using (var binding = factory.CreateBinding(new Foo(), "NestedBarObservable.BarProperty"))
            {
                Assert.Equal(typeof(string), binding.SourceType);
                Assert.Equal("Hello", binding.GetValue());
            }
        }

        [Fact]
        public async Task Binding_to_Foo_NestedBarObservable_produces_correct_values()
        {
            var factory = Mvx.Resolve<IMvxSourceBindingFactory>();
            var binding = factory.CreateBinding(new Foo(), "NestedBarObservable.StringObservable");

            Assert.Equal(typeof(string), binding.SourceType);

            var array = await Observable.FromEventPattern<EventHandler, EventArgs>(eh => binding.Changed += eh, eh => binding.Changed -= eh)
                .Select(x => binding.GetValue())
                .Take(10)
                .ToArray()
                .ToTask();

            for (var i = 0; i < 10; i++)
            {
                Assert.Equal(i.ToString(), array[i]);
            }
        }

        [Fact]
        public async Task Binding_to_Foo_DynamicNestedBarObservable_Value_produces_correct_values()
        {
            var factory = Mvx.Resolve<IMvxSourceBindingFactory>();
            var binding = factory.CreateBinding(new Foo(), "DynamicNestedBarObservable.Value");

            var array = await Observable.FromEventPattern<EventHandler, EventArgs>(eh => binding.Changed += eh, eh => binding.Changed -= eh)
                .Select(x => binding.GetValue())
                .Take(10)
                .ToArray()
                .ToTask();

            for (var i = 0; i < 10; i++)
            {
                Assert.Equal(i, array[i]);
            }
        }

        [Fact]
        public void SetValue_does_nothing()
        {
            var factory = Mvx.Resolve<IMvxSourceBindingFactory>();
            var binding = factory.CreateBinding(new Foo(), "DynamicNestedBarObservable.Value");

            binding.SetValue("Some value");
            binding.SetValue(36);
            binding.SetValue(new object());
        }

        [Fact]
        public void Covariance_and_contravariance_test()
        {
            Assert.True(typeof(IObservable<string>).IsGenericType);

            Assert.True(typeof(IObservable<string>).GetGenericTypeDefinition() == typeof(IObservable<>));

            Assert.True(typeof(IObservable<object>).IsAssignableFrom(typeof(IObservable<string>)));
            Assert.False(typeof(IObservable<object>).IsAssignableFrom(typeof(IObservable<bool>)));

            Assert.True(typeof(IObserver<string>).IsAssignableFrom(typeof(IObserver<object>)));
            Assert.False(typeof(IObserver<bool>).IsAssignableFrom(typeof(IObserver<object>)));
        }
    }
}
