using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using OpenIDE.Core.Logging;

namespace OpenIDE.Core.Packaging 
{
	public class Package
	{
		public class InstallAction
		{
			public string Action { get; set; }
			public string Global { get; set; }

			public override string ToString() {
				if (Global != null) {
					return "{ \"local\": \""+Action+"\", \"global\": \""+Global+"\" }";
				}
				return "\""+Action+"\"";
			}
		}

		public static Package Read(string json, string file) {
			return read(file, json);
		}

		public static Package Read(string file) {
			return read(file, System.IO.File.ReadAllText(file));
		}

		private static Package read(string file, string json) {
			try {
				var data = JObject.Parse(json);
				var os = new List<string>();
				data["os"].Children().ToList()
						.ForEach(x => os.Add(x.ToString()));
				var package = 
					new Package(
						os.ToArray(),
						data["target"].ToString(),
						data["id"].ToString(), 
						data["version"].ToString(), 
						data["command"].ToString(), 
						data["name"].ToString(), 
						data["description"].ToString());
				var language = data["language"];
				if (language != null)
					package.Language = language.ToString();
				package.File = file;
				if (data["pre-install-actions"] != null) {
					data["pre-install-actions"].Children().ToList()
						.ForEach(x => {
							var action = parseAction(x);
							package.AddPreInstallAction(action.Action, action.Global);
						});
				}
				if (data["dependencies"] != null) {
					data["dependencies"].Children().ToList()
						.ForEach(x => 
							package.AddDependency(
								x["id"].ToString(),
								x["versions"].Children()
									.Select(y => y.ToString())));
				}
				if (data["post-install-actions"] != null) {
					data["post-install-actions"].Children().ToList()
						.ForEach(x => {
							var action = parseAction(x);
							package.AddPostInstallAction(action.Action, action.Global);
						});
				}
				if (data["pre-uninstall-actions"] != null) {
					data["pre-uninstall-actions"].Children().ToList()
						.ForEach(x => {
							var action = parseAction(x);
							package.AddPreUninstallAction(action.Action, action.Global);
						});
				}
				if (data["post-uninstall-actions"] != null) {
					data["post-uninstall-actions"].Children().ToList()
						.ForEach(x => {
							var action = parseAction(x);
							package.AddPostUninstallAction(action.Action, action.Global);
						});
				}
				if (package.IsValid())
					return package;
			} catch (Exception ex) {
				Logger.Write(ex);
			}
			return null;
		}

		private static InstallAction parseAction(JToken x) {
			var local = x["local"];
			var global = x["global"];
			if (local != null || global != null) {
				return new InstallAction() {
					Action = local.ToString(),
					Global = global.ToString()
				};
			}
			return new InstallAction() {
				Action = x.ToString()
			};
		}

		public class Dependency
		{
			private List<string> _versions = new List<string>();
			public string ID { get; set; }
			public string[] Versions { get { return _versions.ToArray(); } }

			public Dependency AddVersion(string version) {
				_versions.Add(version);
				return this;
			}

			public Dependency AddVersions(IEnumerable<string> versions) {
				_versions.AddRange(versions);
				return this;
			}
		}

		private List<string> _os = new List<string>();

