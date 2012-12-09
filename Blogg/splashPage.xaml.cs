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
using System.Text;
using System.IO;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Phone.Shell;
using Blogg.Translations;

namespace Blogg
{
    public partial class splashPage : PhoneApplicationPage
    {
        public splashPage()
        {
            InitializeComponent();
            string savedToken;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>("refresh_token", out savedToken);
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = AppResources.SignIn;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = AppResources.CreateNew;
            if (savedToken != null)
                oAuth.refreshToken(savedToken);
            else
                ApplicationBar.IsVisible = true;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            do
            {
                NavigationService.RemoveBackEntry();
            }
            while (NavigationService.CanGoBack != false);
        }

        private void createNewButton(object sender, EventArgs e)
        {
            App.webbrowser.Uri = new Uri("http://www.blogger.com");
            App.webbrowser.Show();
        }

        private void signInButton(object sender, EventArgs e)
        {
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/oAuthPage.xaml", UriKind.Relative));       
        }
    }
}