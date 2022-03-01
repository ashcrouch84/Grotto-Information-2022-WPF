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
using System.Windows.Shapes;
using System.Windows.Forms;
using Microsoft.Maps.MapControl.WPF;

namespace Grotto_Information_2022_WPF
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
         
        }

        private void Window_LayoutUpdated(object sender, EventArgs e)
        {

        }

        private void Window_Projector_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Left = Screen.AllScreens[Properties.Settings.Default.giSecondMonitor].WorkingArea.Left;
                this.Top = Screen.AllScreens[Properties.Settings.Default.giSecondMonitor].WorkingArea.Top;
                //this.WindowState=WindowState.Minimized;
                this.WindowStyle = WindowStyle.None;

                double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;

                double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

                this.Width = screenWidth;
                this.Height = screenHeight;

                
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Can't find display, please check settings and try again", "Missing Display", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
            }
        }
    }
}
