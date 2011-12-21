using System;
using System.Windows.Forms;
using OpenIDENet.CodeEngine.Core.Crawlers;
using OpenIDENet.CodeEngine.Core.Caching;
using OpenIDENet.CodeEngine.Core.Endpoints;
using System.IO;
using System.Threading;
using OpenIDENet.CodeEngine.Core.ChangeTrackers;
using OpenIDENet.CodeEngine.Core.Logging;
using OpenIDENet.CodeEngine.Core.UI;
using OpenIDENet.CodeEngine.Core.EditorEngine;
using System.Drawing;

namespace OpenIDENet.CodeEngine
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			if (args.Length != 1)
				return;
			var path = args[0];
			if (!Directory.Exists(path) && !File.Exists(path))
				return;
			
			Application.Run(new TrayForm(path));
		}
	}

    class TrayForm : Form
    {
        private SynchronizationContext _ctx;
        private string _path;
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        public TrayForm(string path)
        {
            _ctx = SynchronizationContext.Current;

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
            _path = path;
            new Thread(startEngine).Start();
        }

        private void startEngine()
        {
            Logger.Assign(new FileLogger());
            var options = new CrawlOptions(_path);
            var cache = new TypeCache();
            var wather = new CSharpFileTracker();
            wather.Start(options.Directory, cache);
            new CSharpCrawler(cache).InitialCrawl(options);

            var endpoint = new CommandEndpoint(options.Directory, cache, handleMessage);
            endpoint.Start(_path);
            while (endpoint.IsAlive)
                Thread.Sleep(100);
            Close();
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
                    _gotoType = new TypeSearchForm(cache, (file, line, column) => { editor.GoTo(file, line, column); }, () => { editor.SetFocus(); });
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
						(file, line, column) => { editor.GoTo(file, line, column); },
						() => { editor.SetFocus(); });
                    _exploreForm.Show();
                    _exploreForm.BringToFront();
                }, null);
        }

        protected override void OnLoad(EventArgs e)
        {
			WindowState = FormWindowState.Minimized;
			Left = 15000;
			Top = 15000;
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.

            base.OnLoad(e);
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

