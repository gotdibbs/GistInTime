using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GistInTime
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : MetroWindow
    {
        private bool _isRevoke = false;

        private GistInTime.Properties.Settings _settings = GistInTime.Properties.Settings.Default;

        internal bool IsAuthenticated { get; set; }

        internal Model.AuthorizationResponse AuthResponse { get; set; }

        public SettingsDialog(bool isRevoke = false)
        {
            InitializeComponent();

            _isRevoke = isRevoke;

            if (isRevoke)
            {
                Save.Content = "Revoke Authorization";
            }

            IsAuthenticated = false;

            Save.Click += Save_Click;

            username.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var user = username.Text;
            var pass = password.Password;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Both a username and a password are required to authenticate. Neither will be stored locally.");
                return;
            }

            if (!_isRevoke)
            {
                GetAuthorizationToken(user, pass);
            }
            else
            {
                RevokeAuthorizationToken(user, pass);
            }
        }

        private async void GetAuthorizationToken(string user, string pass)
        {
            try 
            {
                var response = await App.Api.Authenticate(user, pass);

                if (response != null && !string.IsNullOrEmpty(response.token))
                {
                    IsAuthenticated = true;
                    AuthResponse = response;
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Authentication failed. Please check your username and password and try again.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Authentication failure. Please check your username and password and try again.");
            }
        }

        private async void RevokeAuthorizationToken(string user, string pass)
        {
            try 
            {
                var isSuccess = await App.Api.RevokeToken(user, pass, _settings.AuthTokenId);

                if (!isSuccess)
                {
                    MessageBox.Show("Revoke failed. Please confirm your authorization token has been removed manually via the GitHub website.");
                }

                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Authentication failure. Please check your username and password and try again.");
            }
        }
    }
}