		public string File { get; set; }
		public string[] OS { get { return _os.ToArray(); } }
		public string Target { get; set; }
		public string Language { get; set; }
		public string Signature { get{ return ID + "-" + Version; } }
		public string ID { get; set; }
		public string Version { get; set; }
		public string Command { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public List<InstallAction> PreInstallActions = new List<InstallAction>();
		public List<Dependency> Dependencies = new List<Dependency>();
		public List<InstallAction> PostInstallActions = new List<InstallAction>();

		public List<InstallAction> PreUninstallActions = new List<InstallAction>();
		public List<InstallAction> PostUninstallActions = new List<InstallAction>();

		public Package(string[] os, string target, string id, string version, string command, string name, string description) {
			_os.AddRange(os);
			Target = target;
			ID = id;
			Version = version;
			Command = command;
			Name = name;
			Description = description;
		}

		public Package AddPreInstallAction(string item) {
			return AddPreInstallAction(item, null);
		}

		public Package AddPreInstallAction(string item, string global) {
			var action = new InstallAction() {
				Action = item,
				Global = global
			};
			PreInstallActions.Add(action);
			return this;
		}

		public Package AddDependency(string name, IEnumerable<string> versions) {
			Dependencies.Add((new Dependency() { ID = name}).AddVersions(versions));
			return this;
		}

		public Package AddPostInstallAction(string item) {
			return AddPostInstallAction(item, null);
		}

		public Package AddPostInstallAction(string item, string global) {
			var action = new InstallAction() {
				Action = item,
				Global = global
			};
			PostInstallActions.Add(action);
			return this;
		}

		public Package AddPreUninstallAction(string item) {
			return AddPreUninstallAction(item, null);
		}

		public Package AddPreUninstallAction(string item, string global) {
			var action = new InstallAction() {
				Action = item,
				Global = global
			};
			PreUninstallActions.Add(action);
			return this;
		}

		public Package AddPostUninstallAction(string item) {
			return AddPostUninstallAction(item, null);
		}

		public Package AddPostUninstallAction(string item, string global) {
			var action = new InstallAction() {
				Action = item,
				Global = global
			};
			PostUninstallActions.Add(action);
			return this;
		}

		public bool IsValid() {
			var basicValid = 
				new[] { "language", "script", "rscript", "language-script", "language-rscript" }.Contains(Target) &&
				ID.Length > 0 &&
				Version.Length > 0 &&
				Command.Length > 0 &&
				Name.Length > 0 &&
				Name.Length <= 50 &&
				Description.Length > 0;
			if (!basicValid)
				return false;
			if (_os.Count == 0)
				return false;
			foreach (var os in _os) {
				if (new[] { "windows", "linux", "osx" }.Contains(os))
					continue;
				return false;
			}
			if ((Target == "language-script" || Target == "language-rscript") && (Language == null || Language.Length == 0))
				return false;
			return true;
		}

		public string Write() {
			var sb = new StringBuilder();
			sb.AppendLine("{");
			sb.AppendLine("\t\"os\":");
			sb.AppendLine(
				getArrayOf(
					_os.OfType<object>(),
					(itm,tabs) => string.Format("\"{0}\"", itm.ToString()),
					2) + ",");
			sb.AppendLine(string.Format("\t\"target\": \"{0}\",", Target));
			if (Language != null)
				sb.AppendLine(string.Format("\t\"language\": \"{0}\",", Language));
			sb.AppendLine(string.Format("\t\"id\": \"{0}\",", ID));
			sb.AppendLine(string.Format("\t\"version\": \"{0}\",", Version));
			sb.AppendLine(string.Format("\t\"command\": \"{0}\",", Command));
			sb.AppendLine(string.Format("\t\"name\": \"{0}\",", Name));
			sb.AppendLine(string.Format("\t\"description\": \"{0}\",", Description));
			sb.AppendLine("\t\"dependencies\":");
			sb.AppendLine(
				getArrayOf(
					Dependencies.OfType<object>(),
					(itm,tabs) => {
						var depSb = new StringBuilder();
						depSb.AppendLine("{");
						depSb.AppendLine(
							tab(tabs + 1) + 
							"\"id\": \"{0}\"".Replace("{0}", ((Dependency)itm).ID) +
							",");
						depSb.AppendLine(tab(tabs + 1) + "\"versions\":");
						depSb.AppendLine(
							getArrayOf(
								((Dependency)itm).Versions.OfType<object>(),
								(dep,depTabs) => "\"{1}\"".Replace("{1}", dep.ToString()),
								4));
						depSb.Append(tab(tabs) + "}");
						return depSb.ToString();
					},
					2) + ",");
			sb.AppendLine("\t\"pre-install-actions\":");
			sb.AppendLine(
				getArrayOf(
					PreInstallActions.OfType<object>(),
					(itm,tabs) => itm.ToString(),
					2) + ",");
			sb.AppendLine("\t\"post-install-actions\":");
			sb.AppendLine(
				getArrayOf(
					PostInstallActions.OfType<object>(),
					(itm,tabs) => itm.ToString(),
					2) + ",");
			sb.AppendLine("\t\"pre-uninstall-actions\":");
			sb.AppendLine(
				getArrayOf(
					PreUninstallActions.OfType<object>(),
					(itm,tabs) => itm.ToString(),
					2) + ",");
			sb.AppendLine("\t\"post-uninstall-actions\":");
			sb.AppendLine(
				getArrayOf(
					PostUninstallActions.OfType<object>(),
					(itm,tabs) => itm.ToString(),
					2));
			sb.Append("}");
			return sb.ToString();
		}

		public string ToVerboseString() {
			var sb = new StringBuilder();
			sb.AppendLine("Location:\t" + File);
			sb.AppendLine("Target:\t\t" + Target);
			if (Language != null)
				sb.AppendLine("Language:\t" + Language);
			sb.AppendLine("ID:\t\t" + ID);
			sb.AppendLine("Version:\t" + Version);
			sb.AppendLine();
			sb.AppendLine("Description:" + Environment.NewLine + Description);
			if (Dependencies.Count > 0) {
				sb.AppendLine();
				sb.AppendLine("Dependencies");
				Dependencies.ToList().ForEach(x => sb.AppendLine("\t" + x));
			}
			return sb.ToString();
		}

		public override string ToString() {
			return Target + " - " + Signature;
		}

		private string getArrayOf(IEnumerable<object> list, Func<object,int,string> toJSONValue, int tabs) {
			var sb = new StringBuilder();
			sb.AppendLine(tab(tabs) + "[");
			for (int i = 0; i < list.Count(); i++) {
				if (i < list.Count() - 1)
					sb.AppendLine(tab(tabs + 1) + toJSONValue(list.ElementAt(i),tabs + 1) + ",");
				else
					sb.AppendLine(tab(tabs + 1) + toJSONValue(list.ElementAt(i),tabs + 1));
			}
			sb.Append(tab(tabs) + "]");
			return sb.ToString();
		}

		private string tab(int num) {
			return "\t".PadRight(num, '\t');
		}
	}
}
