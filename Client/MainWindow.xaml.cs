using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Threading;
using System.Web;       // need to add reference to System.Web
using System.Net;       // need to add reference to System.Net
using Newtonsoft.Json;  // need to add reference to System.Json
using System.Threading;
using System.Net.Http;
using System.Xml;  // need to add reference to System.Net.Http


namespace Client
{


    public class TestClient
    {
        private HttpClient client = new HttpClient();
        private HttpRequestMessage message;
        private HttpResponseMessage response = new HttpResponseMessage();
        private string urlBase;
        public string storyName { get; set; }
        public string status { get; set; }

        //----< set destination url >------------------------------------------
        public TestClient(string url) { urlBase = url; }

        //----< get list of files available for download >---------------------

        public string[] getAvailableFiles()
        {
            message = new HttpRequestMessage();
            message.Method = HttpMethod.Get;
            message.RequestUri = new Uri(urlBase);
            Task<HttpResponseMessage> task = client.SendAsync(message);
            HttpResponseMessage response1 = task.Result;
            response = task.Result;
            status = response.ReasonPhrase;
            string[] files = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(response1.Content.ReadAsStringAsync().Result);
            return files;
        }
        //----< open file on server for reading >------------------------------

        int openServerDownLoadFile(string fileName)
        {
            message = new HttpRequestMessage();
            message.Method = HttpMethod.Get;
            string urlActn = "?fileName=" + fileName + "&open=download";
            message.RequestUri = new Uri(urlBase + urlActn);
            Task<HttpResponseMessage> task = client.SendAsync(message);
            HttpResponseMessage response = task.Result;
            status = response.ReasonPhrase;
            return (int)response.StatusCode;
        }
        //----< open file on client for writing >------------------------------

        FileStream openClientDownLoadFile(string donwload)
        {

            FileStream down;
            try
            {
                down = new FileStream(donwload, FileMode.OpenOrCreate);
            }
            catch
            {
                return null;
            }
            return down;
        }
        //----< read block from server file and write to client file >---------

        byte[] getFileBlock(FileStream down, int blockSize)
        {
            message = new HttpRequestMessage();
            message.Method = HttpMethod.Get;
            string urlActn = "?blockSize=" + blockSize.ToString();
            message.RequestUri = new Uri(urlBase + urlActn);
            Task<HttpResponseMessage> task = client.SendAsync(message);
            HttpResponseMessage response = task.Result;
            Task<byte[]> taskb = response.Content.ReadAsByteArrayAsync();
            byte[] Block = taskb.Result;
            status = response.ReasonPhrase;
            return Block;
        }
        //----< close FileStream on server and FileStream on client >----------

        void closeServerFile()
        {
            message = new HttpRequestMessage();
            message.Method = HttpMethod.Get;
            string urlActn = "?fileName=dontCare.txt&open=close";
            message.RequestUri = new Uri(urlBase + urlActn);
            Task<HttpResponseMessage> task = client.SendAsync(message);
            HttpResponseMessage response = task.Result;
            status = response.ReasonPhrase;
        }

        /*void updateDatabase(string collageId, string fileName, string content, string order, string duration)
        {
            message = new HttpRequestMessage();
            message.Method = HttpMethod.Get;
            string urlActn = "?fileName=" + collageId + "|" + fileName + "|" + content + "|" + order + "|" + duration + "&open=update";
            message.RequestUri = new Uri(urlBase + urlActn);
            Task<HttpResponseMessage> task = client.SendAsync(message);
            HttpResponseMessage response = task.Result;
            status = response.ReasonPhrase;
        }*/
        //----< open file on server for writing >------------------------------

        int openServerUpLoadFile(string fileName)
        {
            message = new HttpRequestMessage();
            message.Method = HttpMethod.Get;
            string urlActn = "?fileName=" + fileName + "&open=upload";
            message.RequestUri = new Uri(urlBase + urlActn);
            Task<HttpResponseMessage> task = client.SendAsync(message);
            HttpResponseMessage response = task.Result;
            status = response.ReasonPhrase;
            return (int)response.StatusCode;
        }
        //----< open file on client for Reading >------------------------------

