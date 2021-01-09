using System;
using Reddit;
using Reddit.Controllers.EventArgs;
using Telegram.Bot;


namespace RedditToTelegram
{
    class Program
    {

        static void Main(string[] args)
        {
            var r = new RedditClient(appId: "APPId", appSecret: "APP_SECRET", refreshToken: "REFRESH_TOKEN");
            var askReddit = r.Subreddit("SUBREDDIT");

            askReddit.Posts.GetHot();
                askReddit.Posts.NewUpdated += C_HotPostsUpdated;
                askReddit.Posts.MonitorHot();

        }

        public static void C_HotPostsUpdated(object sender, PostsUpdateEventArgs e)
        {
            TelegramBotClient botClient = new TelegramBotClient("YOUR_BOT_TOKEN");

            foreach (var post in e.Added)
            {

                if (!post.Listing.IsVideo && post.Listing.IsRedditMediaDomain)
                {
                    Console.WriteLine(post.Title);
                    Console.WriteLine(post.Listing.URL);
                    botClient.SendPhotoAsync(chatId: INT_CHAT_ID , photo: post.Listing.URL, caption: post.Title);

                }


            }
        }
  
    }

}
