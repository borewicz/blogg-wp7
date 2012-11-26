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
using HtmlAgilityPack;

namespace Blogg
{
    public partial class oAuthPage : PhoneApplicationPage
    {
        public oAuthPage()
        {
            InitializeComponent();
            webBrowser.Navigate(new Uri(oAuth.GetAuthenticationLink()));
            //MessageBox.Show(GetAuthenticationLink());
        }

        private void webBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri.ToString().Contains("https://accounts.google.com/o/oauth2/approval?as="))
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(webBrowser.SaveToString());

                //HtmlNode code = doc.DocumentNode.SelectSingleNode("//input[@id=\"code\"]");
                HtmlNode code = doc.DocumentNode.SelectSingleNode("//title");
                //MessageBox.Show(doc.DocumentNode.SelectNodes("//title").Count.ToString());
                System.Diagnostics.Debug.WriteLine(code.InnerText);
                //oAuth.refreshToken(refresh_token);
                webBrowser.Navigate(new Uri("https://accounts.google.com/logout"));
                oAuth.accessToken(App.extractBlogID(code.InnerText, 1, '='));

            }
        }
    }
}