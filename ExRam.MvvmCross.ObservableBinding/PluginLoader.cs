using MvvmCross.Base;
using MvvmCross.Binding.Bindings.Source.Construction;
using MvvmCross.IoC;
using MvvmCross.Plugin;

namespace ExRam.MvvmCross.ObservableBinding
{
    [MvxPluginAttribute]
    public sealed class Plugin : IMvxPlugin
    {
        public void Load(IMvxIoCProvider provider)
        {
            var host = provider.Resolve<IMvxSourceBindingFactoryExtensionHost>();
            var dispatcher = provider.Resolve<IMvxMainThreadAsyncDispatcher>();
            host?.Extensions.Insert(0, new ObservableMvxPropertySourceBindingFactoryExtension(dispatcher));
        }
    }
}
