using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace dBot
{
    class Program
    {
        // Program entry point
        static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public readonly DiscordSocketClient client;
        private readonly IServiceCollection map = new ServiceCollection();
        private readonly CommandService commands = new CommandService();
        private bool NeedsJsonRead = true;
        private string BotToken = "NDEyMjcwOTU1MjI2NzI2NDEw.DWH0zQ.PXzg2SU7EbxWKoTbPr4kKMXwpfg";
        private ulong ServerID = 411177543442628610;

        public Program()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info
            });
            client.Log += Logger;
            commands.Log += Logger;
            // Read JSON data
            ReadJson();
        }
        private void ReadJson()
        {
            if (NeedsJsonRead)
            {
                

                NeedsJsonRead = false;
            }
        }
        private static Task Logger(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"({DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
            Console.ResetColor();
            return Task.CompletedTask;
        }
        private async Task MainAsync()
        {
            await InitCommands();
            await client.LoginAsync(TokenType.Bot, BotToken);
            await client.StartAsync();
            await Task.Delay(System.Threading.Timeout.Infinite);
        }

        private IServiceProvider services;

        private async Task InitCommands()
        {
            services = map.BuildServiceProvider();

            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
            client.MessageReceived += HandleCommandAsync;

        }
        private async Task HandleCommandAsync(SocketMessage arg)
        {
            // Bail out if it's a System Message.
            var msg = arg as SocketUserMessage;
            if (msg == null) return;

            if (msg.Author.Id == client.CurrentUser.Id || msg.Author.IsBot) return;

            int pos = 0;
            if (msg.HasCharPrefix('!', ref pos) /* || msg.HasMentionPrefix(_client.CurrentUser, ref pos) */)
            {
                var context = new SocketCommandContext(client, msg);
                var result = await commands.ExecuteAsync(context, pos, services);
                string command = context.Message.ToString().TrimStart('!');
                
                if (command == "ping")
                {
                    Console.WriteLine("pong");
                    await SendMessageToChannel(411177792814972928, "pong");
                }
            }
        }
        private async Task SendMessageToChannel(ulong channelID, string message)
        {
            client.GetGuild(ServerID).GetTextChannel(channelID).SendMessageAsync(message);
        }
    }
}