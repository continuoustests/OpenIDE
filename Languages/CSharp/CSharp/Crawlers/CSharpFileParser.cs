using System;
using System.Collections.Generic;
using System.Linq;
using CSharp.Projects;

namespace CSharp.Crawlers
{
    public interface ICSharpParser
    {
        ICSharpParser SetOutputWriter(IOutputWriter writer);
        ICSharpParser ParseLocalVariables();
        void ParseFile(FileRef file, Func<string> getContent);
    }

	enum Location
	{
        Unknown,
		Root,
		Namespace,
		Class,
        Interface,
		Struct,
		Enum,
		Method
	}

	class LocationHierarchyActivity
	{
		public bool Push { get; set; }
		public Location Location { get; set; }
	}

    public class CSharpFileParser : ICSharpParser
    {
        private object _padLock = new object();
        private FileRef _file;
        private string _content;
        private Location _suggestedLocation = Location.Unknown;
        private Location _currentLocation;
		private IOutputWriter _builder;
        private Stack<Location> _locationHierarchy = new Stack<Location>();
		private List<LocationHierarchyActivity> _locationHierarchyActivity = 
													new List<LocationHierarchyActivity>();
        private CSharpCodeNavigator _navigator;

        private Namespce _currentNamespace = null;

        public ICSharpParser SetOutputWriter(IOutputWriter writer)
        {
            _builder = writer;
            return this;
        }

        public ICSharpParser ParseLocalVariables() {
            return this;
        }

        public void ParseFile(FileRef file, Func<string> getContent)
        {
            lock (_padLock)
            {
                _builder.WriteFile(file);
                _file = file;
                _content = getContent();
                _currentLocation = Location.Root;
                _currentNamespace = null;
                _navigator = new CSharpCodeNavigator(
                    _content.ToCharArray(),
                    () =>
                    {
                        _locationHierarchy.Push(_currentLocation);
						_locationHierarchyActivity.Add(
							new LocationHierarchyActivity() { Push = true, Location = _currentLocation });
                        _currentLocation = _suggestedLocation;
                        _suggestedLocation = Location.Unknown;
                    },
                    () =>
					{
                        _currentLocation = _locationHierarchy.Pop();
						_locationHierarchyActivity.Add(
							new LocationHierarchyActivity() { Push = false, Location = _currentLocation });
					},
					(ifdef) =>
						positionForIfDef(ifdef));
                parse();
            }
        }

		private void positionForIfDef(IfDef ifdef)
		{
			if (ifdef == IfDef.If)
				_locationHierarchyActivity = new List<LocationHierarchyActivity>();
			else if (ifdef == IfDef.Else)
				revertLocationHierarchy();
		}

		private void revertLocationHierarchy()
		{
			_locationHierarchyActivity.Reverse();
			_locationHierarchyActivity
				.ForEach(activity =>
					{
						if (activity.Push)
							_locationHierarchy.Pop();
						else
							_locationHierarchy.Push(activity.Location);
					});
			_locationHierarchyActivity = new List<LocationHierarchyActivity>();
		}

        private void parse()
        {
            Word word;
            while ((word = _navigator.GetWord()) != null)
            {
                if (_currentLocation == Location.Root)
                    whenInRoot(word);
                else if (_currentLocation == Location.Namespace)
                    whenInNamespace(word);
            }
        }

        private void whenInRoot(Word word)
        {
            if (word.Text == "namespace")
                handleNamespace(word);
        }

        private void whenInNamespace(Word word)
        {
            if (word.Text == "class")
                handleClass(word);
            if (word.Text == "interface")
                handleInterface(word);
            if (word.Text == "struct")
                handleStruct(word);
            if (word.Text == "enum")
                handleEnum(word);

        }

        private void handleNamespace(Word word)
        {
            suggestLocation(Location.Namespace);
            var signature = _navigator.CollectSignature();
            var ns = new Namespce(
                _file,
                signature.Text,
                signature.Line,
                signature.Column + 1);
            _builder.WriteNamespace(ns);
            _currentNamespace = ns;
        }

        private void handleClass(Word word)
        {
            suggestLocation(Location.Class);
            var signature = _navigator.CollectSignature();
            var ns = "";
            if (_currentNamespace != null)
                ns = _currentNamespace.Name;
            _builder.WriteClass(
                new Class(
                    _file,
                    ns,
                    getNameFromSignature(signature.Text),
                    "",
                    signature.Line,
                    signature.Column + 1));
        }

        private void handleInterface(Word word)
        {
            suggestLocation(Location.Interface);
            var signature = _navigator.CollectSignature();
            var ns = "";
            if (_currentNamespace != null)
                ns = _currentNamespace.Name;
            _builder.WriteInterface(
                new Interface(
                    _file,
                    ns,
                    getNameFromSignature(signature.Text),
                    "",
                    signature.Line,
                    signature.Column + 1));
        }

        private void handleStruct(Word word)
        {
            suggestLocation(Location.Struct);
            var signature = _navigator.CollectSignature();
            var ns = "";
            if (_currentNamespace != null)
                ns = _currentNamespace.Name;
            _builder.WriteStruct(
                new Struct(
                    _file,
                    ns,
                    signature.Text,
                    "",
                    signature.Line,
                    signature.Column + 1));
        }

        private void handleEnum(Word word)
        {
            suggestLocation(Location.Enum);
            var signature = _navigator.CollectSignature();
            var ns = "";
            if (_currentNamespace != null)
                ns = _currentNamespace.Name;
            _builder.WriteEnum(
                new EnumType(
                    _file,
                    ns,
                    signature.Text,
                    "",
                    signature.Line,
                    signature.Column + 1));
        }

        private string getNameFromSignature(string signature)
        {
            if (signature.IndexOf(":") != -1)
                return signature.Substring(0, signature.IndexOf(":"));
            return signature;
        }

        private void suggestLocation(Location location)
        {
            _suggestedLocation = location;
        }
    }
}

