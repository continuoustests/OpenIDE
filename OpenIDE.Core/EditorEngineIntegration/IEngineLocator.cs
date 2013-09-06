using System;
using System.Collections.Generic;
namespace OpenIDE.Core.EditorEngineIntegration
{
	public interface ILocateEditorEngine
	{
		List<Instance> GetInstances();
		Instance GetInstance(string path);
	}
}

