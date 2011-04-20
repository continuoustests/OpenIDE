using System;
namespace OpenIDENet.EditorEngineIntegration
{
	interface ILocateEditorEngine
	{
		Instance GetInstance(string path);
	}
}

