using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using OpenIDE.CodeEngine.Core.Caching;
using OpenIDE.CodeEngine.Core.Endpoints;
using OpenIDE.CodeEngine.Core.Endpoints.Tcp;
using OpenIDE.CodeEngine.Core.EditorEngine;
using OpenIDE.CodeEngine.Core.Handlers;
using OpenIDE.CodeEngine.Core.UI;
using OpenIDE.Core.Config;
using OpenIDE.Core.CommandBuilding;
using OpenIDE.Core.Windowing;
using OpenIDE.Core.Logging;

namespace OpenIDE.CodeEngine
{
	class TrayForm : Form
    {
        private SynchronizationContext _ctx;
        private CommandEndpoint _endpoint;
		private string _defaultLanguage;
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;
		private ICacheBuilder _cacheBuilder;
		private bool _terminateApplication = false;

        public TrayForm(CommandEndpoint endpoint, string defaultLanguage, ICacheBuilder builder)
        {
			_endpoint = endpoint;
			_cacheBuilder = builder;
            _ctx = SynchronizationContext.Current;
			_defaultLanguage = defaultLanguage;
            setupForm();
            new Thread(startEngine).Start();
			setupTray();
        }

		// Hide from alt+tab list
		protected override CreateParams CreateParams {
            get {
                // Turn on WS_EX_TOOLWINDOW style bit
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80; 
                return cp;
            }
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
            WindowState = FormWindowState.Minimized;
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
            var editorHasExisted = false;
			_endpoint.AddHandler(handleMessage);
            _endpoint.Start();
            while (!_terminateApplication) {
                var isAlive = _endpoint.IsAlive;
                if (isAlive)
                    editorHasExisted = true;
                if (editorHasExisted && !isAlive)
                    break;
                Thread.Sleep(100);
            }
			_ctx.Post((s) => {
	            Close();
			}, null);
        }

        private void handleMessage(MessageArgs message, ITypeCache cache, Editor editor)
        {
            Logger.Write("Message trayform args: " + message.Message);
            if (message.Message == "gototype")
                goToType(cache, editor);
            if (message.Message == "explore")
                explore(cache, editor);
			if (message.Message.StartsWith("snippet-complete "))
				snippetComplete(message, editor);
			if (message.Message.StartsWith("snippet-create "))
				snippetCreate(message, editor);
			if (message.Message.StartsWith("member-lookup "))
				memberLookup(message, editor);
            if (message.Message.StartsWith("user-select \"unsupported\" "))
                userSelect(message, editor);
            if (message.Message.StartsWith("user-input \"unsupported\" "))
                userInput(message, editor);
			if (message.Message == "shutdown")
				_terminateApplication = true;
        }

        private TypeSearchForm _gotoType = null;
        private void goToType(ITypeCache cache, Editor editor)
        {
            Logger.Write("Preparing to open type search");
            _ctx.Post((s) =>
                {
                    Logger.Write("Opening type search");
                    try {
    					if (_gotoType == null || !_gotoType.Visible)
    					{
                            Logger.Write("Creating typesearch form");
    						_gotoType = new TypeSearchForm(
    							cache,
    							(file, line, column) => { editor.GoTo(file, line, column); },
    							() => { new System.Threading.Thread(() => { System.Threading.Thread.Sleep(1000); editor.SetFocus(); }).Start(); });
    						_gotoType.Show(this);
    					}
    					setToForeground(_gotoType);
                    } catch (Exception ex) {
                        Logger.Write(ex);
                    }
                }, null);
        }

        private FileExplorer _exploreForm = null;
        private void explore(ITypeCache cache, Editor editor)
        {
            _ctx.Post((s) =>
                {
					if (_exploreForm == null || !_exploreForm.Visible)
					{
						_exploreForm = new FileExplorer(
							cache,
							_defaultLanguage,
							(file, line, column) => { editor.GoTo(file, line, column); },
							() => { editor.SetFocus(); });
						_exploreForm.Show(this);
					}
                    setToForeground(_exploreForm);
                }, null);
        }

