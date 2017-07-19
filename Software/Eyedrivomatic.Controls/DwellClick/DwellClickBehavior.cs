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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interactivity;
using System.Windows.Media;
using Eyedrivomatic.Infrastructure;
using NullGuard;

namespace Eyedrivomatic.Controls.DwellClick
{
    public static class DwellClickBehaviorFactory
    {
        [Import]
        public static Func<DwellClickBehavior> Create;
    }

    [Export(typeof(DwellClickBehavior)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class DwellClickBehavior : Behavior<UIElement>
    {
        #region DefaultConfiguration
        private static IDwellClickConfigurationService _defaultConfiguration; //off.

        [AllowNull]
        public static IDwellClickConfigurationService DefaultConfiguration
        { 
            get => _defaultConfiguration;
            set
            {
                if (ReferenceEquals(_defaultConfiguration, value)) return;

                _defaultConfiguration = value;
                DefaultConfigurationChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static event EventHandler DefaultConfigurationChanged;

        #endregion DefaultConfiguration

        #region Pause (static)

        private static bool _pause = true;
        /// <summary>
        /// Pause is used to suspend DwellClick on all controls for which IgnorePause is set.
        /// The idea is that IgnorePause would be set on the Pause button, but nowhere else.
        /// </summary>
        public static bool Pause
        {
            get => _pause;
            set
            {
                if (_pause == value) return;

                _pause = value;
                PauseChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static event EventHandler PauseChanged;

        #endregion Pause (static)

        #region Attached Properties
        #region Configuration
        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.RegisterAttached("Configuration", typeof(IDwellClickConfigurationService), typeof(DwellClickBehavior), new PropertyMetadata(OnConfigurationChanged));

        public static IDwellClickConfigurationService GetConfiguration(DependencyObject obj)
        {
            return (IDwellClickConfigurationService)obj.GetValue(ConfigurationProperty);
        }

        public static void SetConfiguration(DependencyObject obj, IDwellClickConfigurationService value)
        {
            obj.SetValue(ConfigurationProperty, value);
        }

        private static void OnConfigurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //verify that the dwell click behavior has been created.
            var behaviors = Interaction.GetBehaviors(d);
            var dwellClickBehavior = behaviors.OfType<DwellClickBehavior>().SingleOrDefault();

            if (dwellClickBehavior == null)
            {
                dwellClickBehavior = DwellClickBehaviorFactory.Create();
                behaviors.Add(dwellClickBehavior);
            }
        }
        #endregion Configuration

        public static readonly DependencyProperty RoleProperty = 
            DependencyProperty.RegisterAttached("Role", typeof(DwellClickActivationRole), typeof(DwellClickBehavior), new PropertyMetadata(DwellClickActivationRole.Standard));

        public static void SetRole(DependencyObject obj, DwellClickActivationRole value)
        {
            obj.SetValue(RoleProperty, value);
        }

        public static DwellClickActivationRole GetRole(DependencyObject obj)
        {
            return (DwellClickActivationRole)obj.GetValue(RoleProperty);
        }


        #region AdornerStyle
        public static readonly DependencyProperty AdornerStyleProperty =
            DependencyProperty.RegisterAttached("AdornerStyle", typeof(Style), typeof(DwellClickBehavior), new PropertyMetadata(null));

        public static Style GetAdornerStyle(DependencyObject obj)
        {
            return (Style)obj.GetValue(AdornerStyleProperty);
        }

        public static void SetAdornerStyle(DependencyObject obj, Style value)
        {
            obj.SetValue(AdornerStyleProperty, value);
        }

        #endregion AdornerStyle

        #region IgnorePause
        public static readonly DependencyProperty IgnorePauseProperty =
            DependencyProperty.RegisterAttached("IgnorePause", typeof(bool), typeof(DwellClickBehavior), new PropertyMetadata(false));

        public static bool GetIgnorePause(DependencyObject obj)
        {
            return (bool)obj.GetValue(IgnorePauseProperty);
        }

        public static void SetIgnorePause(DependencyObject obj, bool value)
        {
            obj.SetValue(IgnorePauseProperty, value);
        }
        #endregion IgnorePause
        #endregion Attached Properties

        //If the mouse leaves the control, the animation is paused and a cancellation timer starts. 
        //If the mouse re-enters the control before the timer triggers, then the animation continues. 
        //This is to prevent a very short "eye-twitch" from stopping the dwell click.
        private IDisposable _dwellCancelRegistration;
        private CancellationTokenSource _repeatCancelSource;

        private int _mouseMoves;
        private const int RequiredMouseMoves = 2; //the number of mouse moves that need to happen before a click can begin.

        private readonly IDwellClickAnimator _animator;
        private DwellClickAdorner _adorner;

        [ImportingConstructor]
        public DwellClickBehavior(IDwellClickAnimator animator)
        {
            _animator = animator;
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;
            AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
        }

        private void AssociatedObject_MouseDown(object sender, RoutedEventArgs e)
        {
            _animator?.StopAnimation();
            RemoveAdorner();
            e.Handled = true;
        }

        private void AssociatedObject_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _dwellCancelRegistration?.Dispose(); //If the cancellation timer is running, stop it.
            _dwellCancelRegistration = null;

            _mouseMoves = 0;
            e.Handled = true;
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var configruation = GetConfiguration(AssociatedObject);
            if (configruation == null || !configruation.EnableDwellClick || Paused) return;

            if (IsOverClickableVisibleChild(e.GetPosition(AssociatedObject)))
            {
                CancelDwellClick();
                return;
            }

            //Only start the animation when the mouse has moved a specified number of times after MouseEnter or the last click.
            //This prevents unintended double-clicks if the gaze tracking is lost.
            if (_mouseMoves < 0 || ++_mouseMoves < RequiredMouseMoves) return;
            _mouseMoves = -1;

            var role = GetRole(AssociatedObject);

            StartDwellClick(GetDwellTime(configruation, role));
            e.Handled = true;
        }

        private static TimeSpan GetDwellTime(IDwellClickConfigurationService configuration, DwellClickActivationRole role)
        {
            var accessors = new Dictionary<DwellClickActivationRole, Func<IDwellClickConfigurationService, int>>
            {
                { DwellClickActivationRole.Standard, config => config.StandardDwellTimeMilliseconds },
                { DwellClickActivationRole.DirectionButtons, config => config.DirectionButtonDwellTimeMilliseconds},
                { DwellClickActivationRole.StartButton, config => config.StartButtonDwellTimeMilliseconds },
                { DwellClickActivationRole.StopButton, config => config.StopButtonDwellTimeMilliseconds}
            };

            var ms = accessors.ContainsKey(role) ? accessors[role](configuration) : configuration.StandardDwellTimeMilliseconds;
            return TimeSpan.FromMilliseconds(ms);
        }

        private bool IsOverClickableVisibleChild(Point point)
        {
            var result = VisualTreeHelper.HitTest(AssociatedObject, point);

            var visual = result?.VisualHit;
            while (!(visual?.Equals(AssociatedObject) ?? true))
            {
                var behaviors = Interaction.GetBehaviors(visual);
                if (behaviors.OfType<DwellClickBehavior>().Any()) return true;
                visual = VisualTreeHelper.GetParent(visual);
            }

            return false;
        }

        private void StartDwellClick(TimeSpan dwellTime)
        {
            Log.Info(this, $"Starting dwell click on [{AssociatedObject}]");

            if (_adorner == null)
            {
                CreateAdorner();
                _animator.StartAnimation(_adorner, dwellTime, DoClick);
            }
            else
            {
                //re-entry before the cancel timer has fired.
                _animator.ResumeAnimation();
            }
        }

        private void CancelDwellClick()
        {
            _animator?.StopAnimation();
            RemoveAdorner();
            Log.Info(this, $"Canceled dwell click on [{AssociatedObject}]");
        }

        private void AssociatedObject_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _mouseMoves = -1;
            _repeatCancelSource?.Cancel();

            Log.Info(this, $"Pausing dwell click on [{AssociatedObject}]");
            _animator.PauseAnimation();
            HideAdorner();
            StartCancelTimer();
            e.Handled = true;
        }

        private void DoClick()
        {
            Log.Info(this, $"Performing dwell click on [{AssociatedObject}]");

            _animator.StopAnimation(); //should already be stopped.

            if (!DwellClicker.Click(AssociatedObject)) Log.Warn(this, $"Failed to perform dwell click on [{AssociatedObject}].");

            var configruation = GetConfiguration(AssociatedObject);
            if (configruation == null || !configruation.EnableDwellClick || Paused) return;
            var repeatDelay = TimeSpan.FromMilliseconds(configruation.RepeatDelayMilliseconds);

            StartRepeatTimer(repeatDelay);
        }

        private async void StartRepeatTimer(TimeSpan repeatDelay)
        {
            _repeatCancelSource?.Cancel();
            _repeatCancelSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(repeatDelay, _repeatCancelSource.Token);

                RemoveAdorner();
                _mouseMoves = 0;

                Log.Info(this, "Repeat-click.");
            }
            catch (OperationCanceledException)
            {
                //click repeat cancelled.
                Log.Debug(this, $"ClickRepeat canceled on [{AssociatedObject}].");
                RemoveAdorner();
            }
        }

        private void CreateAdorner()
        {
            _adorner = DwellClickAdorner.CreateAndAdd(AssociatedObject);
            var style = GetAdornerStyle(AssociatedObject);
            if (style != null) _adorner.Style = style;
            _adorner.Visibility = Visibility.Visible;
        }

        private void HideAdorner()
        {
            if (_adorner != null)
            {
                _adorner.Visibility = Visibility.Hidden;
            }
        }

        private void RemoveAdorner()
        {
            var tmp = _adorner;
            _adorner = null;

            if (tmp != null)
            {
                tmp.Visibility = Visibility.Collapsed;

                var adornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject);
                if (adornerLayer == null) return;
                adornerLayer.Remove(tmp);
            }
        }

        private void StartCancelTimer()
        {
            var configruation = GetConfiguration(AssociatedObject);
            var cancelTimeout = TimeSpan.FromMilliseconds(configruation.DwellTimeoutMilliseconds);

            //Set a timer that resets the dwell to 0.
            var cancelation = new CancellationTokenSource(cancelTimeout);
            _dwellCancelRegistration = cancelation.Token.Register(CancelDwellClick, true);
        }

        private bool Paused => Pause && !GetIgnorePause(AssociatedObject);
    }
}
