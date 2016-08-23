using Android.Content;
using Android.Widget;
using libEraDeiFessi;
using libEraDeiFessi.Plugins;
using System;
using System.Collections.Generic;
using System.Text;

namespace xEDF.Droid.Helpers
{
    public class ActivityPusher
    {

        public static void Push(Context context, NestedContent result, string title)
        {
            var blocks = result.GetNestBlocksWithDirectContent();

            if (blocks == null || blocks.Count == 0)
            {
                Toast.MakeText(context, "Caricamento fallito: plugin obsoleto o errore di rete", ToastLength.Long).Show();
                return;
            }

            if (blocks.Count > 1 || blocks.Count == 1 && blocks[0].Children.Count > 1)
            {
                Activity_NestedContent.content = result;
                Activity_NestedContent.title = title;
                Intent i = new Intent(context, typeof(Activity_NestedContent));
                context.StartActivity(i);
            }
            else
            {
                Push(context, blocks[0].Children[0] as ContentNestBlock);
            }
        }

        public static void Push(Context context, ContentNestBlock parentBlock)
        {
            if (parentBlock == null || parentBlock.Links.Count == 0)
            {
                Toast.MakeText(context, "Nessun link disponibile!", ToastLength.Long).Show();
                return;
            }

            if (parentBlock.Links.Count > 1)
            {
                //Load link list
                Activity_Links.Links = parentBlock.Links;
                Intent i = new Intent(context, typeof(Activity_Links));
                context.StartActivity(i);
            }
            else
            {
                //Directly load link widget
                Link link = parentBlock.Links[0];
                Push(context, link);
            }
        }

        public static void Push(Context context, HtmlContent content, string title)
        {
            Activity_HtmlContent.Content = content;
            Activity_HtmlContent.BookmarkTitle = title;
            Intent i = new Intent(context, typeof(Activity_HtmlContent));
            context.StartActivity(i);
        }

        public static void Push(Context context, Link link)
        {
            Activity_LinkManager.Link = link;
            Intent i = new Intent(context, typeof(Activity_LinkManager));
            context.StartActivity(i);
        }

        public static void Push(Context context, TorrentContent content)
        {
            Activity_LinkManager.Link = new Link(content.Title, content.MagnetURI);
            Intent i = new Intent(context, typeof(Activity_LinkManager));
            context.StartActivity(i);
        }
    }
}
