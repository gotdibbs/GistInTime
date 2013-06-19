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
        internal bool IsAuthenticated { get; set; }

        public SettingsDialog()
        {
            InitializeComponent();

            IsAuthenticated = false;

            Save.Click += Save_Click;

            username.Focus();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            var user = username.Text;
            var pass = password.Password;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Both a username and a password are required to authenticate. Neither will be stored locally.");
                return;
            }

            try
            {
                var response = await App.Api.Authenticate(user, pass);

                if (response != null && !string.IsNullOrEmpty(response.token))
                {
                    IsAuthenticated = true;
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
    }
}
