# RedditToTelegram
get posts in a subreddit and forward them to a telegram group/channel/user

# Dependencies NuGet
- Reddit.NET
- Telegram.Bot

# HOW TO INSTALL

WINDOWS:
```
just launch RedditToTelegram.exe, it should download Youtube-dl and FFmpeg automatically
```
LINUX:
```
-install ffmpeg with apt ( youtube-dl will be downloaded automatically )
-python installed
-chmod 777 the ./tools/youtube-dl file

```
place a file called `credentials.json` with the bot and reddit credentials using this format
```
{
  "appId": "appId",
  "appSecret": "appSecret",
  "refreshToken": "refreshToken",
  "SUBREDDIT": "SUBREDDIT",
  "BOT_TOKEN": "BOT_TOKEN",
  "chatId": "chatId",
  "Hot": true    (false is by new)
}
```
