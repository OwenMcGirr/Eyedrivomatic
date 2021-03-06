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


using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Prism.Commands;

namespace Eyedrivomatic.Controls
{
    /// <summary>
    /// Interaction logic for BrowsableList.xaml
    /// </summary>
    public class BrowsableList : ListBox
    {
        public BrowsableList() 
        {
            SetValue(PrevItemCommandProperty, new DelegateCommand(() => SelectedIndex--, () => SelectedIndex > 0));
            SetValue(NextItemCommandProperty, new DelegateCommand(() => SelectedIndex++, () => SelectedIndex < Items.Count - 1));
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            ((DelegateCommand)PrevItemCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)NextItemCommand).RaiseCanExecuteChanged();
        }


        public ICommand PrevItemCommand => (ICommand)GetValue(PrevItemCommandProperty); 
        public static readonly DependencyProperty PrevItemCommandProperty =
            DependencyProperty.Register(nameof(PrevItemCommand), typeof(ICommand), typeof(BrowsableList), new PropertyMetadata());


        public ICommand NextItemCommand => (ICommand)GetValue(NextItemCommandProperty);
        public static readonly DependencyProperty NextItemCommandProperty =
            DependencyProperty.Register(nameof(NextItemCommand), typeof(ICommand), typeof(BrowsableList), new PropertyMetadata());

        public string PrevLabel
        {
            get { return (string)GetValue(PrevLabelProperty); }
            set { SetValue(PrevLabelProperty, value); }
        }
        public static readonly DependencyProperty PrevLabelProperty =
            DependencyProperty.Register("PrevLabel", typeof(string), typeof(BrowsableList), new PropertyMetadata("Prev"));


        public string NextLabel
        {
            get { return (string)GetValue(NextLabelProperty); }
            set { SetValue(NextLabelProperty, value); }
        }
        public static readonly DependencyProperty NextLabelProperty =
            DependencyProperty.Register("NextLabel", typeof(string), typeof(BrowsableList), new PropertyMetadata("Next"));
    }
}

