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
using System.Xml.Linq;

namespace Blogg
{
    public partial class editPost : PhoneApplicationPage
    {
        //CameraCaptureTask ctask;
        XElement activePost;

        XNamespace ns = "http://purl.org/atom/app#";
        XName name = XName.Get("entry", "http://www.w3.org/2005/Atom");
        XName title = XName.Get("title", "http://www.w3.org/2005/Atom");
        XName content = XName.Get("content", "http://www.w3.org/2005/Atom");
        XName categories = XName.Get("category", "http://www.w3.org/2005/Atom");
        XName id = XName.Get("id", "http://www.w3.org/2005/Atom");

        public editPost()
        {
            InitializeComponent();
            //ctask = new CameraCaptureTask();
            //ctask.Completed += new EventHandler<PhotoResult>(ctask_Completed);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (this.NavigationContext.QueryString.ContainsKey("blogid") && this.NavigationContext.QueryString.ContainsKey("postid"))
                getPost(NavigationContext.QueryString["blogid"], NavigationContext.QueryString["postid"]); 
        }

        void getPost(string blogID, string postID)
        {
            //6247561895747118623
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

                activePost = (
                    from a in postlist.Elements()
                    where (string)a.Element(id) == "tag:blogger.com,1999:blog-" + blogID + ".post-" + postID
                    select a
                ).First();

                var cats = from e in activePost.Elements()
                             select (string)e.Attribute("term");

                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    foreach (string cat in cats)
                    {
                        if (cat != null)
                        categoriesTextBox.Text += cat + ",";
                    }

                    contentTextBox.Text = activePost.Descendants(content).FirstOrDefault().Value;
                    titleTextBox.Text = activePost.Descendants(title).FirstOrDefault().Value;
                });
             }, request);
        }


        private void editButtonClick(object sender, System.EventArgs e)
        {
            //(sender as FrameworkElement).
            if (contentTextBox.Text != "")
            App.blog.updatePost(NavigationContext.QueryString["blogid"], NavigationContext.QueryString["postid"], titleTextBox.Text, contentTextBox.Text, categoriesTextBox.Text, activePost);
            else MessageBox.Show("You didn't fill required data!", "Error", MessageBoxButton.OK);
            //updatePost("xD", "xD", categoriesTextBox.Text, activePost);
        }

        /*
        void ctask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK && e.ChosenPhoto != null)
            {
                //MessageBox.Show("Uploading photo");
                App.blog.UploadPhoto(e.ChosenPhoto);
            }
            else
            {

            }
        }

        private void cameraButtonClick(object sender, System.EventArgs e)
        {
            ctask.Show();
        }
         */
    }
}