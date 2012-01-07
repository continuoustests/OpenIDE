using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenIDENet.Arguments;
namespace OpenIDENet.Bootstrapping
{
	public static class Bootstrapper
	{
		private static DIContainer _container;

		public static AppSettings Settings = new AppSettings(); 
		
		public static void Initialize()
		{
			_container = new DIContainer();
		}
		
		public static ICommandDispatcher GetDispatcher()
		{
			return _container.GetDispatcher();
		}

		public static IEnumerable<ICommandHandler> GetCommandHandlers()
		{
			return _container.ICommandHandlers();
		}
	}

	public class AppSettings
	{
		private const string DEFAULT_LANGUAGE = "--default-language=";
		public string DefaultLanguage { get; set; }

		public string[] Parse(string[] args)
		{
			var newArgs = new List<string>();
			foreach (var arg in args)
			{
				if (arg.StartsWith(DEFAULT_LANGUAGE))
				{
					DefaultLanguage = arg
						.Substring(DEFAULT_LANGUAGE.Length, arg.Length - DEFAULT_LANGUAGE.Length);
					continue;
				}
				newArgs.Add(arg);
			}
			return newArgs.ToArray();
		}
	}
}
