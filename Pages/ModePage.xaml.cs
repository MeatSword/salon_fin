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
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace WpfBeauty.Pages
{
    /// <summary>
    /// Логика взаимодействия для ModePage.xaml
    /// </summary>
    public partial class ModePage : Page
    {
        public ModePage()
        {
            InitializeComponent();
        }
        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            bool isAdmin = TbCode.Text == "0000";
            NavigationService.Navigate(new ServicesGRUD(isAdmin));
        }
    }
}
