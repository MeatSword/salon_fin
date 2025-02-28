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
using WpfBeauty.Models;
using WpfBeauty.Pages;

namespace WpfBeauty
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Frame.Navigate(new ModePage());

        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
        private void Frame_LoadCompleted(object sender, NavigationEventArgs e)
        {
            try
            {
                ServicesGRUD pg = (ServicesGRUD)e.Content;
                //pg.displayClient();
            }
            catch { };
        }
        private void Frame_ContentRendered(object sender, EventArgs e)
        {
            if (Frame.CanGoBack)
                BtnBack.Visibility = Visibility.Visible;
            else
                BtnBack.Visibility = Visibility.Hidden;
        }
    }
}