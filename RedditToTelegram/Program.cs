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
        static void Main(string[] args)
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

            
            if (credentials.Hot)
            {
                _ = sub.Posts.GetHot();
                sub.Posts.HotUpdated += C_postsUpdated;
                sub.Posts.MonitorHot();
            }
            else
            {
                _ = sub.Posts.GetNew();
                sub.Posts.NewUpdated += C_postsUpdated;
                sub.Posts.MonitorNew();
            }
               
        }
    
        public static async void C_postsUpdated(object sender, PostsUpdateEventArgs e)
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
                if (!post.Listing.IsVideo && post.Listing.IsRedditMediaDomain)
                {
                    Console.WriteLine(post.Listing.URL);
                    botClient.SendPhotoAsync(chatId: credentials.chatId , photo: post.Listing.URL, caption: post.Title);

                }
                else if (post.Listing.IsVideo && post.Listing.IsRedditMediaDomain)
                {
                    VideoDownload(post.Listing.Permalink);
                    var file = System.IO.File.OpenRead("./video.mp4");
                    Message msg = await botClient.SendVideoAsync(chatId: credentials.chatId, video: file, caption: post.Title, supportsStreaming:true);
                    file.Close();
                    Console.WriteLine("Video Sent!");
                    Console.WriteLine("Deleting local file...");
                    System.IO.File.Delete("./video.mp4");
                  
                }


            }
        }
        static void VideoDownload(string url)
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

            youtubeDl.Options.FilesystemOptions.Output = "./video.mp4";
            youtubeDl.VideoUrl = "https://reddit.com" + url;
            Console.WriteLine("https://reddit.com" + url);
            youtubeDl.StandardOutputEvent += (sender, output) => Console.WriteLine(output);
            youtubeDl.StandardErrorEvent += (sender, errorOutput) => Console.WriteLine(errorOutput);
            string commandToRun = youtubeDl.PrepareDownload();
            youtubeDl.Download();
            youtubeDl = null;
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
