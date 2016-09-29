using System;
using System.Reactive.Linq;
using MvvmCross.Platform.Core;

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
                    .Select(x => DateTime.Now.Add(this._offset).ToString("HH:mm:ss"));
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
