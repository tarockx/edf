using EraDeiFessi.WebFormsApp.Helpers;
using EraDeiFessi.WebFormsApp.Telegram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Telegram.Bot.Types;
using System.Threading.Tasks;

namespace EraDeiFessi.WebFormsApp
{
    /// <summary>
    /// Summary description for TelegramHandler
    /// </summary>
    public class TelegramHandler : HttpTaskAsyncHandler
    {
        

        public async override Task ProcessRequestAsync(HttpContext context)
        {
            try
            {
                string updateStr = context.Request.GetFromBodyString();
                Update update = Update.FromString(updateStr);
                await TelegramRepo.ProcessUpdateAsync(update);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 200;
                context.Response.Write("{}");
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.StackTrace);
            }

            
        }

    }
}