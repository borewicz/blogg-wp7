using System.ComponentModel;
using System;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Blogg.Translations;
using RestSharp;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows;
using Microsoft.Phone.Controls;

namespace Blogg
{
    public class PostItem : INotifyPropertyChanged
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string profile { get; set; }
        public string blogID { get; set; }
        public DateTime time { get; set; }
        public string author { get; set; }
        private BitmapImage _authorAvatar { get; set; }

        public BitmapImage authorAvatar
        {
            get
            {
                return this._authorAvatar;
            }

            set
            {
                if (value != this._authorAvatar)
                {
                    this._authorAvatar = value;
                    OnPropertyChanged("authorAvatar");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    public class BlogItem : INotifyPropertyChanged
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string summary { get; set; }
        public string posts { get; set; }
        public DateTime published { get; set; }
        public DateTime updated { get; set; }

        private List<PostItem> _Items = new List<PostItem>();
        public List<PostItem> Items
        {
            get
            {
                return this._Items;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    public class blogger
    {
        public ObservableCollection<BlogItem> blogCollection = new ObservableCollection<BlogItem>();

        public void GetBlogsList()
        {
            App.prog.Text = AppResources.Updating;
            App.prog.IsIndeterminate = true;
            App.prog.IsVisible = true;
            var client = new RestClient();
            var request = new RestRequest("https://www.googleapis.com/blogger/v3/users/self/blogs", Method.GET);
            request.AddHeader("Authorization", "Bearer " + oAuth.access_token);
            //System.Diagnostics.Debug.WriteLine("Bearer " + oAuth.access_token);

            client.ExecuteAsync(request, (resp) =>
            {
                //System.Diagnostics.Debug.WriteLine(resp.Content);
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    Dictionary<string, BitmapImage> dict = new Dictionary<string,BitmapImage>();

                    JObject root = JObject.Parse(resp.Content);
                    if (root["items"] != null)
                    {
                        JArray items = (JArray)root["items"];

                        //JObject item;
                        //JToken jtoken;
                        foreach (JObject i in items)
                        {
                            BlogItem blogItem = new BlogItem();

                            blogItem.id = i["id"].ToString();
                            blogItem.name = i["name"].ToString();
                            blogItem.summary = i["description"].ToString();
                            //blogItem.url = i["url"].ToString();
                            //blogItem.posts = i["posts"]["totalItems"].ToString();
                            //blogItem.published = Convert.ToDateTime(i["published"].ToString());     //i["published"].ToString();
                            //blogItem.updated = Convert.ToDateTime(i["updated"].ToString());


                            //System.Diagnostics.Debug.WriteLine("https://www.googleapis.com/blogger/v3/blogs/" + blogItem.id + "/posts");
                            //HttpResponseMessage postResponse = await client.GetAsync("https://www.googleapis.com/blogger/v3/blogs/" + blogItem.id + "/posts?fetchBodies=false&maxResults=20");
                            var postRequest = new RestRequest("https://www.googleapis.com/blogger/v3/blogs/" + blogItem.id + "/posts/", Method.GET);
                            postRequest.AddHeader("Authorization", "Bearer " + oAuth.access_token);

                            client.ExecuteAsync(postRequest, (postResponse) =>
                            {
                                //System.Diagnostics.Debug.WriteLine(postResponse.Content);
                                if (postResponse.StatusCode == HttpStatusCode.OK)
                                {
                                    //System.Diagnostics.Debug.WriteLine(postContent);
                                    JObject postRoot = JObject.Parse(postResponse.Content);
                                    if (postRoot["items"] != null)
                                    {
                                        JArray posts = (JArray)postRoot["items"];

                                        foreach (JObject p in posts)
                                        {
                                            PostItem postItem = new PostItem();
                                            postItem.blogID = blogItem.id;
                                            postItem.id = p["id"].ToString();
                                            postItem.name = p["title"].ToString();
                                            postItem.time = Convert.ToDateTime(p["published"].ToString());
                                            postItem.author = p["author"]["displayName"].ToString();
                                            postItem.url = p["url"].ToString();
                                            postItem.authorAvatar = new BitmapImage();
                                            //author.image.url
                                            string profile = p["author"]["image"]["url"].ToString();
                                            if (profile != "http://img2.blogblog.com/img/b16-rounded.gif")
                                            {
                                                if (dict.ContainsKey(profile))
                                                    postItem.authorAvatar = dict[profile];
                                                else
                                                {
                                                    var photoRequest = new RestRequest(profile, Method.GET);
                                                    client.ExecuteAsync(photoRequest, (photoResponse) =>
                                                    {
                                                        if (photoResponse.StatusCode == HttpStatusCode.OK)
                                                        {
                                                            string output = photoResponse.Content;
                                                            byte[] bytes = photoResponse.RawBytes;
                                                            //System.Buffer.BlockCopy(output.ToCharArray(), 0, bytes, 0, bytes.Length);
                                                            using (MemoryStream stream = new MemoryStream(bytes))
                                                            {
                                                                stream.Seek(0, SeekOrigin.Begin);
                                                                BitmapImage b = new BitmapImage();
                                                                b.SetSource(stream);
                                                                if (!dict.ContainsKey(profile))
                                                                    dict.Add(profile, b);
                                                                postItem.authorAvatar = dict[profile];
                                                                //blogItem.Items.Add(postItem);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            MessageBox.Show(AppResources.ErrorOccured + photoResponse.StatusCode.ToString() + AppResources.Report, AppResources.Error, MessageBoxButton.OK);
                                                        }
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                BitmapImage bitmap = new BitmapImage();
                                                bitmap.UriSource = new Uri("/icons/blogger.png", UriKind.Relative);
                                                postItem.authorAvatar = bitmap;
                                            }
                                            blogItem.Items.Add(postItem);
                                        }
                                    }
                                    this.blogCollection.Add(blogItem);
                                }
                                else
                                {
                                    MessageBox.Show(AppResources.ErrorOccured + postResponse.StatusCode.ToString() + AppResources.Report, AppResources.Error, MessageBoxButton.OK);
                                }
                            });
                        }
                    }
                    (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                    App.prog.IsIndeterminate = false;
                    App.prog.IsVisible = false;
                }
                else
                {
                    MessageBox.Show(AppResources.ErrorOccured + resp.StatusCode.ToString() + AppResources.Report, AppResources.Error, MessageBoxButton.OK);
                }
            });
        }
    }
}
