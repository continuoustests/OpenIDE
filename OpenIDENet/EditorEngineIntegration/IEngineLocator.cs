using System;
namespace OpenIDENet.EditorEngineIntegration
{
	public interface ILocateEditorEngine
	{
		Instance GetInstance(string path);
	}
}

