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
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Windows.Input;

using Prism.Mvvm;
using Prism.Regions;

using Eyedrivomatic.ButtonDriver.Macros.Models;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Resources;

namespace Eyedrivomatic.ButtonDriver.Macros.ViewModels
{
    [Export(typeof(ExecuteMacrosViewModel))]
    public class ExecuteMacrosViewModel : BindableBase, INavigationAware, IHeaderInfoProvider<string>
    {
        private readonly IMacroSerializationService _macroSerializationService;

        [ImportingConstructor]
        public ExecuteMacrosViewModel([Import("ExecuteMacroCommand")]ICommand executeMacroCommand, IMacroSerializationService macroSerializationService)
        {
            Contract.Requires<ArgumentNullException>(executeMacroCommand != null, nameof(executeMacroCommand));
            Contract.Requires<ArgumentNullException>(macroSerializationService != null, nameof(macroSerializationService));

            ExecuteMacroCommand = executeMacroCommand;
            _macroSerializationService = macroSerializationService;
            Macros = new ObservableCollection<IMacro>(_macroSerializationService.LoadMacros());
        }

        public ObservableCollection<IMacro> Macros { get; }

        public ICommand ExecuteMacroCommand { get; }

        public string HeaderInfo { get; } = Strings.ViewName_Macros;

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Macros.Clear();
            Macros.AddRange(_macroSerializationService.LoadMacros());
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
