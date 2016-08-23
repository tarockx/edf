using libEraDeiFessi;
using libEraDeiFessi.Plugins;
using System;
using System.Collections.Generic;
using System.Text;
using ToastIOS;
using UIKit;

namespace xEDF.iOS.Helpers
{
    public class ControllerPusher
    {
        public static UIStoryboard Storyboard { get; set; }
        public static UINavigationController Navigator { get; set; }

        public static void Push(NestedContent result, string title, IEDFPlugin plugin)
        {
            var blocks = result.GetNestBlocksWithDirectContent();

            if (blocks == null || blocks.Count == 0)
            {
                Toast.MakeText("Caricamento fallito: plugin obsoleto o errore di rete", Toast.LENGTH_LONG).Show(ToastType.Error);
                return;
            }

            if (blocks.Count > 1)
            {
                //More than one season/container. Let user chose
                UIViewController controller = Storyboard.InstantiateViewController("NestBlocksController");
                ViewController_NestBlocks nestBlocksController = controller as ViewController_NestBlocks;
                if (nestBlocksController != null)
                {
                    nestBlocksController.Blocks = blocks;
                    nestBlocksController.Plugin = plugin as IEDFContentProvider;
                    nestBlocksController.ContentTitle = title;
                    Navigator.PushViewController(nestBlocksController, true);
                }
            }
            else
            {
                Push(blocks[0]);
            }
        }

        public static void Push(NestBlock parentBlock)
        {
            if (parentBlock == null || parentBlock.Children.Count == 0)
            {
                Toast.MakeText("Nessun episodio/file da mostrare!", Toast.LENGTH_LONG).Show(ToastType.Error);
                return;
            }


            if (parentBlock.Children.Count > 1)
            {
                //More than one episode/link, let user chose
                UIViewController controller = Storyboard.InstantiateViewController("ContentNestBlockController");
                ViewController_ContentNestBlock contentNestBlockController = controller as ViewController_ContentNestBlock;
                if (contentNestBlockController != null)
                {
                    contentNestBlockController.ParentBlock = parentBlock;
                    Navigator.PushViewController(contentNestBlockController, true);
                }
            }
            else
            {
                //only one episode/link, load link list directly
                Push(parentBlock.Children[0] as ContentNestBlock);
            }

        }

        public static void Push(ContentNestBlock parentBlock)
        {
            if (parentBlock == null || parentBlock.Links.Count == 0)
            {
                Toast.MakeText("Nessun link disponibile!", Toast.LENGTH_LONG).Show(ToastType.Error);
                return;
            }

            if (parentBlock.Links.Count > 1)
            {
                //Load link list
                UIViewController controller = Storyboard.InstantiateViewController("LinksController");
                ViewController_Links linksController = controller as ViewController_Links;
                if (linksController != null)
                {
                    linksController.ParentBlock = parentBlock;
                    Navigator.PushViewController(linksController, true);
                }
            }
            else
            {
                //Directly load link widget
                Link link = parentBlock.Links[0];
                Push(link);
            }
        }

        public static void Push(Link link)
        {
            //Load link manager
            UIViewController controller = Storyboard.InstantiateViewController("LinkManagerController");
            ViewController_LinkManager linkManagerController = controller as ViewController_LinkManager;
            if (linkManagerController != null)
            {
                linkManagerController.Link = link;
                Navigator.PushViewController(linkManagerController, true);
            }
        }

        public static void Push(HtmlContent content, string title)
        {
            //Load link manager
            UIViewController controller = Storyboard.InstantiateViewController("HtmlContentController");
            ViewController_HtmlContent htmlContentController = controller as ViewController_HtmlContent;
            if (htmlContentController != null)
            {
                htmlContentController.Content = content;
                htmlContentController.ContentTitle = title;
                Navigator.PushViewController(htmlContentController, true);
            }
        }

        public static void Push(TorrentContent content)
        {
            //Load link manager
            UIViewController controller = Storyboard.InstantiateViewController("LinkManagerController");
            ViewController_LinkManager linkManagerController = controller as ViewController_LinkManager;
            if (linkManagerController != null)
            {
                linkManagerController.Link = new Link(content.Title, content.MagnetURI);
                Navigator.PushViewController(linkManagerController, true);
            }
        }
    }
}
