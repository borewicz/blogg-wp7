using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using RestSharp;
using Newtonsoft.Json.Linq;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Controls;
using Blogg.Translations;

namespace Blogg
{
    public static class oAuth
    {
        public const string HOST = "https://accounts.google.com/o/oauth2/auth";
        public const string TOKEN_HOST = "https://accounts.google.com/o/oauth2/token";
        public const string REDIRECT_URI = "urn:ietf:wg:oauth:2.0:oob";

        public static string consumerKey = "470788067421.apps.googleusercontent.com";
        public static string consumerSecret = "zHB7W8MJlp-VDeQ96TdR8sKM";
        //public string token { get; set; }
        //public string refresh_token = "1/y3tYGtnT2vX0Xye3KfL3RRa7tVqqqsM9gipiyCbfN2o";
        public static string refresh_token { get; set; }
        public static string access_token { get; set; }
        public static string scope = "https://www.googleapis.com/auth/blogger+https://picasaweb.google.com/data/";
        //4/nqffjgT8Cln0xxQOIg7-AAAoks1J.otqfE69jo30QOl05ti8ZT3bLlLsVcQI

        public static string GetAuthenticationLink()
        {
            string getData = HOST;
            getData += "?response_type=code";
            getData += "&client_id=" + consumerKey;
            getData += "&redirect_uri=" + REDIRECT_URI;
            getData += "&scope=" + scope;
            //byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            //return string.Format("response_type=code&client_id={0}&redirect_uri={1}", this.consumerKey, REDIRECT_URI);
            return getData;
        }

        public static void refreshToken(string refresh_token)
        {
            string postData = "&client_id=" + consumerKey;
            postData += "&client_secret=" + consumerSecret;
            postData += "&refresh_token=" + refresh_token;
            postData += "&grant_type=refresh_token";

            var client = new RestClient("https://accounts.google.com/");
            var request = new RestRequest("o/oauth2/token", Method.POST);
            request.AddParameter("client_id", consumerKey);
            request.AddParameter("client_secret", consumerSecret);
            request.AddParameter("refresh_token", refresh_token);
            request.AddParameter("grant_type", "refresh_token"); 

            client.ExecuteAsync(request, (resp) =>
            {
                if (resp.StatusCode == HttpStatusCode.OK) 
                {
                    JObject o = JObject.Parse(resp.Content);
                    if (o["error"] == null)
                    {
                        oAuth.access_token = o["access_token"].ToString();
                        //ApplicationData.Current.LocalSettings.Values["refresh_token"] = oauth2.refresh_token;
                        //System.Diagnostics.Debug.WriteLine(access_token + ", " + refresh_token);
                        System.Diagnostics.Debug.WriteLine(o["access_token"]);
                        (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                        App.blog.GetBlogsList();
                    }
                    else System.Diagnostics.Debug.WriteLine("Error : " + o["error"]);
                }
                else
                {
                    MessageBox.Show(AppResources.ErrorOccured + resp.StatusCode.ToString() + AppResources.Report, AppResources.Error, MessageBoxButton.OK);
                }
                //App.prog.IsVisible = false;
            });
        }

        public static void accessToken(string code)
        {
            var client = new RestClient("https://accounts.google.com/");
            var request = new RestRequest("o/oauth2/token", Method.POST);
            request.AddParameter("code", code);
            request.AddParameter("client_id", consumerKey);
            request.AddParameter("client_secret", consumerSecret);
            request.AddParameter("redirect_uri", REDIRECT_URI);
            request.AddParameter("grant_type", "authorization_code");

            client.ExecuteAsync(request, (resp) =>
            {
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    //(Application.Current.RootVisual as PhoneApplicationFrame).GoBack(); ;
                    System.Diagnostics.Debug.WriteLine(resp.Content);

                    JObject o = JObject.Parse(resp.Content);
                    if (o["error"] == null)
                    {
                        oAuth.access_token = o["access_token"].ToString();
                        oAuth.refresh_token = o["refresh_token"].ToString();
                        //ApplicationData.Current.LocalSettings.Values["refresh_token"] = oauth2.refresh_token;
                        //System.Diagnostics.Debug.WriteLine(access_token + ", " + refresh_token);
                        System.Diagnostics.Debug.WriteLine(o["access_token"]);
                        System.Diagnostics.Debug.WriteLine(o["refresh_token"]);
                        updateIsolatedStorage(oAuth.refresh_token);
                        (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/MainPage.xaml", UriKind.Relative)); 
                        App.blog.GetBlogsList();
                    }
                    else System.Diagnostics.Debug.WriteLine("Error : " + o["error"]);
                    //return (resp.Content);
                }
                else { MessageBox.Show(resp.StatusCode.ToString()); }
                //App.prog.IsVisible = false;
            });
        }

        public static void updateIsolatedStorage(string refresh_token)
        {
            string savedToken;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>("refresh_token", out savedToken);
            if (savedToken == null)
                IsolatedStorageSettings.ApplicationSettings.Add("refresh_token", refresh_token);
            else
                IsolatedStorageSettings.ApplicationSettings["refresh_token"] = refresh_token;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

    }
}
