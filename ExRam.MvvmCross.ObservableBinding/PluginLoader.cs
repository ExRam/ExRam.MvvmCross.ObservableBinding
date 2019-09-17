using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.Base;
using MvvmCross.Binding.Bindings.Source.Construction;
using MvvmCross.Plugin;

namespace ExRam.MvvmCross.ObservableBinding
{
    [MvxPluginAttribute]
    public sealed class Plugin : IMvxPlugin
    {
        public void Load()
        {
            Mvx.IoCProvider.CallbackWhenRegistered<IMvxSourceBindingFactoryExtensionHost>(host => host.Extensions.Insert(0, new ObservableMvxPropertySourceBindingFactoryExtension(Mvx.IoCProvider.Resolve<IMvxMainThreadAsyncDispatcher>())));
        }
    }
}
