using libEraDeiFessi;
using libEraDeiFessi.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace EraDeiFessi.WebFormsApp.Telegram
{
    public class TelegramRepo
    {
        private static TelegramBotClient client;
        private static HashSet<int> processedUpdates;

        public static TelegramBotClient Client
        {
            get
            {
                if (client == null)
                {
                    client = new TelegramBotClient("232066451:AAE0MXSUIhozdFkbdRVTiT-xvFAW8kRQGUQ");
                    client.UpdateReceived += Client_UpdateReceived;
                }

                return client;
            }
        }

        public static HashSet<int> ProcessedUpdates
        {
            get
            {
                if (processedUpdates == null)
                {
                    processedUpdates = new HashSet<int>();
                }

                return processedUpdates;
            }
        }

        public static bool HookIsSet { get; set; } = false;

        private static void Client_UpdateReceived(object sender, global::Telegram.Bot.Args.UpdateEventArgs e)
        {
            var update = e.Update;


        }

        public static Task ProcessUpdateAsync(Update update)
        {
            return Task.Factory.StartNew(() => { ProcessUpdate(update); });
        }

        public static async void ProcessUpdate(Update update)
        {
            if (!ProcessedUpdates.Contains(update.Id))
            {
                switch (update.Type)
                {
                    case UpdateType.MessageUpdate:
                        string msg = update.Message.Text.Trim();
                        if(msg.StartsWith("/cerca", StringComparison.CurrentCultureIgnoreCase))
                        {
                            //Search()
                        }
                        await Client.SendTextMessageAsync(update.Message.Chat.Id,
                            "Hai scritto: " + update.Message.Text, replyToMessageId: update.Message.MessageId);
                        break;
                    case UpdateType.InlineQueryUpdate:
                        await Client.SendTextMessageAsync(update.Message.Chat.Id,
                            "Hai fatot una query in-line: " + update.Message.Text, replyToMessageId : update.Message.MessageId);
                        break;
                }
                //ProcessedUpdates.Add(update.Id);
            }
        }

        private static void Search(string query)
        {
            int count = 0;
            foreach (var plugin in PluginsRepo.Plugins.Values)
            {
                IEDFSearch searchplugin = plugin as IEDFSearch;
                if (plugin is IEDFSearch)
                {
                    PerformSearch(searchplugin, query);
                    count++;
                }

            }

        }

        private async static void PerformSearch(IEDFSearch searchplugin, string searchterm)
        {
            //return;
            //var searchres = await ParsingHelpers.PerformSearchAsync(searchplugin, searchterm);
            var searchres = await ParsingHelpers.PerformSearchAsync(searchplugin, searchterm);
            string nextpage = null;
            IEnumerable<Bookmark> res = null;

            if (searchres != null)
            {
                res = searchres.Result;
                nextpage = searchres.NextPageUrl;
            }

        }
    }
}