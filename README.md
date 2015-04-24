ExRam.MvvmCross.ObservableBinding
=================================

Support MvvmCross-Bindings to IObservable&lt;T>-properties in view models.

Usage:
At some point in the initialisation process (usually in your override of <code>MvxSetup.LoadPlugins</code>), call <code>ExRam.MvvmCross.ObservableBinding.PluginLoader.Instance.EnsureLoaded();</code> and you're done.

Note:
- This does now also work for properties of type <code>IObservable&lt;T></code> where <code>T</code> is a value type. However, on AOT platforms, it may not work for arbitrary value types, especially custom structs. For the built-in value types (int, char, etc.) it should work though. In any case, you may always expose your Observable as <code>IObservable&lt;object></code> by boxing each element.
- It is now possible to pass an IMvxMainThreadDispatcher to the extension. Changes on observables (even nested ones) are then observed on that dispatcher.
