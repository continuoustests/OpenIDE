using System;
using System.Collections.Generic;
namespace OpenIDENet.Projects
{
	public interface IProject
	{
		string Fullpath { get; }
		string Xml { get; }
	}
}

