using System;
using System.Net;
using Reddit;
using Reddit.Controllers.EventArgs;
using Telegram.Bot;
using System.IO;
using System.IO.Compression;
using NYoutubeDL;
using Telegram.Bot.Types;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RedditToTelegram
{
    public class credentials
    {
        public string appId { get; set; }
        public string appSecret { get; set; }
        public string refreshToken { get; set; }
        public string SUBREDDIT { get; set; }
        public string BOT_TOKEN { get; set; }
        public Int64 chatId { get; set; }
        public bool Hot { get; set; }

    }
    class Program
    {
        List<string> Cache; //using list because of .RemoveAt

        static void Main(string[] args)
        {
            Program entry = new Program();
            List<string> lists = new List<string>();
            entry.Cache = lists;
            entry.init();
               
        }
        public void init()
        {
            credentials credentials;
            using (StreamReader stream = new StreamReader("credentials.json"))
            {
                string json = stream.ReadToEnd();
                credentials = JsonConvert.DeserializeObject<credentials>(json);
            }

            CheckTools();
            var r = new RedditClient(appId: Convert.ToString(credentials.appId), appSecret: Convert.ToString(credentials.appSecret),
                refreshToken: Convert.ToString(credentials.refreshToken));
            var sub = r.Subreddit(Convert.ToString(credentials.SUBREDDIT));

            Console.WriteLine("waiting for posts...");
            if (credentials.Hot)
            {
                Console.WriteLine("HOT");
                _ = sub.Posts.GetHot();
                sub.Posts.HotUpdated += C_postsUpdated;
                sub.Posts.MonitorHot();
            }
            else
            {
                Console.WriteLine("NEW");
                _ = sub.Posts.GetNew();
                sub.Posts.NewUpdated += C_postsUpdated;
                sub.Posts.MonitorNew();
            }
        }

            public async void C_postsUpdated(object sender, PostsUpdateEventArgs e)
        {
            
            credentials credentials;
            using (StreamReader stream = new StreamReader("credentials.json"))
            {
                string json = stream.ReadToEnd();
                credentials = JsonConvert.DeserializeObject<credentials>(json);
            }
            TelegramBotClient botClient = new TelegramBotClient(credentials.BOT_TOKEN);

            foreach (var post in e.Added)
            {
                if (!post.Listing.IsVideo && post.Listing.IsRedditMediaDomain && !InCache(post.Permalink))
                {
                    Console.WriteLine(post.Listing.URL);
                    botClient.SendPhotoAsync(chatId: credentials.chatId , photo: post.Listing.URL, caption: post.Title);

                }
                else if (post.Listing.IsVideo && post.Listing.IsRedditMediaDomain && !InCache(post.Permalink))
                {
                    string filename = VideoDownload(post.Listing.Permalink);
                    var file = System.IO.File.OpenRead("./"+filename);
                    Message msg = await botClient.SendVideoAsync(chatId: credentials.chatId, video: file, caption: post.Title, supportsStreaming:true);
                    file.Close();
                    Console.WriteLine("Video Sent!");
                    Console.WriteLine("Deleting local file...");
                    System.IO.File.Delete("./"+filename);
                  
                }


            }
        }
        private bool InCache(string url)
        {
            if (Cache.Contains(url))
            {
                Console.WriteLine(url, "In Cache");
                if(Cache.Count >= 32)
                {
                    Cache.RemoveAt(0);
                }
                return true;
            }
            else
            {
                Console.WriteLine(url, "Not in Cache");
                Cache.Add(url);
                return false;
            }
        }
        static string VideoDownload(string url)
        {
            var youtubeDl = new YoutubeDL();
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                youtubeDl.YoutubeDlPath = "./tools/youtube-dl.exe";
            }
            else
            {
                youtubeDl.YoutubeDlPath = "./tools/youtube-dl";         
            }
            string filename = url.Split("/")[url.Split("/").Length-2]+".mp4";
            youtubeDl.Options.FilesystemOptions.Output = filename;
            youtubeDl.VideoUrl = "https://reddit.com" + url;
            Console.WriteLine("https://reddit.com" + url);
            youtubeDl.StandardOutputEvent += (sender, output) => Console.WriteLine(output);
            youtubeDl.StandardErrorEvent += (sender, errorOutput) => Console.WriteLine(errorOutput);
            string commandToRun = youtubeDl.PrepareDownload();
            youtubeDl.Download();
            youtubeDl = null;
            return filename;
        }

        static void CheckTools()
        {
            using (var client = new WebClient())
            {
                if (!Directory.Exists("./tools"))
                {
                    Directory.CreateDirectory("./tools");
                }
                Console.WriteLine("Check if youtube-dl exist...");
                if (!(System.IO.File.Exists("./tools/youtube-dl.exe") || System.IO.File.Exists("./tools/youtube-dl")))
                {

                    Console.WriteLine("Download of Youtube-dl");
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        client.DownloadFile("https://yt-dl.org/downloads/2021.01.08/youtube-dl.exe", "./tools/youtube-dl.exe");
                    }
                    else
                    {
                        client.DownloadFile("https://yt-dl.org/downloads/latest/youtube-dl", "./tools/youtube-dl");

                    }
                    Console.WriteLine("Done with youtube-dl");
                }
                Console.WriteLine("Check if ffmpeg exist...");
                if (!System.IO.File.Exists("./tools/ffmpeg.exe") && !RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Console.WriteLine("Downloading ffmpeg.exe, might take some time");
                    client.DownloadFile("https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip", "./tools/ffmpeg.zip");
                    ZipFile.ExtractToDirectory("./tools/ffmpeg.zip", "./tools/ffmpeg/");
                    System.IO.File.Move(Directory.GetDirectories("./tools/ffmpeg/")[0] + "/bin/ffmpeg.exe", "./tools/ffmpeg.exe");
                    System.IO.File.Delete("./tools/ffmpeg.zip");
                    Directory.Delete("./tools/ffmpeg/", true);
                    Console.WriteLine("Done with ffmpeg");
                }

            }
        }
    }

}
