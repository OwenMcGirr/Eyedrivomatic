﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Eyedrivomatic.Configuration {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.1.0.0")]
    internal sealed partial class DwellClickConfiguration : global::System.Configuration.ApplicationSettingsBase {
        
        private static DwellClickConfiguration defaultInstance = ((DwellClickConfiguration)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new DwellClickConfiguration())));
        
        public static DwellClickConfiguration Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int SettingsVersion {
            get {
                return ((int)(this["SettingsVersion"]));
            }
            set {
                this["SettingsVersion"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool EnableDwellClick {
            get {
                return ((bool)(this["EnableDwellClick"]));
            }
            set {
                this["EnableDwellClick"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("200")]
        public int DwellTimeoutMilliseconds {
            get {
                return ((int)(this["DwellTimeoutMilliseconds"]));
            }
            set {
                this["DwellTimeoutMilliseconds"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("250")]
        public int RepeatDelayMilliseconds {
            get {
                return ((int)(this["RepeatDelayMilliseconds"]));
            }
            set {
                this["RepeatDelayMilliseconds"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("700")]
        public int StandardDwellTimeMilliseconds {
            get {
                return ((int)(this["StandardDwellTimeMilliseconds"]));
            }
            set {
                this["StandardDwellTimeMilliseconds"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("700")]
        public int DirectionButtonDwellTimeMilliseconds {
            get {
                return ((int)(this["DirectionButtonDwellTimeMilliseconds"]));
            }
            set {
                this["DirectionButtonDwellTimeMilliseconds"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("200")]
        public int StopButtonDwellTimeMilliseconds {
            get {
                return ((int)(this["StopButtonDwellTimeMilliseconds"]));
            }
            set {
                this["StopButtonDwellTimeMilliseconds"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1000")]
        public int StartButtonDwellTimeMilliseconds {
            get {
                return ((int)(this["StartButtonDwellTimeMilliseconds"]));
            }
            set {
                this["StartButtonDwellTimeMilliseconds"] = value;
            }
        }
    }
}
