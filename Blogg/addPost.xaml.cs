using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System.IO;
using System.Windows.Threading;
using Microsoft.Phone.Shell;
using Blogg.Translations;

namespace Blogg
{
    public partial class Page1 : PhoneApplicationPage
    {
        private CameraCaptureTask ctask = new CameraCaptureTask();
        private PhotoChooserTask ptask = new PhotoChooserTask();

        public Page1()
        {
            InitializeComponent();
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = AppResources.Publish;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = AppResources.Camera;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).Text = AppResources.Picture;

            PostUtility.stackPanel = new StackPanel();
            PostUtility.stackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
            PostUtility.stackPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            // scrollViewer.Children.Add(PostUtility.stackPanel);
            scrollViewer.Content = PostUtility.stackPanel;
            SystemTray.SetProgressIndicator(this, App.prog); 
            if (App.blog.blogCollection.Count > 1)
            {
                foreach (var blog in App.blog.blogCollection)
                {
                    listPicker.Items.Add(blog.name);
                }
                //ctask = new CameraCaptureTask();
            }
            else listPicker.Visibility = System.Windows.Visibility.Collapsed;
            ctask.Completed += new EventHandler<PhotoResult>(ctask_Completed);
            ptask.Completed += new EventHandler<PhotoResult>(ctask_Completed);
        }

        private void ctask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                PostUtility.uploadPhoto((listPicker.SelectedItem as string), e.ChosenPhoto);
                //Code to display the photo on the page in an image control named myImage.
                //myImage.Source = bmp;
            }
        }

        private void cameraClick(object sender, System.EventArgs e)
        {
            ctask.Show();
        }

        private void photoClick(object sender, System.EventArgs e)
        {
            ptask.Show();
        }

        private void publishButtonClick(object sender, System.EventArgs e)
        {
            if (titleTextBox.Text != "")
            {
                string blogID = (from blog in App.blog.blogCollection
                                 where blog.name == (listPicker.SelectedItem as string)
                                 select blog.id).First();

                PostUtility.sendPost(blogID, titleTextBox.Text, contentTextBox.Text);
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
            else MessageBox.Show(AppResources.TitleRequired, AppResources.Error, MessageBoxButton.OK);
            //if (contentTextBox.Text != "")
            //    App.blog.sendPost(App.blog.blogID, titleTextBox.Text, contentTextBox.Text, categoriesTextBox.Text, 0);
            //else MessageBox.Show("You didn't't fill required data!", "Error", MessageBoxButton.OK);
            //System.Diagnostics.Debug.WriteLine(contentTextBox.GetHtml(C1.Phone.RichTextBox.Documents.HtmlEncoding.Inline));
        }

        private void contentTextBox_TextInputStart(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
        	this.contentScroll.UpdateLayout();
 			this.contentScroll.ScrollToVerticalOffset(this.contentTextBox.ActualHeight);
        }
    }
}