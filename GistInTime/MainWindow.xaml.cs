using MahApps.Metro.Controls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GistInTime
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            // Handle events
            Activated += MainWindow_Activated;
            PreviewKeyDown += new KeyEventHandler(HandleEsc);
            SearchBox.KeyDown += SearchBox_KeyDown;
        }

        /// <summary>
        /// Hides the window and clears the searchbox
        /// </summary>
        public void HideClean()
        {
            SearchBox.Text = string.Empty;
            SearchBox.SelectedItem = null;
            this.Hide();
        }

        /// <summary>
        /// Sets focus to the search box when the window is shown
        /// </summary>
        private void MainWindow_Activated(object sender, EventArgs e)
        {
            TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
            SearchBox.MoveFocus(tRequest);
        }

        /// <summary>
        /// Ensure our app doesn't exist when the 'X' is closed, just hidden
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            HideClean();
        }

        /// <summary>
        /// Hide our window when escape is pressed
        /// </summary>
        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                HideClean();
            }
        }

        /// <summary>
        /// Load the Gist contents when Enter is pressed in the searchbox.
        /// Assumes that you want the text from the first file in the Gist.
        /// </summary>
        private async void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            var gist = SearchBox.SelectedItem as Model.GistsResponse;

            if (gist != null)
            {
                var contents = await App.Api.GetGistContents(gist.files.First().Value.raw_url);
                Clipboard.SetText(contents);
                HideClean();
            }
        }
    }
}
