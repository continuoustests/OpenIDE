using System;
using System.Linq;
using System.Windows.Forms;
using OpenIDENet.CodeEngine.Core.Caching;
using OpenIDENet.CodeEngine.Core.Endpoints;
using System.IO;
using System.Threading;
using System.Reflection;
using OpenIDENet.CodeEngine.Core.ChangeTrackers;
using OpenIDENet.CodeEngine.Core.Logging;
using OpenIDENet.CodeEngine.Core.UI;
using OpenIDENet.CodeEngine.Core.EditorEngine;
using System.Drawing;
using OpenIDENet.Core.Language;
using OpenIDENet.Core.Windowing;

namespace OpenIDENet.CodeEngine
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			if (args.Length < 1)
				return;
			var path = args[0];
			string defaultLanguage = null;
			if (args.Length > 1)
				defaultLanguage = args[1];	
			if (!Directory.Exists(path) && !File.Exists(path))
				return;
			
			Application.Run(new TrayForm(path, defaultLanguage));
		}
	}

    class TrayForm : Form
    {
        private SynchronizationContext _ctx;
        private string _path;
		private string _defaultLanguage;
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        public TrayForm(string path, string defaultLanguage)
        {
            _ctx = SynchronizationContext.Current;
            _path = path;
			_defaultLanguage = defaultLanguage;
            new Thread(startEngine).Start();
			setupTray();
			setupForm();
        }

		private void setupTray()
		{
			// Create a simple tray menu with only one item.
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);

            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            trayIcon = new NotifyIcon();
            trayIcon.Text = "MyTrayApp";
            trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);

            // Add menu to tray icon and show it.
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = false;
		}

		private void setupForm()
		{
			Height = 0;
			Width = 0;
			ShowInTaskbar = false;
			Visible = false;
			MinimizeBox = false;
			MaximizeBox = false;
			ControlBox = false;
			Opacity = 0;
		}

        private void startEngine()
        {
            Logger.Assign(new FileLogger());
            var cache = new TypeCache();
			var crawlHandler = new CrawlHandler(cache);
			var pluginLocator = new PluginLocator(
				Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)),
				(msg) => {});
			initPlugins(pluginLocator, crawlHandler);
			var tracker = new PluginFileTracker();
			tracker.Start(
				_path,
				cache,
				pluginLocator);

            var endpoint = new CommandEndpoint(_path, cache, handleMessage);
            endpoint.Start(_path);
            while (endpoint.IsAlive)
                Thread.Sleep(100);
            Close();
        }

		private void initPlugins(PluginLocator locator, CrawlHandler handler)
		{
			new Thread(() =>
				{
					locator.Locate().ToList()
						.ForEach(x => 
							{
								try
								{
									foreach (var line in x.Crawl(new string[] { _path }))
										handler.Handle(line);
								} catch (Exception ex) {
									Logger.Write(ex.ToString());
								}
							});
				}).Start();
		}

        private void handleMessage(string message, ITypeCache cache, Editor editor)
        {
            if (message == "gototype")
                goToType(cache, editor);
            if (message == "explore")
                explore(cache, editor);
        }

        private TypeSearchForm _gotoType = null;
        private void goToType(ITypeCache cache, Editor editor)
        {
            _ctx.Post((s) =>
                {
                    _gotoType = new TypeSearchForm(
						cache,
						(file, line, column) => { editor.GoTo(file, line, column); },
						() => { new System.Threading.Thread(() => { System.Threading.Thread.Sleep(1000); editor.SetFocus(); }).Start(); });
                    _gotoType.Show();
                    _gotoType.BringToFront();
                }, null);
        }

        private FileExplorer _exploreForm = null;
        private void explore(ITypeCache cache, Editor editor)
        {
            _ctx.Post((s) =>
                {
                    _exploreForm = new FileExplorer(
						cache,
						_defaultLanguage,
						(file, line, column) => { editor.GoTo(file, line, column); },
						() => { editor.SetFocus(); });
                    _exploreForm.Show();
                    _exploreForm.BringToFront();
                }, null);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource.
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }
    }
}

