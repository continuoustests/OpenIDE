using System;
using CSharp.Files;
using CSharp.Versioning;

namespace CSharp.Projects
{
	public interface IProjectHandler
	{
		string Fullpath { get; }
		string Type { get; }
		string DefaultNamespace { get; }

		bool Read(string location, Func<string> getTypesProviderByLocation);
		void AppendFile(IFile file);
		void Reference(IFile file);
		void Dereference(IFile file);
		void Write();
	}

	class ProjectHandler : IProjectHandler
	{
		private Project _project;
		private IProvideVersionedTypes _with;
		
		public string Fullpath { get { return _project.Fullpath; } }
		public string Type { get { return _project.Settings.Type; } }
		public string DefaultNamespace { get { return _project.Settings.DefaultNamespace; } }

		public bool Read(string location, Func<string> getTypesProviderByLocation)
		{
			_project = null;
			_provider = getTypesProviderByLocation(location);
			if (_provider == null)
				return false;
			_with = (IProvideVersionedTypes) _provider.TypesProvider;
			if (_with == null)
				return false;
			_project = _with.Reader().Read(_provider.ProjectFile);
			return _project != null;
		}

		public void AppendFile(IFile file)
		{
			_with.FileAppenderFor(file).Append(_project, file);
		}

		public void Reference(IFile file)
		{
			_with.ReferencerFor(file).Reference(_project, file);	
		}

		public void Dereference(IFile file)
		{
			_with.DereferencerFor(file).Dereference(_project, file);
		}

		public void Write()
		{
			_with.Writer().Write(_project);
		}
	}	
}
