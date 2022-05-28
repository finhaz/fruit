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
        public DataAnal()
        {
            InitializeComponent();
        }


        private void btRUN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btSTOP_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Page_Loaded(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            dt = DB_Access.GetDBTable("PARAMETER_RUN");
            datashow.ItemsSource = dt.DefaultView;

            dt = DB_Access.GetDBTable("PARAMETER_SET");
            dataset.ItemsSource = dt.DefaultView;

            dt = DB_Access.GetDBTable("PARAMETER_FACTOR");
            datafactor.ItemsSource = dt.DefaultView;
        }
    }
}
