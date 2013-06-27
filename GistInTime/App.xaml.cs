using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using GistInTime.Helpers;
using Application = System.Windows.Application;
using ContextMenu = System.Windows.Forms.ContextMenu;
using MenuItem = System.Windows.Forms.MenuItem;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButtons = System.Windows.MessageBoxButton;
using System.Deployment.Application;

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
            contextMenu.AppendCommand("New Gist", new EventHandler(_notifyIcon_New));
            contextMenu.AppendCommand("Refresh Gists", new EventHandler(_notifyIcon_Refresh));
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                contextMenu.AppendCommand("Check for Updates", new EventHandler(_notifyIcon_CheckUpdates));
            }
            contextMenu.AppendSeparator();
            contextMenu.AppendCommand("Logout", new EventHandler(_notifyIcon_Logout));
            contextMenu.AppendSeparator();
            contextMenu.AppendCommand("Exit", new EventHandler(_notifyIcon_Exit));

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
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                dialog.ShowDialog();

                if (!dialog.IsAuthenticated)
                {
                    App.Current.Shutdown();
                    return;
                }
                else
                {
                    _settings.IsSetup = true;
                    _settings.AuthToken = dialog.AuthResponse.token;
                    _settings.AuthTokenId = dialog.AuthResponse.id.ToString();
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

            if (!(Gists != null && Gists.Count > 0))
            {
                _notifyIcon.ShowBalloonTip(500, "GistInTime", "You have no Gists, right click the tray icon to create a new gist.", ToolTipIcon.Warning);
            }

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

        private void _notifyIcon_CheckUpdates(object sender, EventArgs e)
        {
            var ad = ApplicationDeployment.CurrentDeployment;
            UpdateCheckInfo info = null;

            try
            {
                info = ad.CheckForDetailedUpdate();
            }
            catch (DeploymentDownloadException dde)
            {
                MessageBox.Show(string.Format("This version is {0}.{1}.{2}.{3} ", 
                    ad.UpdatedVersion.Major, 
                    ad.UpdatedVersion.Minor, 
                    ad.UpdatedVersion.Build, 
                    ad.UpdatedVersion.Revision) + Environment.NewLine +
                    "The new version of the application cannot be downloaded at this time. \n\nPlease check your network connection, or try again later. Error: " + dde.Message, "Error!");

                return;
            }
            catch (InvalidDeploymentException ide)
            {
                MessageBox.Show("Cannot check for a new version of the application. The ClickOnce deployment is corrupt. Please redeploy the application and try again. Error: " + ide.Message, "Error!");
                return;
            }
            catch (InvalidOperationException ioe)
            {
                MessageBox.Show("This application cannot be updated. It is likely not a ClickOnce application. Error: " + ioe.Message, "Error!");
                return;
            }

            if (!info.UpdateAvailable)
            {
                MessageBox.Show("You're all up to date!");
            }
            else
            {
                bool doUpdate = true;

                if (!info.IsUpdateRequired)
                {
                    var dr = MessageBox.Show(
                        String.Format("This version is {0}.{1}.{2}.{3} ", ad.UpdatedVersion.Major, ad.UpdatedVersion.Minor, ad.UpdatedVersion.Build, ad.UpdatedVersion.Revision) + Environment.NewLine +
                        String.Format("Update Version is {0}.{1}.{2}.{3}", info.AvailableVersion.Major, info.AvailableVersion.Minor, info.AvailableVersion.Build, info.AvailableVersion.Revision) + Environment.NewLine +
                        "Would you like to update the application now?", "Update Available", MessageBoxButtons.OKCancel);
                    if (dr != MessageBoxResult.OK)
                    {
                        doUpdate = false;

                        return;
                    }
                }
                else
                {
                    // Display a message that the app MUST reboot. Display the minimum required version.
                    MessageBox.Show("This application has detected an update from your current " +
                        "version to version " + info.MinimumRequiredVersion.ToString() +
                        ". The application will now install the update and restart.",
                        "Update Available", MessageBoxButtons.OK);
                    doUpdate = true;
                }

                if (doUpdate)
                {
                    try
                    {
                        ad.Update();
                        MessageBox.Show("The application has been upgraded and will restart now!");
                        App.Current.Shutdown();
                        return;
                    }
                    catch (DeploymentDownloadException dde)
                    {
                        MessageBox.Show("Cannot install the latest version of the application. \n\nPlease check your network connection, or try again later. Error: " + dde);
                        return;
                    }
                }
            }
        }

        private void _notifyIcon_Logout(object sender, EventArgs e)
        {
            var dialog = new SettingsDialog(true);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialog.ShowDialog();

            _settings.IsSetup = false;
            LoadSettings();
        }

        private void _notifyIcon_Exit(object sender, EventArgs e)
        {
            _notifyIcon.Visible = false;
            App.Current.Shutdown(); 
        }
    }
}