        FileStream openClientUpLoadFile(string fileName)
        {
            FileStream up;
            try
            {
                up = new FileStream(fileName, FileMode.Open);
            }
            catch
            {
                return null;
            }
            return up;
        }
        //----< post blocks to server >----------------------------------------
        void putBlock(byte[] Block)
        {
            message = new HttpRequestMessage();
            message.Method = HttpMethod.Post;
            message.Content = new ByteArrayContent(Block);
            message.Content.Headers.Add("Content-Type", "application/http;msgtype=request");
            string urlActn = "?blockSize=" + Block.Count().ToString();
            message.RequestUri = new Uri(urlBase + urlActn);
            Task<HttpResponseMessage> task = client.SendAsync(message);
            HttpResponseMessage response = task.Result;
            status = response.ReasonPhrase;
        }
        //----< downLoad File >------------------------------------------------
        /*
         *  Open server file for reading
         *  Open client file for writing
         *  Get blocks from server
         *  Write blocks to local file
         *  Close server file
         *  Close client file
         */
        public void downLoadFile(string filename, string path)
        {

            FileStream down;



            int status = openServerDownLoadFile(filename);

            filename = path + "\\" + filename;
            if (status >= 400)
                return;



            down = openClientDownLoadFile(filename);


            while (true)
            {
                int blockSize = 512;
                byte[] Block = getFileBlock(down, blockSize);


                if (Block.Length == 0 || blockSize <= 0)
                    break;
                down.Write(Block, 0, Block.Length);
                if (Block.Length < blockSize)    // last block
                    break;
            }




            closeServerFile();


            down.Close();
        }
        //----< upLoad File >--------------------------------------------------
        /*
         *  Open server file for writing
         *  Open client file for reading
         *  Read blocks from local file
         *  Send blocks to server
         *  Close server file
         *  Close client file
         */
        public void upLoadFile(string filename)
        {
            //  Attempting to upload file {0}", filename

            //  Sending get request to open file");
            //string temp_file_name = collageId + "\\" + DateTime.Now.Ticks + Path.GetExtension(filename);
            openServerUpLoadFile(filename);
            //  Response status = {0}\n", status
            FileStream up = openClientUpLoadFile(filename);

            //  Sending Post requests to send blocks

            const int upBlockSize = 512;
            byte[] upBlock = new byte[upBlockSize];
            int bytesRead = upBlockSize;
            while (bytesRead == upBlockSize)
            {
                bytesRead = up.Read(upBlock, 0, upBlockSize);
                if (bytesRead < upBlockSize)
                {
                    byte[] temp = new byte[bytesRead];
                    for (int i = 0; i < bytesRead; ++i)
                        temp[i] = upBlock[i];
                    upBlock = temp;
                }
                //  sending block of size {0}", upBlock.Count()
                putBlock(upBlock);
                //  status = {0}\n", status
            }

            //  Sending Get request to close file
            closeServerFile();
            //updateDatabase(collageId, temp_file_name, content, order, duration);
            //  Response status = {0}\n", status
            up.Close();
            string a = filename;
        }


    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TestClient tc;
        public MainWindow()
        {
            InitializeComponent();
            tc = new TestClient("http://localhost:49695/api/File");

        }

        private void ChooseImage_Click(object sender, RoutedEventArgs e)
        {
            string resultFile = "";
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = "c:\\";
            dialog.Filter = "All files (*.*)|*.*|All Image Files|*.bmp;*.ico;*.gif;*.jpeg;*.jpg;*.png;*.tif;*.tiff";
            dialog.FilterIndex = 2;
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                resultFile = dialog.FileName;

            ImageUrl.Text = resultFile;

        }
        private void Upload_Click(object sender, RoutedEventArgs e)
        {

            if (this.Title.Text == "" || this.Title.Text == null)
            {
                this.Alert.Content = "Please input title of story.";
                return;
            }
            else if (this.Text.Text == "" || this.Text.Text == null)
            {
                this.Alert.Content = "Please input text of story.";
                return;
            }
            else if (this.ImageUrl.Text == "" || this.ImageUrl.Text == null)
            {
                this.Alert.Content = "Please choose an photo to upload.";
                return;
            }



            string uploadFile = ImageUrl.Text;

            string name = System.IO.Path.GetFileName(ImageUrl.Text);

            string uploadText = generateXMLfile(this.Title.Text, this.Text.Text, name);
            tc.upLoadFile(uploadFile);
            tc.upLoadFile(uploadText);
            this.Alert.Content = "Story block uploaded successful!";


        }

        private void DownloadStory_Click(object sender, RoutedEventArgs e)
        {
            string[] files = null;
            files = tc.getAvailableFiles();
            Thread.Sleep(100);
            if (files.Length == 0)
                return;

            string filename = files[files.Length - 1];
            if (this.DownloadPath.Text == "" || this.DownloadPath.Text == null)
            {
                this.Alert.Content = "Please choose a path to download";
                return;
            }
            string path = DownloadPath.Text;
            tc.downLoadFile(filename, path);
        }

        public string generateXMLfile(string title, string text, string img)
        { 
            XmlDocument doc=new XmlDocument();　　  
            
            string fileName = System.IO.Directory.GetCurrentDirectory();
            fileName += "/StoryBlock/" + "story.xml";

            XmlNode node = doc.CreateXmlDeclaration("1.0", "utf-8", "");
            doc.AppendChild(node);

            XmlElement xmlRoot = doc.CreateElement("StoryBlock");

            doc.AppendChild(xmlRoot);
　  
　          XmlElement xmlChild1 = doc.CreateElement("Title");

            xmlChild1.InnerText = title;

           XmlElement xmlChild2 = doc.CreateElement("Text");
            xmlChild2.InnerText = text;

            XmlElement xmlChild3 = doc.CreateElement("Image");
            xmlChild3.InnerText = "~\\CollagePages\\UploadedFiles\\" + img;

            xmlRoot.AppendChild(xmlChild1);
            xmlRoot.AppendChild(xmlChild2);
            xmlRoot.AppendChild(xmlChild3);
                
            doc.Save(fileName);
            
            return fileName;
        }


        private void ChoosePath_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog pathdlg = new FolderBrowserDialog();
            string path = AppDomain.CurrentDomain.BaseDirectory;
            pathdlg.SelectedPath = path;
            DialogResult result = pathdlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                DownloadPath.Text = pathdlg.SelectedPath;
            }
        }
    


    }


}
