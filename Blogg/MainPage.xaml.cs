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
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Phone.Shell;
using System.Windows.Data;
using System.Globalization;
using System.ComponentModel;

namespace Blogg
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
            //postList.ItemsSource = App.blog.postCollection;
            this.DataContext = App.blog.blogCollection;
            this.postList.ItemsSource = App.blog.blogCollection;
            //draftList.ItemsSource = App.blog.draftCollection;
            //App.prog.IsVisible = true;
            //App.prog.IsIndeterminate = true;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            do
            {
                NavigationService.RemoveBackEntry();
            }
            while (NavigationService.CanGoBack != false);
        }

        private void showPostTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //ListBoxItem selectedListBoxItem = .ItemContainerGenerator.ContainerFromItem((sender as MenuItem).DataContext) as ListBoxItem;
            PostItem item = (sender as ListBox).SelectedItem as PostItem;
            NavigationService.Navigate(new Uri("/postPage.xaml?blogid=" + item.blogID + "&postid=" + item.id, UriKind.Relative));
            //ListBoxItem selectedListBoxItem = this.postList.ItemContainerGenerator.ContainerFromItem((sender as MenuItem).DataContext) as ListBoxItem;
            //App.webbrowser.Uri = new Uri(App.blog.postCollection[postList.SelectedIndex].url);
            App.webbrowser.Show();
            //App.blog.deletePost("6247561895747118623", (selectedListBoxItem.Content as postListClass).id);
        }

        private void addPostClick(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri("/addPost.xaml", UriKind.Relative));
        }

        private void signOutClick(object sender, System.EventArgs e)
        {
            IsolatedStorageSettings.ApplicationSettings.Remove("access_token");
            IsolatedStorageSettings.ApplicationSettings.Remove("refresh_token");
            App.blog.blogCollection.Clear();
            NavigationService.Navigate(new Uri("/splashPage.xaml", UriKind.Relative));
        }

        private void bloggerSiteClick(object sender, System.EventArgs e)
        {
        	App.webbrowser.Uri = new Uri("http://www.blogger.com/");
            App.webbrowser.Show();
        }
    }
}