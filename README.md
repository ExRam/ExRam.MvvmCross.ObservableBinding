ExRam.MvvmCross.ObservableBinding
=================================

Support MvvmCross-Bindings to IObservable&lt;T>-properties in view models.

By overriding the initialization process in MvvmCross to use <code>ObservableSourceBindingFactory</code>, view models may expose properties of Type <code>IObservable&lt;T></code>. The changes in the Observable will then be observed by the binding and be reflected by the UI.

Note:
- This does only work for properties of type <code>IObservable&lt;T></code> where <code>T</code> is a reference type. If you need this for value types, project your observable to <code>IObservable&lt;object></code> by boxing the values.
- The view model must ensure that the changes on the observable are observed on the UI-Thread. You may use the <code>ObserveOn(IMvxMainThreadDispatcher)</code> extension method for this.

