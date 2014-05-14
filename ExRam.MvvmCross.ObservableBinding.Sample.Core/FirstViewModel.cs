using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.ViewModels;

namespace ExRam.MvvmCross.ObservableBinding.Sample.Core
{
    public class FirstViewModel : MvxViewModel
    {
        private readonly CityViewModel[] _cities;

        public FirstViewModel()
        {
            this._cities = new[]
            {
                new CityViewModel("Aachen", TimeSpan.Zero),
                new CityViewModel("London", TimeSpan.FromHours(-1)),
                new CityViewModel("New York", TimeSpan.FromHours(-6)),
                new CityViewModel("Tokio", TimeSpan.FromHours(8)),
            };
        }

        public IObservable<CityViewModel> CurrentCity
        {
            get 
            {
                return Observable
                    .Timer(TimeSpan.Zero, TimeSpan.FromSeconds(5))
                    .Select(x => this._cities[x % this._cities.Length])
                    .ObserveOn(this.Dispatcher);
            }
        }

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
