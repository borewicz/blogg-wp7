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
using System.IO.IsolatedStorage;

namespace Blogg
{
    public partial class blogList : PhoneApplicationPage
    {
        public blogList()
        {
            InitializeComponent();
            blogListBox.ItemsSource = App.blog.blogCollection;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            App.blog.blogCollection.Clear();
            App.blog.GetBlogsList();
        }

        private void StackPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string blogID;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>("blogID", out blogID);
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (blogID == null)
                {
                    IsolatedStorageSettings.ApplicationSettings.Add("blogID", App.blog.blogCollection[blogListBox.SelectedIndex].id);
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings["blogID"] = App.blog.blogCollection[blogListBox.SelectedIndex].id;
                }
                IsolatedStorageSettings.ApplicationSettings.Save();
            });
            //App.blog.blogID = App.blog.blogCollection[blogListBox.SelectedIndex].id;
            App.blog.doLogin();
        }
    }
}