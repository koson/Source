using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotSpatial.Controls;
using DotSpatial.Controls.Header;
using System.Windows.Forms;
using DotSpatial.Controls.Docking;
using System.IO;
using System.Reflection;
//using Plugins.HydroModeler;

namespace Plugins.Airnose01
{
    public class Airnose_Plugin : Extension
    {

        private RootItem root = null;
        private SimpleActionItem action = null;
        // plugin info
        private const string _pluginName = "Airnose";
        private const string kAirnoseDock = "kHydroModelerDock";
        private const string KAirnose = "RootAirnose";
        private const string _tablePanelName = "Table";


         

        private Dictionary<string, object> rps_dict = new Dictionary<string, object>();
        private cAirnoseView airnoseViewControl;
        private string kAirnoseView;

        public override void Deactivate()
        {
            // Remove ribbon tab
            App.HeaderControl.RemoveAll();
            // Remove the plugin panel
            // this line ensures that 'enabled' is set to false
            base.Deactivate();
        }
        public override void Activate()
        {
            //root = new RootItem(KAirnose, _pluginName);
            //root.SortOrder = 150;
            //root.Visible = true;
            //App.HeaderControl.Add(root);


            action = new SimpleActionItem(_pluginName, Airnose_Click);
            action.GroupCaption = "";
            action.ToolTipText = "Airnose system";
            action.SmallImage = Properties.Resources.AIRNOSE_Logo.GetThumbnailImage(16, 16, null, IntPtr.Zero);
            action.LargeImage = Properties.Resources.AIRNOSE_Logo;
            action.RootKey = HeaderControl.HomeRootItemKey;
            action.ToggleGroupKey = "Hello Tim test";
            App.HeaderControl.Add(action);
            var rootItem = new RootItem(KAirnose, _pluginName);
            rootItem.Visible = true;
            rootItem.SortOrder = 150; //give it a high sort order to move the button to the right
            App.HeaderControl.Add(rootItem);

            //RefreshTheme
            var refreshThemeButton = new SimpleActionItem("Refresh", rbRefreshTheme_Click)
            {
                RootKey = KAirnose,
                LargeImage = Properties.Resources.open,
                SmallImage = Properties.Resources.open,
                ToolTipText = "Refresh Themes",
                GroupCaption = _tablePanelName,
            };
            App.HeaderControl.Add(refreshThemeButton);


            // Todo: add buttons and handler.
            // Todo: study method to add another controls.

            // add buttons to the ribbon
            rps_dict = BuildRibbonPanel();

            //// Add a dockable panel
            //Add_HM_Panel();

            airnoseViewControl = new cAirnoseView(this) { Dock = DockStyle.Fill };
            var tableViewPanel = new DockablePanel
            {
                Key = kAirnoseView,
                Caption = _tablePanelName,
                InnerControl = airnoseViewControl,
                Dock = DockStyle.Fill,
                DefaultSortOrder = 10
            };
            App.DockManager.Add(tableViewPanel);
            App.DockManager.ActivePanelChanged += DockManager_ActivePanelChanged;


            // activate plugin
            base.Activate();
        }

        private void DockManager_ActivePanelChanged(object sender, DockablePanelEventArgs e)
        {
            if (e.ActivePanelKey == kAirnoseView)
                App.HeaderControl.SelectRoot(kAirnoseView);
        }

        private void rbRefreshTheme_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Refresh");
        }

        private void Airnose_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Airnose");
        }

        private Dictionary<string, object> BuildRibbonPanel()
        {
            List<SimpleActionItem> btns = new List<SimpleActionItem>();
            Dictionary<string, object> rps = new Dictionary<string, object>();

            #region menu panel
            //Open Composition
            var rb = new SimpleActionItem("Open", OpenComp_Click);
            rb.ToolTipText = "Open a saved model configuration";
            rb.LargeImage = Properties.Resources.open.GetThumbnailImage(32, 32, null, IntPtr.Zero);
            rb.SmallImage = Properties.Resources.open.GetThumbnailImage(20, 20, null, IntPtr.Zero);
            rb.GroupCaption = _tablePanelName;
            rb.RootKey = KAirnose;
            App.HeaderControl.Add(rb);
            btns.Add(rb);
            rps.Add("open", rb);

            #endregion 

            return rps;
        }

        private void OpenComp_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Open");
        }
    }
}
