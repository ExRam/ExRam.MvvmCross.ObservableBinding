ExRam.MvvmCross.ObservableBinding
=================================

Support MvvmCross-Bindings to IObservable&lt;T>-properties in view models.

By overriding the initialization process in MvvmCross to use <code>ObservableSourceBindingFactory</code>, view models may expose properties of Type <code>IObservable&lt;T></code>. The changes in the Observable will then be observed by the binding and be reflected by the UI.

Note:
- This does now also work for properties of type <code>IObservable&lt;T></code> where <code>T</code> is a value type. However, on AOT platforms, it may not work for arbitrary value types, especially custom structs. For the built-in value types (int, char, etc.) it should work though.
- It is now possible to pass an IMvxMainThreadDispatcher to the extension. Changes on observables (even nested ones) are then observed on that dispatcher.
