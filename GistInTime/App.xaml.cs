using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Application = System.Windows.Application;
using ContextMenu = System.Windows.Forms.ContextMenu;
using MenuItem = System.Windows.Forms.MenuItem;
using System.Threading.Tasks;
using System.Threading;

namespace GistInTime
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Window _searchWindow = new MainWindow();

        private GistInTime.Properties.Settings _settings = GistInTime.Properties.Settings.Default;

        private NotifyIcon _notifyIcon = null;

        #region Static Extensions to App

        public static List<Model.GistsResponse> Gists { get; set; }

        public static Data.GitHubApi Api { get; set; }

        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _searchWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            InitializeGlobalHotKey();

            InitializeNotifyIcon();

            LoadSettings();
        }

        private void InitializeGlobalHotKey()
        {
            var _hotKey = new HotKey(ModifierKeys.Control | ModifierKeys.Windows, Keys.I, _searchWindow);
            _hotKey.HotKeyPressed += (k) =>
            {
                if (!_searchWindow.IsVisible)
                {
                    _searchWindow.Show();
                }
                if (_searchWindow.WindowState == WindowState.Minimized)
                {
                    _searchWindow.WindowState = WindowState.Normal;
                }

                _searchWindow.Activate();
                _searchWindow.Topmost = true;
                _searchWindow.Topmost = false;
            };
        }

        private void InitializeNotifyIcon()
        {
            var contextMenu = new ContextMenu();
            var newCommand = new MenuItem("New Gist", new EventHandler(_notifyIcon_New));
            contextMenu.MenuItems.Add(newCommand);
            var refreshCommand = new MenuItem("Refresh Gists", new EventHandler(_notifyIcon_Refresh));
            contextMenu.MenuItems.Add(refreshCommand);
            var exitCommand = new MenuItem("Exit", new EventHandler(_notifyIcon_Exit));
            contextMenu.MenuItems.Add(exitCommand);

            _notifyIcon = new NotifyIcon();
            _notifyIcon.ContextMenu = contextMenu;
            _notifyIcon.DoubleClick += _notifyIcon_DoubleClick;
            var iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/GistInTime;component/cloud.ico")).Stream;
            _notifyIcon.Icon = new Icon(iconStream);
            _notifyIcon.Text = "GistInTime";
            _notifyIcon.Visible = true;
        }

        private void LoadSettings()
        {
            if (!_settings.IsSetup || string.IsNullOrEmpty(_settings.AuthToken))
            {
                Api = new Data.GitHubApi();

                var dialog = new SettingsDialog();
                dialog.ShowDialog();

                if (!dialog.IsAuthenticated)
                {
                    App.Current.Shutdown();
                    return;
                }
                else
                {
                    _settings.IsSetup = true;
                    _settings.AuthToken = Api.AuthToken;
                    _settings.Save();

                    LoadSettings();
                }
            }
            else
            {
                Api = new Data.GitHubApi(_settings.AuthToken);

                StartPoll();
            }
        }

        /// <summary>
        /// Kicks off background polling for meeting refresh
        /// </summary>
        public void StartPoll()
        {
            Task.Run(() => PollForGists());
        }

        private void PollForGists()
        {
            Dispatcher.Invoke(async () =>
            {
                await RefreshGists();
            });

            Thread.Sleep(new TimeSpan(0, 60, 0));
            PollForGists();
        }

        private async Task<bool> RefreshGists()
        {
            Gists = new List<Model.GistsResponse>(await Api.GetMine());
            Gists.AddRange(await Api.GetStarred());

            return true;
        }

        private void _notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            _searchWindow.Show();
        }

        private void _notifyIcon_New(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://gist.github.com/");
        }

        private async void _notifyIcon_Refresh(object sender, EventArgs e)
        {
            await RefreshGists();

            _notifyIcon.ShowBalloonTip(500, "GistInTime", "Gists Refreshed.", ToolTipIcon.Info);
        }

        private void _notifyIcon_Exit(object sender, EventArgs e)
        {
            _notifyIcon.Visible = false;
            App.Current.Shutdown(); 
        }
    }
}
