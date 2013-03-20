using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace oipckmngr
{
	public class Package
	{
		public static Package Read(string json) {
			try {
				var data = JObject.Parse(json);
				var package = new Package(data["target"].ToString(), data["id"].ToString(), data["description"].ToString());
				if (data["pre-install-actions"] != null) {
					data["pre-install-actions"].Children().ToList()
						.ForEach(x => package.AddPreInstallAction(x.ToString()));
				}
				if (data["dependencies"] != null) {
					data["dependencies"].Children().ToList()
						.ForEach(x => package.AddDependency(x.ToString()));
				}
				if (data["post-install-actions"] != null) {
					data["post-install-actions"].Children().ToList()
						.ForEach(x => package.AddPostInstallAction(x.ToString()));
				}
				if (package.IsValid())
					return package;
			} catch (Exception ex) {
				Console.WriteLine(ex.ToString());
			}
			return null;
		}

		public string Target { get; set; }
		public string ID { get; set; }
		public string Description { get; set; }
		public List<string> PreInstallActions = new List<string>();
		public List<string> Dependencies = new List<string>();
		public List<string> PostInstallActions = new List<string>();

		public Package(string target, string id, string description) {
			Target = target;
			ID = id;
			Description = description;
		}

		public Package AddPreInstallAction(string item) {
			PreInstallActions.Add(item);
			return this;
		}

		public Package AddDependency(string item) {
			Dependencies.Add(item);
			return this;
		}

		public Package AddPostInstallAction(string item) {
			PostInstallActions.Add(item);
			return this;
		}

		public bool IsValid() {
			return 
				new[] { "language", "script", "rscript" }.Contains(Target) &&
				ID.Length > 0 &&
				Description.Length > 0;
		}

		public string Write() {
			var sb = new StringBuilder();
			sb.AppendLine("{");
			sb.AppendLine(string.Format("\t\"target\": \"{0}\",", Target));
			sb.AppendLine(string.Format("\t\"id\": \"{0}\",", ID));
			sb.AppendLine(string.Format("\t\"description\": \"{0}\",", Description));
			sb.AppendLine("\t\"dependencies\":");
			sb.AppendLine(
				getArrayOf(
					Dependencies.OfType<object>(),
					(itm,tabs) => string.Format("\"{0}\"", itm.ToString()),
					2) + ",");
			sb.AppendLine("\t\"pre-install-actions\":");
			sb.AppendLine(
				getArrayOf(
					PreInstallActions.OfType<object>(),
					(itm,tabs) => string.Format("\"{0}\"", itm.ToString()),
					2) + ",");
			sb.AppendLine("\t\"post-install-actions\":");
			sb.AppendLine(
				getArrayOf(
					PostInstallActions.OfType<object>(),
					(itm,tabs) => string.Format("\"{0}\"", itm.ToString()),
					2));
			sb.Append("}");
			return sb.ToString();
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