using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SomeNameSpace;

namespace ocean.UI
{
    /// <summary>
    /// DataAnal.xaml 的交互逻辑
    /// </summary>
    public partial class DataAnal : Page
    {
        public DataTable dtrun { get; set; }
        public DataTable dtset { get; set; }
        public DataTable dtfactor { get; set; }
        int select_index;
        public DataAnal()
        {
            InitializeComponent();
            //this.DataContext = this;
        }


        private void btRUN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btSTOP_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Page_Loaded(object sender, EventArgs e)
        {
            //dt1 = DB_Access.GetDBTable("PARAMETER_RUN");
            //datashow.ItemsSource = CommonRes.dt1.DefaultView;

            //dt2 = DB_Access.GetDBTable("PARAMETER_SET");
            //dataset.ItemsSource = CommonRes.dt2.DefaultView;

            //dt3 = DB_Access.GetDBTable("PARAMETER_FACTOR");
            //datafactor.ItemsSource = CommonRes.dt3.DefaultView;

            dtrun = CommonRes.dt1;

            dtset = CommonRes.dt2;

            dtfactor = CommonRes.dt3;
        }


        private void datashow_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            string newValue = (e.EditingElement as TextBox).Text;
            DB_Access.UpdateDBTable(CommonRes.dt1, "PARAMETER_RUN");
        }

        private void datashow_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            select_index = datashow.SelectedIndex;
            string oldValue = datashow.SelectedCells.ToString();
            
        }
    }
}
