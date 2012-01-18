using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using OpenIDENet.CodeEngine.Core.Caching;
using OpenIDENet.CodeEngine.Core.Endpoints;
using OpenIDENet.CodeEngine.Core.EditorEngine;
using OpenIDENet.CodeEngine.Core.UI;

namespace OpenIDENet.CodeEngine
{
	class TrayForm : Form
    {
        private SynchronizationContext _ctx;
        private CommandEndpoint _endpoint;
		private string _defaultLanguage;
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        public TrayForm(CommandEndpoint endpoint, string defaultLanguage)
        {
			_endpoint = endpoint;
            _ctx = SynchronizationContext.Current;
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
			_endpoint.AddHandler(handleMessage);
            _endpoint.Start();
            while (_endpoint.IsAlive)
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
                trayIcon.Dispose();
            base.Dispose(isDisposing);
        }
    }
}
