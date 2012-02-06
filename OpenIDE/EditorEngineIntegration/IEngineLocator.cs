using System;
namespace OpenIDE.EditorEngineIntegration
{
	public interface ILocateEditorEngine
	{
		Instance GetInstance(string path);
	}
}

