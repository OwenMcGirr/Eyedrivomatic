﻿// Copyright (c) 2016 Eyedrivomatic Authors
//
// This file is part of the 'Eyedrivomatic' PC application.
//
//    This program is intended for use as part of the 'Eyedrivomatic System' for 
//    controlling an electric wheelchair using soley the user's eyes. 
//
//    Eyedrivomatic is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Eyedrivomatic is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Eyedrivomatic.  If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Prism.Commands;

using Eyedrivomatic.ButtonDriver.Hardware;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Resources;

namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    [Export]
    public class DeviceConfigViewModel : ButtonDriverViewModelBase, IHeaderInfoProvider<string>
    {
        private readonly IButtonDriverConfigurationService _configurationService;

        [ImportingConstructor]
        public DeviceConfigViewModel(IHardwareService hardwareService, IButtonDriverConfigurationService configurationService)
            : base(hardwareService)
        {
            Contract.Requires<ArgumentNullException>(hardwareService != null, nameof(hardwareService));
            Contract.Requires<ArgumentNullException>(configurationService != null, nameof(configurationService));

            _configurationService = configurationService;
            _configurationService.PropertyChanged += ConfigurationService_PropertyChanged;
            SaveCommand = new DelegateCommand(SaveChanges, CanSaveChanges);
            RefreshAvailableDeviceListCommand = new DelegateCommand(RefreshAvailableDeviceList, CanRefreshAvailableDeviceList);
            AutoDetectDeviceCommand = DelegateCommand.FromAsyncHandler(AutoDetectDeviceAsync, CanAutoDetectDevice);
            ConnectCommand = DelegateCommand.FromAsyncHandler(ConnectAsync, CanConnect);
            DisconnectCommand = new DelegateCommand(Disconnect, CanDisconnect);

            RefreshAvailableDeviceList();
        }

        private void ConfigurationService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(string.Empty);

            ConnectCommand.RaiseCanExecuteChanged();
            DisconnectCommand.RaiseCanExecuteChanged();
            AutoDetectDeviceCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
        }

        public string HeaderInfo => Strings.ViewName_DeviceConfig;

        public ICommand RefreshAvailableDeviceListCommand { get; }

        public DelegateCommand AutoDetectDeviceCommand { get; }
        public DelegateCommand ConnectCommand { get; }
        public DelegateCommand DisconnectCommand { get; }

        public DelegateCommand SaveCommand { get; }

        public bool Connecting => HardwareService.CurrentDriver?.IsConnecting ?? false;
        public bool Connected => HardwareService.CurrentDriver?.IsConnected ?? false;
        public bool Ready => HardwareService.CurrentDriver?.HardwareReady ?? false;

        /// <remarks>
        /// This safety bypass setting is a wierd animal. Currently the firmware does not save the 
        /// setting as part of auto-save. But we want it to act like a device property that is saved
        /// in this manner. Similarly, we don't want to force the user to select save (or even think they need to)
        /// after toggling the safety bypass. So, the idea here is to delegate this setting to the driver if possible
        /// and only to the configuration service if there is no connection.
        /// </remarks>
        public bool SafetyBypass
        {
            get
            {
                return HardwareService.CurrentDriver != null
                    ? HardwareService.CurrentDriver.SafetyBypassStatus == SafetyBypassState.Unsafe
                    : _configurationService.SafetyBypass;
            }
            set
            {
                if (HardwareService.CurrentDriver != null)
                {
                    HardwareService.CurrentDriver.SafetyBypassStatus = value ? SafetyBypassState.Unsafe : SafetyBypassState.Safe;
                    OnPropertyChanged();
                }
                else
                {
                    _configurationService.SafetyBypass = value;
                }
            }
        }

        public bool AutoSaveDeviceSettingsOnExit
        {
            get { return _configurationService.AutoSaveDeviceSettingsOnExit; }
            set { _configurationService.AutoSaveDeviceSettingsOnExit = value; }
        }

        public class Device
        {
            public string Name;
            public string Port;

            public override string ToString()
            {
                return $"{Port} - {Name}";
            }
        };

        private IList<Device> _availableDevices;
        public IList<Device> AvailableDevices
        {
            get { return _availableDevices; }
            set { SetProperty(ref _availableDevices, value); }
        }

        public Device SelectedDevice
        {
            get { return AvailableDevices.FirstOrDefault(device => device.Port == _configurationService.ConnectionString);  }
            set { _configurationService.ConnectionString = (value?.Port ?? string.Empty); }
        }

        public bool AutoConnect
        {
            get { return _configurationService.AutoConnect; }
            set { _configurationService.AutoConnect = value; }
        }

        public void RefreshAvailableDeviceList()
        {

            AvailableDevices = (from dev in HardwareService.CurrentDriver?.GetAvailableDevices() orderby dev.Item2 select new Device { Name = dev.Item1, Port = dev.Item2 }).ToList();

            ConnectCommand.RaiseCanExecuteChanged();
            DisconnectCommand.RaiseCanExecuteChanged();
            AutoDetectDeviceCommand.RaiseCanExecuteChanged();
        }

        public bool CanRefreshAvailableDeviceList()
        {
            return true;
        }

        protected void SaveChanges()
        {
            _configurationService.Save();
            HardwareService.CurrentDriver?.SaveSettings();
            SaveCommand.RaiseCanExecuteChanged();
        }

        protected bool CanSaveChanges()
        {
            return (HardwareService.CurrentDriver?.IsConnected ?? false) || _configurationService.HasChanges;
        }

        protected async Task AutoDetectDeviceAsync()
        {
            RefreshAvailableDeviceList();
            var port = await HardwareService.CurrentDriver?.AutoDetectDeviceAsync();
            SelectedDevice = AvailableDevices.FirstOrDefault(device => device.Port == port);
        }

        protected bool CanAutoDetectDevice() { return !Connected && !Connecting; }

        protected Task ConnectAsync()
        {
            return HardwareService.CurrentDriver?.ConnectAsync(SelectedDevice.Port);
        }

        protected bool CanConnect()
        {
            return SelectedDevice != null && !Connected && !Connecting;
        }

        protected void Disconnect()
        {
            HardwareService.CurrentDriver?.Disconnect();
        }

        protected bool CanDisconnect()
        {
            return Connected;
        }

        protected override void OnDriverStatusChanged(object sender, EventArgs e)
        {
            base.OnDriverStatusChanged(sender, e);

            ConnectCommand.RaiseCanExecuteChanged();
            DisconnectCommand.RaiseCanExecuteChanged();
            AutoDetectDeviceCommand.RaiseCanExecuteChanged();
        }
    }
}
