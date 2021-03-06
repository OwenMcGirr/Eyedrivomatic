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
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Eyedrivomatic.Infrastructure
{
    public class BoolToImageConverter : Freezable, IValueConverter
    {
        public static readonly DependencyProperty ImageIfTrueProperty =
            DependencyProperty.Register(nameof(ImageIfTrue), typeof(ImageSource), typeof(BoolToImageConverter), new UIPropertyMetadata(null));
        public ImageSource ImageIfTrue
        {
            get { return (ImageSource)GetValue(ImageIfTrueProperty); }
            set { SetValue(ImageIfTrueProperty, value); }
        }

        public static readonly DependencyProperty ImageIfFalseProperty =
            DependencyProperty.Register(nameof(ImageIfFalse), typeof(ImageSource), typeof(BoolToImageConverter), new UIPropertyMetadata(null));
        public ImageSource ImageIfFalse
        {
            get { return (ImageSource)GetValue(ImageIfFalseProperty); }
            set { SetValue(ImageIfFalseProperty, value); }
        }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToBoolean(value) ? ImageIfTrue : ImageIfFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BoolToImageConverter();
        }
    }
}
