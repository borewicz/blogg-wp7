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
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Linq;
using System.Collections.ObjectModel;
using RestSharp;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Phone.Shell;
using Newtonsoft.Json.Linq;

namespace Blogg
{
    public class PostItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string blogID { get; set; }
        public DateTime time { get; set; }
        public string author { get; set; }
    }

    //public class draftItem
    //{
    //    public string id { get; set; }
    //    public string name { get; set; }
    //    public string time { get; set; }
    //    public string www { get; set; }

    //    public draftListClass(string name, string time, string id, string www)
    //    {
    //        this.id = id;
    //        this.name = name;
    //        this.time = time;
    //        this.www = www;
    //    }
    //}

    public class BlogItem
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
    }

    public class blogger
    {
        //public List<postList> list { get; set; }
        //public ObservableCollection<PostItem> postCollection { get; set; }
        public ObservableCollection<BlogItem> blogCollection { get; set; }
        //public ObservableCollection<draftListClass> draftCollection { get; set; }
        public string authenticationToken { get; set; }
        //public string userName { get; set; }
        //public string password { get; set; }
        //public string blogID { get; set; }
        private MemoryStream memoryStream;

        public blogger()
        {
            //postCollection = new ObservableCollection<PostItem>();
            blogCollection = new ObservableCollection<BlogItem>();
            //draftCollection = new ObservableCollection<draftListClass>();
            //isVisible = newV
            //blogID = "6247561895747118623";
            //doLogin();
        }

        public void doLogin()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.google.com/accounts/ClientLogin");
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            request.BeginGetRequestStream(new AsyncCallback(GetRequest), request);
        }

        public void GetRequest(IAsyncResult asyncResult)
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            string email, password, blogID;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>("email", out email);
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>("password", out password);
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>("blogID", out blogID);
            //System.Diagnostics.Debug.WriteLine(email + " " + password);
            if (email != null || password != null)
            {
                string postData = "accountType=HOSTED_OR_GOOGLE";
                postData += "&Email=" + email.ToString();
                postData += "&Passwd=" + password.ToString();
                postData += "&service=blogger";
                postData += "&source=test-test-.01";
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                HttpWebRequest request = (HttpWebRequest)asyncResult.AsyncState;
                Stream newStream = request.EndGetRequestStream(asyncResult);
                newStream.Write(byteArray, 0, byteArray.Length);
                newStream.Close();
                request.BeginGetResponse(state =>
                {
                    string responseFromServer = App.blog.ReadResponse(state);
                    if (responseFromServer == null) return;
                    App.blog.authenticationToken = responseFromServer.Substring(responseFromServer.IndexOf("Auth=") + 5);
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            if (blogID != null)
                            {
                                //App.blog.blogID = blogID;
                                (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                            }
                            else
                            {
                                (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/blogList.xaml", UriKind.Relative));
                            }
                        }); 
                }, request);
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>  (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/settingsPage.xaml", UriKind.Relative)));
            }

        }

        private void OnError(IAsyncResult ar)
        {
            int? buttonIndex = Guide.EndShowMessageBox(ar);
            switch (buttonIndex)
            {
                case 0:
                    Deployment.Current.Dispatcher.BeginInvoke(() => doLogin());
                    break;
                case 1:
                    Deployment.Current.Dispatcher.BeginInvoke(() => (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/settingsPage.xaml", UriKind.Relative)));
                    break;
                default:
                    break;
            }
        }

        public string ReadResponse(IAsyncResult result)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)result.AsyncState;
                WebResponse response = request.EndGetResponse(result);
                System.Diagnostics.Debug.WriteLine(((HttpWebResponse)response).StatusDescription);
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
                //allDone.Set();
                return responseFromServer;
            }
            catch
            {
                Guide.BeginShowMessageBox("I can't connect to Blogger service.", "Check your credentials or tap Retry.", new string[] { "retry", "settings" }, 1, MessageBoxIcon.None,
                              new AsyncCallback(OnError), null);
                return null;
            }
        }

        public void GetBlogsList()
        {
            var client = new RestClient();
            var request = new RestRequest("https://www.googleapis.com/blogger/v3/users/self/blogs", Method.GET);
            request.AddHeader("Authorization", "Bearer " + oAuth.access_token);
            //System.Diagnostics.Debug.WriteLine("Bearer " + oAuth.access_token);

            client.ExecuteAsync(request, (resp) =>
            {
                //System.Diagnostics.Debug.WriteLine(resp.Content);
                if (resp.StatusCode == HttpStatusCode.OK)
                {
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

                                            blogItem.Items.Add(postItem);
                                        }
                                    }
                                    this.blogCollection.Add(blogItem);
                                }
                                else { MessageBox.Show(postResponse.StatusCode.ToString()); }
                            });
                        }
                    }
                    (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                }
                else MessageBox.Show(resp.StatusCode.ToString());
            });
            
            //System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() => { App.prog.IsVisible = true; });
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.blogger.com/feeds/default/blogs");
            //request.Method = "GET";
            //request.Headers["Authorization"] = "GoogleLogin auth=" + authenticationToken;
            //request.BeginGetResponse(new AsyncCallback(GetBlogListResponse), request);
        }

        //private void GetBlogListResponse(IAsyncResult asyncResult)
        //{
        //    string response = ReadResponse(asyncResult);
        //    if (response == null) return;
        //    //System.Diagnostics.Debug.WriteLine(response);
        //    //XDocument portfolio = XDocument.Load(response);
        //    XElement portfolio = XElement.Parse(response);

        //    XName name = XName.Get("entry", "http://www.w3.org/2005/Atom");
        //    XName title = XName.Get("title", "http://www.w3.org/2005/Atom");
        //    XName id = XName.Get("id", "http://www.w3.org/2005/Atom");
        //    XName summary = XName.Get("summary", "http://www.w3.org/2005/Atom");

        //    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
        //    {
        //        foreach (XElement p in portfolio.Descendants(name))
        //        {
        //            blogCollection.Add(new blogItem(p.Element(title).Value, p.Element(summary).Value, extractBlogID(p.Element(id).Value, 2, '-')));
        //            System.Diagnostics.Debug.WriteLine(extractBlogID(p.Element(id).Value, 2, '-'));
        //            System.Diagnostics.Debug.WriteLine(p.Element(title).Value);
        //            System.Diagnostics.Debug.WriteLine(p.Element(summary).Value);
        //        }
        //        App.prog.IsVisible = false;
        //    });
        //}

        //public void GetPostList(string blogID)
        //{
        //    App.prog.IsVisible = true;
        //    App.blog.postCollection.Clear();
        //    //App.blog.draftCollection.Clear();
        //    //6247561895747118623
        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.blogger.com/feeds/" + blogID + "/posts/default?max-results=20");
        //    request.Method = "GET";
        //    request.Headers["Authorization"] = "GoogleLogin auth=" + authenticationToken;
        //    request.BeginGetResponse(new AsyncCallback(GetPostListResponse), request);
        //}

        //private void GetPostListResponse(IAsyncResult asyncResult)
        //{
        //    string response = ReadResponse(asyncResult);
        //    if (response == null) return;
        //    //System.Diagnostics.Debug.WriteLine(response);
        //    //XDocument portfolio = XDocument.Load(response);
        //    XElement postlist = XElement.Parse(response);

        //    XNamespace ns = "http://purl.org/atom/app#";
        //    XName name = XName.Get("entry", "http://www.w3.org/2005/Atom");
        //    XName title = XName.Get("title", "http://www.w3.org/2005/Atom");
        //    XName id = XName.Get("id", "http://www.w3.org/2005/Atom");
        //    XName published = XName.Get("published", "http://www.w3.org/2005/Atom");
        //    XName link = XName.Get("link", "http://www.w3.org/2005/Atom");

        //    //System.Diagnostics.Debug.WriteLine(postlist.ToString());

        //    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() => {
        //    foreach (XElement p in postlist.Descendants(name))
        //    {
        //        //System.Diagnostics.Debug.WriteLine();
        //        //if (p.Element(link).Attribute("rel").Value == "alternate")
        //        //System.Diagnostics.Debug.WriteLine(p.Elements(link).Attributes("href"));

        //        //if (p.Element(ns + "control") != null)
        //        //{
        //        //    draftCollection.Add(new draftListClass(p.Element(title).Value, extractBlogID(p.Element(published).Value, 0, 'T'), extractBlogID(p.Element(id).Value, 2, '-'), p.Descendants(link).Attributes("href").Last().Value));
        //        //}
        //        postCollection.Add(new postItem(p.Element(title).Value, extractBlogID(p.Element(published).Value, 0, 'T'), extractBlogID(p.Element(id).Value, 2, '-'), p.Descendants(link).Attributes("href").Last().Value));
        //    }
        //    App.prog.IsVisible = false;
        //    });
            
        //}

        public void sendPost(string blogID, string title, string content, string categories, int draft)
        {
            App.prog.IsVisible = true;
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;

            memoryStream = new MemoryStream();

            XmlWriter writer = XmlWriter.Create(memoryStream, settings);
            writer.WriteStartDocument();

            //==========================
            writer.WriteStartElement("entry", "http://www.w3.org/2005/Atom");
            writer.WriteAttributeString("xmlns", "http://www.w3.org/2005/Atom");
            //==========================
            if (draft == 1)
            {
                writer.WriteStartElement("app", "control", "http://www.w3.org/2007/app");
                writer.WriteStartElement("draft", "http://www.w3.org/2007/app");
                writer.WriteString("yes");
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            //==========================
            writer.WriteStartElement("title");
            writer.WriteAttributeString("type", "text");
            writer.WriteString(title);
            writer.WriteEndElement();
            //==========================
            writer.WriteStartElement("content");
            writer.WriteAttributeString("type", "xhtml");
            writer.WriteString(content);
            writer.WriteEndElement();
            //==========================
            if (categories != "")
            {
                string[] cat = categories.Split(',');
                foreach (string i in cat)
                {
                    writer.WriteStartElement("category");
                    writer.WriteAttributeString("scheme", "http://www.blogger.com/atom/ns#");
                    writer.WriteAttributeString("term", i);
                    writer.WriteEndElement();
                }
            }
            
            writer.WriteEndElement();
            //==========================
            writer.WriteEndDocument();
            writer.Flush();
            writer.Close();
            
            var client = new RestClient("http://www.blogger.com/");
            var request = new RestRequest("feeds/" + blogID + "/posts/default/", Method.POST);

            request.AddHeader("Authorization", "GoogleLogin auth=" + authenticationToken);
            request.AddHeader("GData-Version", "2");

            byte[] bytes = memoryStream.ToArray();
            string output = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            //System.Diagnostics.Debug.WriteLine(output);
            request.AddParameter("application/atom+xml", output, ParameterType.RequestBody);

            client.ExecuteAsync(request, (resp) =>
            {
                if (resp.StatusCode == HttpStatusCode.Created) 
                {
                    (Application.Current.RootVisual as PhoneApplicationFrame).GoBack(); ;
                }
                else { MessageBox.Show(resp.StatusCode.ToString()); }
                App.prog.IsVisible = false;
            });
            memoryStream.Close();
        }

        public void updatePost(string blogID, string postID, string tit, string cont, string cats, XElement updatedPost)
        {
            App.prog.IsVisible = true;

            XNamespace ns = "http://purl.org/atom/app#";
            XName name = XName.Get("entry", "http://www.w3.org/2005/Atom");
            XName title = XName.Get("title", "http://www.w3.org/2005/Atom");
            XName content = XName.Get("content", "http://www.w3.org/2005/Atom");
            XName categories = XName.Get("category", "http://www.w3.org/2005/Atom");
            XName id = XName.Get("id", "http://www.w3.org/2005/Atom");

            //XElement postlist = XElement.Parse(;
            updatedPost.Descendants(content).FirstOrDefault().Value = cont;
            updatedPost.Descendants(title).FirstOrDefault().Value = tit;
            //var removedCategories = (from node in updatedPost.Descendants(categories) select node).First();

            //removedCategories.Remove();
            var delCategories = updatedPost.Descendants(categories);
            delCategories.Remove();

            if (cats != null)
            {
                string[] cat = cats.Split(',');

                foreach (string i in cat)
                {
                    if (i != "")
                    {
                        XElement node = new XElement(categories,
                            new XAttribute("scheme", "http://www.blogger.com/atom/ns#"),
                            new XAttribute("term", i));
                        updatedPost.Add(node);
                    }
                }
            }
            //System.Diagnostics.Debug.WriteLine(updatedPost.ToString());
            //http://www.blogger.com/feeds/blogID/posts/default/postID
            var client = new RestClient("http://www.blogger.com/");
            var request = new RestRequest("feeds/" + blogID + "/posts/default/" + postID, Method.PUT);

            request.AddHeader("Authorization", "GoogleLogin auth=" + authenticationToken);
            request.AddHeader("GData-Version", "2");
            request.AddParameter("application/atom+xml", updatedPost.ToString(), ParameterType.RequestBody);

            client.ExecuteAsync(request, (resp) =>
            {
                if (resp.StatusCode == HttpStatusCode.OK) {
                    (Application.Current.RootVisual as PhoneApplicationFrame).GoBack();
                }
                else MessageBox.Show(resp.StatusCode.ToString());
                App.prog.IsVisible = false;
            });
        }

        public void publishPost(string blogID, string postID)
        {
            App.prog.IsVisible = true;
            XNamespace ns = "http://purl.org/atom/app#";
            XName name = XName.Get("entry", "http://www.w3.org/2005/Atom");
            XName title = XName.Get("title", "http://www.w3.org/2005/Atom");
            XName content = XName.Get("content", "http://www.w3.org/2005/Atom");
            XName categories = XName.Get("category", "http://www.w3.org/2005/Atom");
            XName id = XName.Get("id", "http://www.w3.org/2005/Atom");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.blogger.com/feeds/" + blogID + "/posts/default");
            request.Method = "GET";
            request.Headers["Authorization"] = "GoogleLogin auth=" + App.blog.authenticationToken;
            //request.BeginGetResponse(new AsyncCallback(GetPostListResponse), request);
            request.BeginGetResponse(state =>
            {

                //var response = webRequest.EndGetResponse(asyncResult);
                string response = App.blog.ReadResponse(state);
                if (response == null) return;
                XElement postlist = XElement.Parse(response);

                XElement activePost = (
                    from a in postlist.Elements()
                    where (string)a.Element(id) == "tag:blogger.com,1999:blog-" + blogID + ".post-" + postID
                    select a
                ).First();

                var delCategories = activePost.Descendants(ns + "config");
                delCategories.Remove();

                var client = new RestClient("http://www.blogger.com/");
                var req = new RestRequest("feeds/" + blogID + "/posts/default/" + postID, Method.PUT);

                req.AddHeader("Authorization", "GoogleLogin auth=" + authenticationToken);
                req.AddHeader("GData-Version", "2");
                req.AddParameter("application/atom+xml", activePost.ToString(), ParameterType.RequestBody);

                client.ExecuteAsync(req, (resp) =>
                {
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (resp.StatusCode == HttpStatusCode.OK)
                        { 
                            //GetPostList(App.blog.blogID);
                            App.prog.IsVisible = false;
                        }
                        else { MessageBox.Show(resp.StatusCode.ToString()); }
                    });
                });

            }, request);
        }

        public void deletePost(string blogID, string postID)
        {
            App.prog.IsVisible = true;
            var client = new RestClient("http://www.blogger.com/");
            var request = new RestRequest("feeds/" + blogID + "/posts/default/" + postID, Method.DELETE);

            request.AddHeader("Authorization", "GoogleLogin auth=" + authenticationToken);
            request.AddHeader("GData-Version", "2");

            client.ExecuteAsync(request, (resp) =>
            {
                if (resp.StatusCode == HttpStatusCode.OK) 
                { 
                    //GetPostList(App.blog.blogID);
                    App.prog.IsVisible = false;
                }
                else { MessageBox.Show(resp.StatusCode.ToString()); }
            });
        }

        public void UploadPhoto(Stream photo)
        {
            var client = new RestClient("https://picasaweb.google.com/");
            var request = new RestRequest("data/feed/api/user/default/albumid/default", Method.POST);

            request.AddHeader("Authorization", "GoogleLogin auth=" + authenticationToken);
            request.AddHeader("GData-Version", "2");

            StreamReader reader = new StreamReader(photo);
            string output = reader.ReadToEnd();

            request.AddParameter("image/jpeg", output, ParameterType.RequestBody);

            client.ExecuteAsync(request, (resp) =>
            {
                if (resp.StatusCode == HttpStatusCode.OK) { }
                else { System.Diagnostics.Debug.WriteLine(resp.StatusCode.ToString(), resp.StatusDescription.ToString()); }
            });
        }
    }
}
