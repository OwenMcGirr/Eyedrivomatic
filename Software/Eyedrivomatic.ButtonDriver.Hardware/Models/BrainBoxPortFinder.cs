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


using Microsoft.Practices.ServiceLocation;
using Prism.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace Eyedrivomatic.ButtonDriver.Hardware
{
    public class BrainBoxPortFinder
    {
        private static readonly string[] DeviceUsbVids = new[] { "2341", "2A03" }; //The Arduino (2341) and Genuino (2A03) Vid's
        private static readonly string[] DeviceUsbPids = new[] { "0001", "0043", "0243" }; //The known Arduino Uno Pid's

        private static string StartupMessage = "0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0";

        private static ILoggerFacade Logger => ServiceLocator.Current.GetInstance<ILoggerFacade>();

        public static async Task<SerialPort> DetectDeviceAsync()
        {
            var devices = UsbSerialDeviceEnumerator.EnumerateDevices();

            if (!devices.Any()) return null;

            var tasks = (from device in devices where DeviceUsbVids.Contains(device.Vid) && DeviceUsbPids.Contains(device.Pid)
                         select Task.Run(() =>
                         {
                             try
                             {
                                 //Open the port and wait for the first message.
                                 var serialPort = OpenSerialPort(device.Port);
                                 if (serialPort == null) return null;

                                 serialPort.ReadLine();
                                 var firstMessage = serialPort.ReadLine().Trim();
                                 if (String.CompareOrdinal(firstMessage, StartupMessage) == 0) return serialPort;

                                 Logger.Log($"Device not found on port {device} - Data = {firstMessage}", Category.Info, Priority.None);

                                 serialPort.Dispose();
                                 return null;
                             }
                             catch (Exception)
                             {
                                 Logger.Log($"Device not found on port {device}", Category.Info, Priority.None);
                                 return null;
                             }
                         })).ToList();

            while (tasks.Any())
            {
                var completed = await Task.WhenAny(tasks);
                if (completed.Result != null)
                {
                    Logger.Log($"Found device on port {completed.Result.PortName}!", Category.Info, Priority.None);
                    return completed.Result;
                }

                tasks.Remove(completed);
            }

            return null;
        }

        public static IEnumerable<Tuple<string, string>> GetAvailableDevices()
        {
            return from device in UsbSerialDeviceEnumerator.EnumerateDevices()
            select new Tuple<string, string>(device.FriendlyName, device.Port);
        }

        public static SerialPort OpenSerialPort(string port)
        {
            try
            {
                Logger.Log($"Opening port {port}.", Category.Info, Priority.None);
                var serialPort = new SerialPort(port, 9600);
                serialPort.DtrEnable = false;

                serialPort.ReadTimeout = 5000;
                serialPort.Open();

                serialPort.DiscardInBuffer();
                serialPort.DtrEnable = true;//this will reset the Arduino.


                if (!serialPort.IsOpen) return null; //sanity check.
                return serialPort;

            }
            catch (UnauthorizedAccessException)
            {
                // Access is denied to the port. 
                // -or -
                // The current process, or another process on the system, already has the specified COM port open either by a SerialPort instance or in unmanaged code.
                return null;
            }
            catch (ArgumentException)
            {
                //Configuration shold start with "COM"
                return null;
            }
            catch (IOException)
            {
                //The port is in an invalid state.
                // -or -
                //An attempt to set the state of the underlying port failed. For example, the parameters passed from this SerialPort object were invalid.
                return null;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}
