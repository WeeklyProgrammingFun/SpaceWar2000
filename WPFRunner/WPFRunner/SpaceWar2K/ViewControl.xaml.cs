using System;
using System.Collections.Generic;
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

namespace WPFRunner.SpaceWar2K
{
    /// <summary>
    /// Interaction logic for ViewControl.xaml
    /// </summary>
    public partial class ViewControl : UserControl
    {
        public ViewControl()
        {
            InitializeComponent();
        }

        private void OnScreenSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataContext is ViewModel vm)
                vm.ViewSize(e.NewSize);

        }
        private void HistorySizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataContext is ViewModel vm)
                vm.HistorySize(e.NewSize);

        }
    }
}
