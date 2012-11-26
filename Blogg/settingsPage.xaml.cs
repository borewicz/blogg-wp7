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

namespace Blogg
{
    public partial class settingsPage : PhoneApplicationPage
    {
        public settingsPage()
        {
            InitializeComponent();
            string email, password;
            var settings = IsolatedStorageSettings.ApplicationSettings;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>("email", out email);
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>("password", out password);
            if (email != null || password != null)
            {
                usernameTextBox.Text = email;
                passwordTextBox.Password = password;
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            do
            { NavigationService.RemoveBackEntry(); }
            while (NavigationService.CanGoBack != false);
        }

        private void loginButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string email, password;

            IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>("email", out email);
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>("password", out password);
            if (email == null || password == null)
            {
                IsolatedStorageSettings.ApplicationSettings.Add("email", usernameTextBox.Text);
                IsolatedStorageSettings.ApplicationSettings.Add("password", passwordTextBox.Password);
            }
            else
            {
                IsolatedStorageSettings.ApplicationSettings["email"] = usernameTextBox.Text;
                IsolatedStorageSettings.ApplicationSettings["password"] = passwordTextBox.Password;
            }
            IsolatedStorageSettings.ApplicationSettings.Save();
            App.blog.doLogin();
        }
    }
}