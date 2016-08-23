using Foundation;
using libEraDeiFessi.Plugins;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace xEDF.iOS
{
    class TableSource_Options : UITableViewSource
    {
        List<string> MainOptions = new List<string>() { "Verifica nuova versione" };
        List<string> MainOptionsDesc = new List<string>() { "Verifica disponibilità di versioni aggiornate di iEDF all'avvio dell'app" };

        List<IEDFPlugin> plugins;
        List<string> disabledPlugins;

        List<Tuple<ICollection, string>> Sections;

        public TableSource_Options(List<IEDFPlugin> plugins)
        {
            this.plugins = plugins;
            disabledPlugins = Repo.Settings.DisabledPlugins;

            Sections = new List<Tuple<ICollection, string>>();
            Sections.Add(new Tuple<ICollection, string>(MainOptions, "Opzioni generali"));
            Sections.Add(new Tuple<ICollection, string>(plugins, "Plugin di ricerca abilitati"));
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return Sections[(int)section].Item1.Count;
        }

        public override string TitleForHeader(UITableView tableView, nint section)
        {
            return Sections[(int)section].Item2;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return Sections.Count;
        }

        public override nint SectionFor(UITableView tableView, string title, nint atIndex)
        {
            int count = 0;
            int curSection = 0;
            for (int i = 0; i < Sections.Count; i++)
            {
                count += Sections[i].Item1.Count;
                if (count >= atIndex)
                    break;
                else
                    curSection++;
            }
            return curSection;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            if (indexPath.Section == 0)
            {
                UITableViewCell cell = new UITableViewCell(UITableViewCellStyle.Subtitle, "TableCell");
                cell.TextLabel.Text = MainOptions[indexPath.Row];
                cell.DetailTextLabel.Text = MainOptionsDesc[indexPath.Row];
                cell.DetailTextLabel.TextColor = UIColor.DarkGray;

                switch (indexPath.Row)
                {
                    case 0:
                        UISwitch sw = new UISwitch();
                        sw.On = Repo.Settings.CheckForUpdates;
                        sw.ValueChanged += delegate
                        {
                            Repo.Settings.CheckForUpdates = sw.On;
                        };
                        cell.AccessoryView = sw;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }


                return cell;
            }
            else
            {
                IEDFPlugin p = plugins[indexPath.Row];
                string title = p.pluginName;

                string subtitle = "Autore: " + p.pluginAuthor + ", " + "ID: " + p.pluginID;

                UITableViewCell cell = new UITableViewCell(UITableViewCellStyle.Subtitle, "TableCell");

                cell.TextLabel.Text = title;
                cell.DetailTextLabel.Text = subtitle;
                cell.DetailTextLabel.TextColor = UIColor.DarkGray;

                UISwitch sw = new UISwitch();
                sw.On = !disabledPlugins.Contains(p.pluginID);
                sw.ValueChanged += delegate
                {
                    if (sw.On)
                        disabledPlugins.Remove(p.pluginID);
                    else
                        disabledPlugins.Add(p.pluginID);

                    Repo.Settings.DisabledPlugins = disabledPlugins;
                };
                cell.AccessoryView = sw;

                return cell;
            }

        }

    }
}
