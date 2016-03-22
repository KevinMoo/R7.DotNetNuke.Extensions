﻿//
//  SettingsWrapper.cs
//
//  Author:
//       Roman M. Yagodin <roman.yagodin@gmail.com>
//
//  Copyright (c) 2014, 2015 Roman M. Yagodin
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Collections;
using DotNetNuke.UI.Modules;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.R7
{
    /// <summary>
    /// Provides strong typed access to settings used by module
    /// </summary>
    public class SettingsWrapper
    {
        protected ModuleController ctrl;
        protected int ModuleId;
        protected int TabModuleId;
        protected Hashtable settings;

        private SettingsWrapper (int moduleId, int tabModuleId)
        {
            Init (moduleId, tabModuleId);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetNuke.R7.SettingsWrapper"/> class.
        /// </summary>
        /// <param name='module'>
        /// Module control.
        /// </param>
        public SettingsWrapper (IModuleControl module) : this (module.ModuleContext.ModuleId, module.ModuleContext.TabModuleId)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetNuke.R7.SettingsWrapper"/> class
        /// and should only be used in generic classes along with <see cref="DotNetNuke.R7.SettingsWrapper.Init" /> call.
        /// </summary>
        public SettingsWrapper ()
        {
        }

        public void Init (int moduleId, int tabModuleId)
        {
            ctrl = new ModuleController ();
            ModuleId = moduleId;
            TabModuleId = tabModuleId;
            var tabModule = ctrl.GetTabModule (tabModuleId);

            // taken from PortalModuleBase settings definition
            settings = new Hashtable (tabModule.ModuleSettings);

            // combine settings
            foreach (string key in tabModule.TabModuleSettings.Keys)
                settings [key] = tabModule.TabModuleSettings [key];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetNuke.R7.SettingsWrapper"/> class.
        /// </summary>
        /// <param name='module'>
        /// Module info.
        /// </param>
        public SettingsWrapper (ModuleInfo module) : this (module.ModuleID, module.TabModuleID)
        {
        }

        /// <summary>
        /// Reads module setting.
        /// </summary>
        /// <returns>
        /// The setting value.
        /// </returns>
        /// <param name='settingName'>
        /// Setting name.
        /// </param>
        /// <param name='defaultValue'>
        /// Default value for setting.
        /// </param>
        /// <typeparam name='T'>
        /// Type of the setting
        /// </typeparam>
        protected T ReadSetting<T> (string settingName, T defaultValue)
        {
            T ret;

            if (settings.ContainsKey (settingName))
            {
                var tc = TypeDescriptor.GetConverter (typeof(T));
                try
                {
                    ret = (T)tc.ConvertFrom (settings [settingName]);
                }
                catch
                {
                    ret = defaultValue;
                }
            }
            else
                ret = defaultValue;

            return ret;
        }

        /// <summary>
        /// Writes module or tabmodule setting.
        /// </summary>
        /// <param name='settingName'>
        /// Setting name.
        /// </param>
        /// <param name='value'>
        /// Setting value.
        /// </param>
        /// <param name='tabSpecific'>
        /// If set to <c>true</c>, setting is for this module on current tab.
        /// If set to <c>false</c>, setting is for this module on all tabs.
        /// </param>
        protected void WriteSetting<T> (string settingName, T value, bool tabSpecific)
        {
            if (tabSpecific)
                ctrl.UpdateTabModuleSetting (TabModuleId, settingName, value.ToString ());
            else
                ctrl.UpdateModuleSetting (ModuleId, settingName, value.ToString ());
        }

        /// <summary>
        /// Writes module setting.
        /// </summary>
        /// <param name='settingName'>
        /// Setting name.
        /// </param>
        /// <param name='value'>
        /// Setting value.
        /// </param>
        protected void WriteModuleSetting<T> (string settingName, T value)
        {
            ctrl.UpdateModuleSetting (ModuleId, settingName, value.ToString ());
        }

        /// <summary>
        /// Writes tabmodule setting.
        /// </summary>
        /// <param name='settingName'>
        /// Setting name.
        /// </param>
        /// <param name='value'>
        /// Setting value.
        /// </param>
        protected void WriteTabModuleSetting<T> (string settingName, T value)
        {
            ctrl.UpdateTabModuleSetting (TabModuleId, settingName, value.ToString ());
        }

        /// <summary>
        /// Gets the raw module settings hashtable.
        /// </summary>
        /// <value>The settings.</value>
        public Hashtable Settings 
        {
            get { return settings; }
        }
    }
    // class
}
// namespace

