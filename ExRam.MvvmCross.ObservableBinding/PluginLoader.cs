using MvvmCross.Binding.Bindings.Source.Construction;
using MvvmCross.Platform;
using MvvmCross.Platform.Core;
using MvvmCross.Platform.Plugins;

namespace ExRam.MvvmCross.ObservableBinding
{
    public sealed class PluginLoader
        : IMvxPluginLoader
    {
        public static readonly PluginLoader Instance = new PluginLoader();

        private bool _loaded;

        public void EnsureLoaded()
        {
            if (!this._loaded)
            {
                Mvx.CallbackWhenRegistered<IMvxSourceBindingFactoryExtensionHost>(host => host.Extensions.Insert(0, new ObservableMvxPropertySourceBindingFactoryExtension(Mvx.Resolve<IMvxMainThreadDispatcher>())));
                this._loaded = true;
            }
        }
    }
}