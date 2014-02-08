using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Cirrious.MvvmCross.ViewModels;

namespace ExRam.MvvmCross.ObservableBinding.Sample.Core
{
    public class FirstViewModel : MvxViewModel
    {
        public IObservable<string> ElapsedSeconds
        {
            get
            {
                return Observable
                    .Interval(TimeSpan.FromSeconds(1))
                    .Select(x => x.ToString())
                    .ObserveOn(this.Dispatcher);
            }
        }
    }
}
