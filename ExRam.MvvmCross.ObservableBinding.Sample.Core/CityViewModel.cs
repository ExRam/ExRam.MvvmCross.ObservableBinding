using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.ViewModels;

namespace ExRam.MvvmCross.ObservableBinding.Sample.Core
{
    public class CityViewModel : MvxMainThreadDispatchingObject
    {
        private readonly string _name;
        private readonly TimeSpan _offset;

        public CityViewModel(string name, TimeSpan offset)
        {
            this._name = name;
            this._offset = offset;
        }

        public IObservable<string> CurrentTime
        {
            get
            {
                return Observable
                    .Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
                    .Select(x => DateTime.Now.Add(this._offset).ToString("HH:mm:ss"))
                    .ObserveOn(this.Dispatcher);
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }
    }
}
