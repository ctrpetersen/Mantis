using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Mantis
{
    class Mantis
    {
        //factorio
        //guild id 139677590393716737
        //live role id 441683574333112340
        //anime role id 334669440421462017
        //reaction msg id 549988110423818240

        //testing server
        //guild id 322868375774560267
        //live role id 
        //anime role id 645366358233841666
        //reaction msg id 645368798404280381

        internal DiscordSocketClient Client;
        internal SocketGuild Guild;
        internal ulong GuildID = 139677590393716737;
        internal ulong LiveRoleID = 441683574333112340;
        internal ulong AnimeRoleID = 334669440421462017;
        internal ulong ReactionMessageID = 549988110423818240;
        internal string BotName = "Mantis";
        internal string GameToTrack = "Factorio";
        internal string Token;
        internal long _workLoopIntervalTime = TimeSpan.FromMinutes(1).TotalMilliseconds;
        internal CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        internal async Task StartAsync()
        {
            Token = File.ReadAllText("token.txt");

            Client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Debug,
                MessageCacheSize = 100,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                AlwaysDownloadUsers = true
            });

            await Client.LoginAsync(TokenType.Bot, Token);
            await Client.StartAsync();

            Client.Ready += () =>
            {
                Guild = Client.GetGuild(GuildID);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("                             .   ,");
                Console.WriteLine("                              |_|");
                Console.WriteLine("                             (/ \\)");
                Console.WriteLine("                             |`='");
                Console.WriteLine("                             L L    ...");
                Console.WriteLine("                            J / ;._// |\\");
                Console.WriteLine("                            | |\\ `|' ,;,|");
                Console.WriteLine("                           _L L `'`'\"");
                Console.WriteLine("       _............._...-\": j");
                Console.WriteLine("      '.\\_\\_\\_\\_\\_\\_\\.:`_`.-:|");
                Console.WriteLine("        `-._:_:_:_:_:_:.-.-'||.===.");
                Console.WriteLine("                       //||'.-'::||");
                Console.WriteLine("                      // JJ    ||||    ___");
                Console.WriteLine("                     //   LL   ||||---' -");
                Console.WriteLine("                     ||__.\"\"---''-|\\ __  -");
                Console.WriteLine("              ____.--||  __  -- __  ___.---");
                Console.WriteLine("       __.---' __  - //--  ____.---'");
                Console.WriteLine(" __.--' __. --   - __\"_.--'");
                Console.WriteLine("'   .--    __.----'");
                Console.ResetColor();


                Log($"Logged in as {Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator}." +
                    $"\nServing {Client.Guilds.Count} guilds with a total of {Client.Guilds.Sum(g => g.Users.Count)} online users." +
                    $"\nLatency: {Client.Latency} ms");

                Task.Run(() => WorkLoop(_cancellationTokenSource.Token), _cancellationTokenSource.Token);

                return Task.CompletedTask;
            };

            Client.ReactionAdded += AddUserToRole;
            Client.ReactionRemoved += RemoveUserFromRole;

            await Task.Delay(-1);
            
            //_cancellationTokenSource.Cancel(); //Do this if you want to exit the work loop
        }
        
        private async Task WorkLoop(CancellationToken token) {
            while (!token.IsCancellationRequested) {
                try {
                    await Heartbeat(token);
                    await CheckUsers(token);
                } catch (Exception ex) {
                    Log($"Work Loop encountered unhandled exception: {ex.ToString()}");
                }
                await Task.Delay(_workLoopIntervalTime, token);
            }
        }

        private async Task CheckUsers(CancellationToken token)
        {
            foreach (var user in Guild.Users)
            {
                if (user.Activity != null && user.Activity.Type == ActivityType.CustomStatus && user.Activity is Game game)
                {
                    if (game.ToString().Replace(" ", string.Empty) == GameToTrack && user.Roles.All(r => r.Id != LiveRoleID))
                    {
                        await user.AddRoleAsync(Guild.GetRole(LiveRoleID));
                        Log($"Added live role to {user}");
                    }

                    else if (game.ToString().Replace(" ", string.Empty) != GameToTrack && user.Roles.Any(r => r.Id == LiveRoleID))
                    {
                        await user.RemoveRoleAsync(Guild.GetRole(LiveRoleID));
                        Log($"Removed live role from {user}");
                    }
                }

                else if (user.Roles.Any(r => r.Id == LiveRoleID))
                {
                    await user.RemoveRoleAsync(Guild.GetRole(LiveRoleID));
                    Log($"Removed live role from {user}");
                }
            }
        }

        private async Task Heartbeat(CancellationToken token)
        {
            if (Client.ConnectionState != ConnectionState.Connected)
            {
                Log("Disconnected! Reconnecting...", LogSeverity.Critical);
                await Client.Rest.LoginAsync(TokenType.Bot, Token);
                return;
            }
            
            Log($"Heartbeat | Latency {Client.Latency} ms");
        }

        private async Task AddUserToRole(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (message.Id != ReactionMessageID)
            {
                return;
            }

            var reactionAuthor = Guild.GetUser(reaction.UserId);

            if (reactionAuthor.Roles.Any(r => r.Id == AnimeRoleID)) return;

            await reactionAuthor.AddRoleAsync(Guild.GetRole(AnimeRoleID));
            Log($"Added {reactionAuthor.Username} to the anime role.");
        }

        private async Task RemoveUserFromRole(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (message.Id != ReactionMessageID)
            {
                return;
            }

            var reactionAuthor = Guild.GetUser(reaction.UserId);

            if (reactionAuthor.Roles.All(r => r.Id != AnimeRoleID)) return Task.CompletedTask;

            await reactionAuthor.RemoveRoleAsync(Guild.GetRole(AnimeRoleID));
            Log($"Removed {reactionAuthor.Username} from the anime role.");

        }

        internal static Task Log(string msg, LogSeverity severity = LogSeverity.Info, Exception exception = null, string source = "Mantis")
        {
            Console.ForegroundColor = severity switch
            {
                LogSeverity.Critical => ConsoleColor.Red,
                LogSeverity.Error => ConsoleColor.Red,
                LogSeverity.Warning => ConsoleColor.Yellow,
                LogSeverity.Info => ConsoleColor.White,
                LogSeverity.Verbose => ConsoleColor.DarkGray,
                LogSeverity.Debug => ConsoleColor.DarkGray,
                _ => Console.ForegroundColor
            };

            Console.WriteLine(
                $"{DateTime.Now} [{severity,8}] {source}: {msg} {(exception == null ? "" : exception.ToString())}");
            Console.ResetColor();
            return Task.CompletedTask;
        }
    }
}
