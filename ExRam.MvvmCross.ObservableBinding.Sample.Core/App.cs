using Cirrious.MvvmCross.ViewModels;

namespace ExRam.MvvmCross.ObservableBinding.Sample.Core
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            base.Initialize();
            this.RegisterAppStart<FirstViewModel>();
        }
    }
}
