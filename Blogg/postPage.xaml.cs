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
using Microsoft.Phone.Shell;
using Blogg.Translations;

namespace Blogg
{
    public partial class postPage : PhoneApplicationPage
    {
        string blogID, postID, url;
        public postPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = AppResources.Delete;

            if (this.NavigationContext.QueryString.ContainsKey("blogid") && this.NavigationContext.QueryString.ContainsKey("postid"))
            {
                System.Diagnostics.Debug.WriteLine(this.NavigationContext.QueryString["blogid"]);
                System.Diagnostics.Debug.WriteLine(this.NavigationContext.QueryString["postid"]);
                var item = (from gr in App.blog.blogCollection
                              from i in gr.Items
                              where i.id == this.NavigationContext.QueryString["postid"]
                              select i).First();

                blogID = item.blogID;
                postID = item.id;
                url = item.url;
                webBrowser.Navigate(new Uri(url + "?m=1"));
            }
        }

        private void deleteClick(object sender, System.EventArgs e)
        {
            MessageBoxResult m = MessageBox.Show(AppResources.WarningDelete, AppResources.Warning, MessageBoxButton.OKCancel);
            if (m == MessageBoxResult.OK)
            {
                PostUtility.removePost(blogID, postID);
            }
        }
    }
}