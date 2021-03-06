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
using System.Diagnostics.Contracts;

using Prism.Mvvm;

using Eyedrivomatic.ButtonDriver.Hardware;


namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    public abstract class ButtonDriverViewModelBase : BindableBase
    {
        protected IHardwareService HardwareService { get; }

        public ButtonDriverViewModelBase(IHardwareService hardwareService)
        {
            Contract.Requires<ArgumentNullException>(hardwareService != null, nameof(hardwareService));

            HardwareService = hardwareService;

            hardwareService.CurrentDriverChanged += Hardware_CurrentDriverChanged;
            Hardware_CurrentDriverChanged(this, EventArgs.Empty); //force an update.
        }

        protected virtual void Hardware_CurrentDriverChanged(object sender, EventArgs e)
        {
            if (HardwareService.CurrentDriver != null)
                HardwareService.CurrentDriver.StatusChanged += OnDriverStatusChanged;
            OnPropertyChanged(string.Empty);
        }

        protected virtual void OnDriverStatusChanged(object sender, EventArgs e)
        {
            if (sender is IHardwareService && sender != HardwareService.CurrentDriver)
            {
                //a status change from a driver that is not active. Unsubscribe from events.
                ((IHardwareService)sender).CurrentDriverChanged -= OnDriverStatusChanged;
                return;
            }

            OnPropertyChanged(string.Empty);
        }
    }
}
