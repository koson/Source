using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics.Contracts;

namespace Plugins.Airnose01
{
    public partial class cAirnoseView : UserControl
    {
        private Airnose_Plugin _parent;
        public cAirnoseView()
        {
            InitializeComponent();
        }


        public cAirnoseView(Airnose_Plugin parent)
        {
            _parent = parent;
            if (parent == null) throw new ArgumentNullException("parent");
            Contract.EndContractBlock();

            InitializeComponent();

            //dataGridViewNavigator1.PageChanged += dataGridViewNavigator1_PageChanged;

            //_parent.SeriesControl.SeriesCheck += seriesSelector_Refreshed;
            //_parent.SeriesControl.Refreshed += seriesSelector_Refreshed;
            //_parent.IsPanelActiveChanged += OnIsPanelActiveChanged;

            Disposed += OnDisposed;
        }

        private void OnDisposed(object sender, EventArgs eventArgs)
        {
            //_parent.SeriesControl.SeriesCheck -= seriesSelector_Refreshed;
            //_parent.SeriesControl.Refreshed -= seriesSelector_Refreshed;
            //_parent.IsPanelActiveChanged -= OnIsPanelActiveChanged;
        }
    }
}
