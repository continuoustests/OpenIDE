using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenIDENet.Arguments;
using OpenIDENet.Arguments.Handlers;
namespace OpenIDENet.Bootstrapping
{
	public static class Bootstrapper
	{
		private static DIContainer _container;

		public static AppSettings Settings = null; 
		
		public static void Initialize()
		{
			_container = new DIContainer();
			Settings = new AppSettings(
				GetCommandHandlers()
					.Where(x => x.GetType().Equals(typeof(LanguageHandler))));
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

		private ICommandHandler[] _handlers;

		public string DefaultLanguage { get; set; }

		public AppSettings(IEnumerable<ICommandHandler> handlers)
		{
			_handlers = handlers.ToArray();
		}

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
			if (DefaultLanguage != null && newArgs.Count > 0)
			{
				var command = 
					_handlers
						.FirstOrDefault(x => x.Command.Equals(DefaultLanguage));
				if (command != null)
				{
					var usage = command.Usage;
					if (usage != null)
					{
						if (usage.Parameters.Count(x => x.Name.Equals(newArgs[0])) > 0)
							newArgs.Insert(0, DefaultLanguage);
					}
				}
			}
			return newArgs.ToArray();
		}
	}
}
