using System;
using System.Linq;
using System.Collections.Generic;
using OpenIDE.Core.Profiles;

namespace OpenIDE.Core.Config
{
	public class ConfigReader
	{
		private string _token;
		private ProfileLocator _locator;

		public ConfigReader(string token) {
			_token = token;
			_locator = new ProfileLocator(_token);
		}

		public string Get(string settingName) {
			// Get from local profile
			var localProfile = _locator.GetActiveLocalProfile();
			var path = _locator.GetLocalProfilePath(localProfile);
			var value = valueFromConfig(path, settingName);
			if (value != null)
				return value;

			// Get from local default profile
			if (localProfile != "default") {
				path = _locator.GetLocalProfilePath("default");
				value = valueFromConfig(path, settingName);
				if (value != null)
					return value;
			}

			// Get from global profile
			var globalProfile = _locator.GetActiveGlobalProfile();
			path = _locator.GetGlobalProfilePath(globalProfile);
			value = valueFromConfig(path, settingName);
			if (value != null)
				return value;

			// Get from global default profile
			if (globalProfile != "default") {
				path = _locator.GetGlobalProfilePath("default");
				value = valueFromConfig(path, settingName);
				if (value != null)
					return value;
			}
			return null;
		}

		public ConfigurationSetting[] GetStartingWith(string settingName) {
			// Get from local profile
			var results = new List<ConfigurationSetting>();
			var localProfile = _locator.GetActiveLocalProfile();
			var path = _locator.GetLocalProfilePath(localProfile);
			valuesFromConfig(path, settingName, results);

			// Get from local default profile
			if (localProfile != "default") {
				path = _locator.GetLocalProfilePath("default");
				valuesFromConfig(path, settingName, results);
			}

			// Get from global profile
			var globalProfile = _locator.GetActiveGlobalProfile();
			path = _locator.GetGlobalProfilePath(globalProfile);
			valuesFromConfig(path, settingName, results);

			// Get from global default profile
			if (globalProfile != "default") {
				path = _locator.GetGlobalProfilePath("default");
				valuesFromConfig(path, settingName, results);
			}
			return results.ToArray();
		}

		private string valueFromConfig(string path, string name) {
			var cfg = new Configuration(path, false);
			var setting = cfg.Get(name);
			if (setting == null)
				return null;
			return setting.Value;
		}

		private void valuesFromConfig(string path, string name, List<ConfigurationSetting> results) {
			var cfg = new Configuration(path, false);
			cfg.GetSettingsStartingWith(name).ToList()
				.ForEach(x => {
						if (!results.Any(y => y.Key == x.Key))
							results.Add(x);
					});
		}
	}
}