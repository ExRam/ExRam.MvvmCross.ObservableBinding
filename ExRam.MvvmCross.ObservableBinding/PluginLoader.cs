using Cirrious.CrossCore;
using Cirrious.CrossCore.Core;
using Cirrious.CrossCore.Plugins;
using Cirrious.MvvmCross.Binding.Bindings.Source.Construction;

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