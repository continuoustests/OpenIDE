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
				var package = new Package(data["id"].ToString(), data["description"].ToString());
				data["pre-install-actions"].Children().ToList()
					.ForEach(x => package.AddPreInstallAction(x.ToString()));

				var contents = data["contents"].Children();
				contents.ToList()
					.ForEach(x => {
							var content = new PackageContent(x["target"].ToString(), x["file-root"].ToString());
							x["pre-install-actions"].Children().ToList()
								.ForEach(y => content.AddPreInstallAction(y.ToString()));
							x["post-install-actions"].Children().ToList()
								.ForEach(y => content.AddPostInstallAction(y.ToString()));
							package.AddContent(content);
						});

				data["dependencies"].Children().ToList()
					.ForEach(x => package.AddDependency(x.ToString()));
				data["post-install-actions"].Children().ToList()
					.ForEach(x => package.AddPostInstallAction(x.ToString()));
				if (package.IsValid())
					return package;
			} catch {
			}
			return null;
		}

		public string ID { get; set; }
		public string Description { get; set; }
		public List<string> PreInstallActions = new List<string>();
		public List<PackageContent> Contents = new List<PackageContent>();
		public List<string> Dependencies = new List<string>();
		public List<string> PostInstallActions = new List<string>();

		public Package(string id, string description) {
			ID = id;
			Description = description;
		}

		public Package AddPreInstallAction(string item) {
			PreInstallActions.Add(item);
			return this;
		}

		public Package AddContent(PackageContent content) {
			Contents.Add(content);
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
				ID.Length > 0 &&
				Description.Length > 0 &&
				(
					PreInstallActions.Count > 0 ||
					(
						Contents.Count > 0 &&
						!Contents.Any(x => !x.IsValid())
					) ||
					PostInstallActions.Count > 0
				);
		}

		public string Write() {
			var sb = new StringBuilder();
			sb.AppendLine("{");
			sb.AppendLine(string.Format("\t\"id\": \"{0}\",", ID));
			sb.AppendLine(string.Format("\t\"description\": \"{0}\",", Description));
			sb.AppendLine("\t\"pre-install-actions\":");
			sb.AppendLine(
				getArrayOf(
					PreInstallActions.OfType<object>(),
					(itm,tabs) => string.Format("\"{0}\"", itm.ToString()),
					2) + ",");
			sb.AppendLine("\t\"contents\":");
			sb.AppendLine(
				getArrayOf(
					Contents.OfType<object>(),
					(itm,tabNum) => ((PackageContent)itm).ToJSONValue(getArrayOf, tabNum),
					2) + ",");
			sb.AppendLine("\t\"dependencies\":");
			sb.AppendLine(
				getArrayOf(
					Dependencies.OfType<object>(),
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

	public class PackageContent
	{
		public List<string> PreInstallActions = new List<string>();
		public string Target { get; set; }
		public string FileRoot { get; set; }
		public List<string> PostInstallActions = new List<string>();

		public PackageContent(string target, string fileroot) {
			Target = target;
			FileRoot = fileroot;
		}

		public PackageContent AddPreInstallAction(string item) {
			PreInstallActions.Add(item);
			return this;
		}

		public PackageContent AddPostInstallAction(string item) {
			PostInstallActions.Add(item);
			return this;
		}

		public bool IsValid() {
			return 
				new[] {"language","language-script","script"}.Contains(Target) &&
				(
					FileRoot.Length > 0 ||
					PreInstallActions.Count > 0 ||
					PostInstallActions.Count > 0
				);
		}

		public string ToJSONValue(Func<IEnumerable<object>,Func<object,int,string>, int, string> arrayWriter, int tabs)  {
			var sb = new StringBuilder();
			sb.AppendLine("{");
			sb.AppendLine(tab(tabs + 1) + "\"pre-install-actions\":");
			sb.AppendLine(
				arrayWriter(
					PreInstallActions.OfType<object>(),
					(itm,tbs) => string.Format("\"{0}\"", itm.ToString()),
					tabs + 2) + ",");
			sb.AppendLine(tab(tabs + 1) + string.Format("\"target\": \"{0}\",", Target));
			sb.AppendLine(tab(tabs + 1) + string.Format("\"file-root\": \"{0}\",", FileRoot));
			sb.AppendLine(tab(tabs + 1) + "\"post-install-actions\":");
			sb.AppendLine(
				arrayWriter(
					PostInstallActions.OfType<object>(),
					(itm,tbs) => string.Format("\"{0}\"", itm.ToString()),
					tabs + 2));
			sb.Append(tab(tabs) + "}");
			return sb.ToString();
		}

		private string tab(int num) {
			return "\t".PadRight(num, '\t');
		}
	}
}