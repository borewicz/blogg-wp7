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
using Blogg.Translations;

namespace Blogg
{
    public static class PostUtility
    {
        public static StackPanel stackPanel;
        public static List<string> images = new List<string>();

        public static void sendPost(string blogID, string title, string content)
        {
            App.prog.Text = AppResources.Publishing;
            App.prog.IsIndeterminate = true;
            App.prog.IsVisible = true;
            var client = new RestClient();
            var request = new RestRequest("https://www.googleapis.com/blogger/v3/blogs/" + blogID + "/posts/", Method.POST);
            request.AddHeader("Authorization", "Bearer " + oAuth.access_token);

            string replaceWith = "<br>";
            content = content.Replace("\r\n", replaceWith).Replace("\n", replaceWith).Replace("\r", replaceWith);
            content += "<br>";
            foreach (string img in images)
            {
                content += ("<img align=\"center\" src=\"" + img + "\"></img><br>");
            }

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
                if (resp.StatusCode == HttpStatusCode.OK) 
                {
                    App.blog.blogCollection.Clear();
                    App.blog.GetBlogsList();
                }
                else { MessageBox.Show(resp.StatusCode.ToString()); }
                //App.prog.IsVisible = false;
                App.prog.IsIndeterminate = false;
                App.prog.IsVisible = false;
            });
        }

        public static void removePost(string blogID, string postID)
        {
            var client = new RestClient();
            var request = new RestRequest("https://www.googleapis.com/blogger/v3/blogs/" + blogID + "/posts/" + postID, Method.DELETE);
            request.AddHeader("Authorization", "Bearer " + oAuth.access_token);

            client.ExecuteAsync(request, (resp) =>
            {
                //System.Diagnostics.Debug.WriteLine(resp.Content);
                if (resp.StatusCode == HttpStatusCode.NoContent)
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
                    (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                }
                else { MessageBox.Show(resp.StatusCode.ToString()); }
            });
        }

        public static bool uploadPhoto(string blogName, Stream stream)
        {
            //isFinished.Visible = true;
            //isFinished.Enabled = false;
            //isFinished.visible = Visibility.Collapsed;
            //isFinished.Enabled = true;
            App.prog.Text = AppResources.Uploading;
            App.prog.IsIndeterminate = true;
            App.prog.IsVisible = true;
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
                                string link = res.Element(name + "entry").Element(name + "content").Attribute("src").Value;
                                System.Diagnostics.Debug.WriteLine(link);
                                images.Add(link);
                                Deployment.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    App.prog.IsIndeterminate = false;
                                    App.prog.IsVisible = false;
                                    //isFinished.Enabled = false;
                                    System.Windows.Media.Imaging.BitmapImage bmp = new System.Windows.Media.Imaging.BitmapImage();
                                    bmp.SetSource(stream);
                                    Image img = new Image();
                                    img.Source = bmp;
                                    img.Width = 150;
                                    img.Height = 100;
                                    stackPanel.Children.Add(img);
                                });
                                //return res.Element(name + "entry").Element(name + "content").Attribute("src").Value;
                            }
                        }, myRequest);
                    }, imgRequest);
                }
                else { MessageBox.Show(resp.StatusCode.ToString()); }
            });
            //isFinished.Enabled = false;
            return true;
        }       
    }
}
