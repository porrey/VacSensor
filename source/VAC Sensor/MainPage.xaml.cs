// Copyright © 2015 Daniel Porrey
//
// This file is part of VAC Sensor Solution.
// 
// VAC Sensor Solution is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// VAC Sensor Solution is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with VAC Sensor Solution.  If not, see http://www.gnu.org/licenses/.
//
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace VacSensor
{
	public sealed partial class MainPage : Page, INotifyPropertyChanged
	{
		private GpioPin _pin = null;
		private CoreDispatcher _dispatcher = null;

		public MainPage()
		{
			this.InitializeComponent();
		}

		public event PropertyChangedEventHandler PropertyChanged = null;

		private void OnPropertyChanged([CallerMemberName]string propertyName = null)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private string _gpioState = "Unknown";
		public string GpioState
		{
			get
			{
				return _gpioState;
			}

			set
			{
				this._gpioState = value;
				this.OnPropertyChanged();
			}
		}

		private async Task UpdateUI()
		{
			if (_dispatcher != null)
			{
				await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
				{
					GpioPinValue value = _pin.Read();
					this.GpioState = value == GpioPinValue.High ? "High" : "Low";
				});
			}
		}

		protected async override void OnNavigatedTo(NavigationEventArgs e)
		{
			_dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

			GpioController gpio = GpioController.GetDefault();

			if (gpio != null)
			{
				_pin = gpio.OpenPin(5);
				_pin.SetDriveMode(GpioPinDriveMode.Input);
				_pin.ValueChanged += Pin_ValueChanged;
			}

			await UpdateUI();

			base.OnNavigatedTo(e);
		}

		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			if (_pin != null)
			{
				_pin.Dispose();
				_pin = null;
			}

			_dispatcher = null;

			base.OnNavigatingFrom(e);
		}

		private async void Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
		{
			await UpdateUI();
		}		
	}
}
