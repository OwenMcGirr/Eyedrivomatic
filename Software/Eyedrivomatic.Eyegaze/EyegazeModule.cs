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
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Eyedrivomatic.Eyegaze.DwellClick;
using Eyedrivomatic.Infrastructure;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;

namespace Eyedrivomatic.Eyegaze
{
    /// <summary>
    /// The purpose of this module is to register custom controls and their dependencies.
    /// </summary>
    [ModuleExport(typeof(EyegazeModule), 
        InitializationMode = InitializationMode.WhenAvailable,
        DependsOnModuleNames =  new[] { nameof(InfrastructureModule) })]
    public class EyegazeModule : IModule, IDisposable
    {
        [ImportingConstructor]
        public EyegazeModule(ExportFactory<DwellClickBehavior> dwellClickBehaviorFactory)
        {
            Log.Debug(this, $"Creating Module {nameof(EyegazeModule)}.");

            DwellClickBehaviorFactory.Create = () => dwellClickBehaviorFactory.CreateExport().Value;
        }

        [Import]
        public IRegionManager RegionManager { get; set; }

        public void Initialize()
        {
            Log.Debug(this, $"Initializing Module {nameof(EyegazeModule)}.");

            var thisDir = Path.GetDirectoryName(new Uri(GetType().Assembly.CodeBase).AbsolutePath);
            var catalog = new AggregateCatalog(new AssemblyCatalog(Assembly.GetExecutingAssembly()),
                new DirectoryCatalog(thisDir ?? ".", "Eyedrivomatic.Eyegaze.Interfaces.*.dll"));
            var container = new CompositionContainer(catalog);
            EyegazeProviders = container.GetExports<IEyegazeProvider, IEyegazeProviderMetadata>().ToList();
        }

        [Export(typeof(IEnumerable<Lazy<IEyegazeProvider, IEyegazeProviderMetadata>>))]
        public List<Lazy<IEyegazeProvider, IEyegazeProviderMetadata>> EyegazeProviders { get; private set; }


        [Export(typeof(Func<UIElement, DwellClickAdorner>))]
        public DwellClickAdorner CreateDwellClickAdorner(UIElement adornedElement)
        {
            return new DwellClickPieAdorner(adornedElement);
        }

        public void Dispose()
        {
            try
            {
                foreach (var providerExport in EyegazeProviders.Where(e => e.IsValueCreated))
                {
                    providerExport.Value.Dispose();
                }
            }
            catch (Exception e)
            {
                Log.Error(this, $"Error while disposing Eyegaze provider - [{e}]");
            }
        }
    }
}