		private void snippetComplete(MessageArgs message, Editor editor)
		{
			_ctx.Post((s) =>
				{
					var command = "snippet-complete ";
					var msg = message.Message.Substring(command.Length, message.Message.Length - command.Length);
					new CompleteSnippetHandler(_cacheBuilder, _endpoint.Token, _endpoint)
						.Handle(
							new CommandStringParser()
								.Parse(msg).ToArray());
				}, null);
		}

		private void snippetCreate(MessageArgs message, Editor editor)
		{
			var command = "snippet-create ";
					var msg = message.Message
						.Substring(command.Length, message.Message.Length - command.Length);
					new CreateSnippetHandler(editor, _cacheBuilder, _endpoint.Token)
						.Handle(
							new CommandStringParser()
								.Parse(msg).ToArray());
		}

		private MemberLookupForm _memberLookup = null;
		private void memberLookup(MessageArgs message, Editor editor)
		{
			_ctx.Post((s) =>
				{
					var members = new string[]
						{
							"Text|Type: System.Int",
							"Visible|Type: System.bool",
							"BringToFront|Return Type: void",
							"Parse(string[], bool)|Return Type: string[[newline]]\tstring[]: Line Array[[newline]]\tbool: Remove Empty entries"
						};
					_memberLookup = new MemberLookupForm(members);
                    _memberLookup.Show(this);
                    setToForeground(_memberLookup);
				}, null);
		}

        private void userSelect(MessageArgs message, Editor editor)
        {
            var state = new ConfigReader(_endpoint.Token).Get("oi.userselect.ui.fallback");
            if (state == "disabled")
                return;
            _ctx.Post((s) =>
                {
                    try {
                        var args = new CommandStringParser().Parse(message.Message).ToArray();
                        var items = new List<string>();
                        var keys = new List<string>();
                        foreach (var item in args[3].Split(new[] {','})) {
                            var chunks = item.Split(new[] {"||"}, StringSplitOptions.None);
                            if (chunks.Length > 1) {
                                keys.Add(chunks[0]);
                                items.Add(chunks[1]);
                            } else {
                                keys.Add(item);
                                items.Add(item);
                            }
                        }
                        var form = new UserSelectForm(items, keys, (item) => {
                            if (item != null)
                                _endpoint.PublishEvent("user-selected '" + args[2] + "' '"  + item + "'");
                            else
                                _endpoint.PublishEvent("user-selected '" + args[2] + "' 'user-cancelled'");
                            editor.SetFocus();
                        });
                        form.Show(this);
                        setToForeground(form);
                    } catch {
                    }
                }, null);
        }

        private void userInput(MessageArgs message, Editor editor)
        {
            Logger.Write("Getting state for userinput fallback");
            var state = new ConfigReader(_endpoint.Token).Get("oi.userinput.ui.fallback");
            Logger.Write("State is "+state);
            if (state == "disabled")
                return;
            _ctx.Post((s) =>
                {
                    Logger.Write("Launching user input form");
                    try {
                        var args = new CommandStringParser().Parse(message.Message).ToArray();
                        var defaultValue = "";
                        if (args.Length > 3)
                            defaultValue = args[3];
                        var form = new UserInputForm(defaultValue, (item) => {
                            if (item != null)
                                _endpoint.PublishEvent("user-inputted '" + args[2] + "' '"  + item + "'");
                            else
                                _endpoint.PublishEvent("user-inputted '" + args[2] + "' 'user-cancelled'");
                            editor.SetFocus();
                        });
                        form.Show(this);
                        setToForeground(form);
                    } catch (Exception ex) {
                        Logger.Write(ex);
                    }
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

		private void setToForeground(Form form)
		{
            Logger.Write("Form a handle is " + form.Handle.ToString());

            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX) {
                ThreadPool.QueueUserWorkItem((u) => {
                    Thread.Sleep(50);
                    BringToForeGround.ByProcAndName(System.Diagnostics.Process.GetCurrentProcess().ProcessName, form.Text);
                }, null);
            } else {
                BringToForeGround.ByHWnd(form.Handle);
            }
		}
    }
}
