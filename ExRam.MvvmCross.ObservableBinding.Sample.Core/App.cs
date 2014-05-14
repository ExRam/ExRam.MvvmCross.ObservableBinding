using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
