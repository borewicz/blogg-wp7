using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using System.Windows;
using Microsoft.Phone.Controls;
using RestSharp;
using System.Net;
using System.Windows.Controls;

namespace Blogg
{
    public class isFinishedNotifier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool _Visible = false;

        private Visibility _visible;

        public bool Visible
        {
            get { return this._Visible; }

            set
            {
                if (value != this._Visible)
                {
                    this._Visible = value;
                    NotifyPropertyChanged("Visible");
                }
            }
        }

        public Visibility visible
        {
            get { return this._visible; }

            set
            {
                if (this._visible != Visibility.Collapsed)
                {
                    this._visible = Visibility.Collapsed;
                    NotifyPropertyChanged("visible");
                }
                else
                {
                    this._visible = Visibility.Visible;
                    NotifyPropertyChanged("visible");
                }
            }
        }

        private bool _Enabled;

        public bool Enabled
        {
            get { return this._Enabled; }

            set
            {
                if (value != this._Enabled)
                {
                    this._Enabled = value;
                    NotifyPropertyChanged("Enabled");
                }
            }
        }

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

    }

    public static class PostUtility
    {
        public static isFinishedNotifier isFinished;
        public static StackPanel stackPanel;
        public static List<string> images;

        public static void sendPost(string blogID, string title, string content)
        {
            isFinished.Visible = true;
            isFinished.Enabled = false;
            var client = new RestClient();
            var request = new RestRequest("https://www.googleapis.com/blogger/v3/blogs/" + blogID + "/posts/", Method.POST);
            request.AddHeader("Authorization", "Bearer " + oAuth.access_token);

            JObject rss = new JObject(
                new JProperty("kind", "blogger#post"),
                new JProperty("blog", new JObject(new JProperty("id", blogID))),
                new JProperty("title", title),
                new JProperty("content", content));
            //labels

            //System.Diagnostics.Debug.WriteLine(rss.ToString());

            request.AddParameter("application/json", rss.ToString(), ParameterType.RequestBody);

            client.ExecuteAsync(request, (resp) =>
            {
                if (resp.StatusCode == HttpStatusCode.Created) 
                {
                    (Application.Current.RootVisual as PhoneApplicationFrame).GoBack(); ;
                }
                else { MessageBox.Show(resp.StatusCode.ToString()); }
                //App.prog.IsVisible = false;
            });
            isFinished.Visible = false;
            isFinished.Enabled = true;
            isFinished.visible = Visibility.Visible;
        }

        public static void removePost(string blogID, string postID)
        {
            var client = new RestClient();
            var request = new RestRequest("https://www.googleapis.com/blogger/v3/blogs/" + blogID + "/posts/" + postID, Method.DELETE);
            request.AddHeader("Authorization", "Bearer " + oAuth.access_token);

            client.ExecuteAsync(request, (resp) =>
            {
                //System.Diagnostics.Debug.WriteLine(resp.Content);
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    /*
                        * from gr in App.dataSource.blogs
                            from items in gr.Items
                            where items.id == (string)navigationParameter
                            select items).First();*/
                    for (int i = 0; i < App.blog.blogCollection.Count(); i++)
                    {
                        for (int j = 0; j < App.blog.blogCollection[i].Items.Count; j++)
                        {
                            if ((App.blog.blogCollection[i].Items[j].id == postID) &&
                                (App.blog.blogCollection[i].Items[j].blogID == blogID))
                                App.blog.blogCollection[i].Items.RemoveAt(j);
                            break;
                        }
                    }
                    (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/blogList.xaml", UriKind.Relative));
                }
            });
        }

        public static bool uploadPhoto(string blogName, Stream stream)
        {
            //isFinished.Visible = true;
            //isFinished.Enabled = false;
            //isFinished.visible = Visibility.Collapsed;
            string blogUrl = null;

            var client = new RestClient();
            var request = new RestRequest("https://picasaweb.google.com/data/feed/api/user/default", Method.GET);
            request.AddHeader("Authorization", "Bearer " + oAuth.access_token);
            request.AddHeader("GData-Version", "2");

            //StreamReader reader = new StreamReader(stream);
            //string output = reader.ReadToEnd();

            client.ExecuteAsync(request, (resp) =>
            {
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    //System.Diagnostics.Debug.WriteLine(resp.Content);
                    XElement doc = XElement.Parse(resp.Content);
                    XNamespace name = "http://www.w3.org/2005/Atom";
                    foreach (XElement element in doc.Descendants(name + "entry"))
                    {
                        if (element.Element(name + "title").Value == blogName)
                        {
                            blogUrl = App.extractBlogID(element.Element(name + "link").Attribute("href").Value, 0, '?');
                            //blogUrl = element.Element(name + "id").Value;
                            break;
                            
                        }
                    }
                    if (blogUrl == null)
                    {
                        blogUrl = "https://picasaweb.google.com/data/feed/api/user/default/albumid/default";
                    }

                    HttpWebRequest imgRequest = (HttpWebRequest)HttpWebRequest.Create(new Uri(blogUrl));
                    imgRequest.Method = "POST";
                    imgRequest.ContentType = "image/jpeg";
                    imgRequest.Headers["Authorization"] = "Bearer " + oAuth.access_token;
                    imgRequest.BeginGetRequestStream((callbackResult) =>
                    {
                        HttpWebRequest myRequest = (HttpWebRequest)callbackResult.AsyncState;
                        // End the stream request operation
                        Stream postStream = myRequest.EndGetRequestStream(callbackResult);
                        byte[] file = App.ReadFully(stream);
                        postStream.Write(file, 0, file.Length);
                        postStream.Close();

                        // Start the web request
                        myRequest.BeginGetResponse((requestCallbackResult) =>
                        {
                            HttpWebRequest newRequest = (HttpWebRequest)requestCallbackResult.AsyncState;
                            HttpWebResponse response = (HttpWebResponse)newRequest.EndGetResponse(requestCallbackResult);
                            using (StreamReader httpWebStreamReader = new StreamReader(response.GetResponseStream()))
                            {
                                string result = httpWebStreamReader.ReadToEnd();
                                XDocument res = XDocument.Parse(result);
                                System.Diagnostics.Debug.WriteLine(res.Element(name + "entry").Element(name + "content").Attribute("src").Value);
                                Deployment.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    System.Windows.Media.Imaging.BitmapImage bmp = new System.Windows.Media.Imaging.BitmapImage();
                                    bmp.SetSource(stream);
                                    Image img = new Image();
                                    img.Source = bmp;
                                    img.Width = 150;
                                    img.Height = 100;
                                    stackPanel.Children.Add(img);
                                    images.Add(res.Element(name + "entry").Element(name + "content").Attribute("src").Value);
                                });
                                //return res.Element(name + "entry").Element(name + "content").Attribute("src").Value;
                            }
                        }, myRequest);
                    }, imgRequest);
                }
            });
            return true;
        }       
    }
}
