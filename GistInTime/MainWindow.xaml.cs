using MahApps.Metro.Controls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GistInTime
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private TextBox _searchInputField = null;

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
            // Clear the selected item
            SearchBox.SelectedItem = null;

            // Clear the parent control's text property
            SearchBox.Text = string.Empty;

            /* Sadly clearing the parent control's text property does not clear
                the child textbox all the time, so clear it manually. */
            if (_searchInputField == null)
            {
                _searchInputField = FindChild<TextBox>(SearchBox, "PART_Editor");
            }
            _searchInputField.Text = string.Empty;

            // Hide the search window.
            Hide();
        }

        /// <summary>
        /// Sets focus to the search box when the window is shown
        /// </summary>
        private void MainWindow_Activated(object sender, EventArgs e)
        {
            // Put focus on first element
            MoveFocus(new TraversalRequest(FocusNavigationDirection.First));

            // Move focus to searchbox's first focusable element (the text input)
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

            if (gist != null && !string.IsNullOrEmpty(gist.id))
            {
                var contents = await App.Api.GetGistContents(gist.files.First().Value.raw_url);
                Clipboard.SetText(contents);
                HideClean();
            }
        }

        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        public T FindChild<T>(DependencyObject parent, string childName)
           where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }
    }
}
