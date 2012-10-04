using System;
namespace OpenIDE.Core.EditorEngineIntegration
{
	public interface ILocateEditorEngine
	{
		Instance GetInstance(string path);
	}
}

