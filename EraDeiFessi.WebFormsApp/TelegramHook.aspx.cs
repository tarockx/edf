using EraDeiFessi.WebFormsApp;
using EraDeiFessi.WebFormsApp.Helpers;
using EraDeiFessi.WebFormsApp.Telegram;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telegram.Bot.Types;

namespace EraDeiFessi.WebFormsApp
{
    public partial class TelegramHook : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (!TelegramRepo.HookIsSet)
            //{
            //    TelegramRepo.Client.SetWebhookAsync("http://localhost:1911/TelegramHook.aspx").Wait();
            //    TelegramRepo.HookIsSet = true;
            //}

            //  TelegramRepo.ProcessUpdates();

            try
            {
                string updateStr = Request.GetFromBodyString();
                Update update = Update.FromString(updateStr);
                this.RegisterAsyncTask(new PageAsyncTask(async cancellationToken =>
                {
                    TelegramRepo.ProcessUpdate(update);
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

           
            
        }

    }
}