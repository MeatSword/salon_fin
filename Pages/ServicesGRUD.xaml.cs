using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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

namespace WpfBeauty.Pages
{
    public partial class ServicesGRUD : Page
    {
        List<Service> _services = new List<Service>();

        private int servicesFullCount = 0; // всего клиентов
        private int servicesCount = 0; // всего клиентов отображено

        private bool _isAdmin;

        private string servicesSortBy;
        private string serviceFilter;
        private string serviceSearch = "";

        private decimal finalCost;

        public ServicesGRUD(bool isAdmin)
        {
            InitializeComponent();
            if (isAdmin)
                AdminPanel.Visibility = Visibility.Visible;

            _isAdmin = isAdmin;
            RefreshServiceList();
        }
        void LoadServiceGrid()
        {
            _services.Clear();
            var serviceList = Helper.GetContext().Service.ToList();
            servicesFullCount = serviceList.Count;
            foreach (Service service in serviceList)
            {
                service.IsAdmin = _isAdmin;
                service.ServiceTime = ConvertSecondsToHoursAndMinutes(service.DurationInSeconds);
                if (service.Discount.HasValue)
                {
                    service.HasDiscount = true;
                    service.FinalCost = CalculateDiscount(service.Cost, (decimal)service.Discount);
                    service.DiscountCost = $"{service.FinalCost:N2} рублей за";
                    service.DiscountStr = $"* скидка {service.Discount * 100}%";
                } else
                {
                    service.FinalCost = service.Cost;
                    service.DiscountCost = "рублей за";
                }
                _services.Add(service);
            }
        }
        void DisplayServices()
        {
            var servicesFiltrList = _services
                .Where(s =>
                    (serviceFilter == "Все") || (serviceFilter == null) ||
                    (serviceFilter == "от 5% до 15%" && (s.Discount >= 0.05 && s.Discount < 0.15)) ||
                    (serviceFilter == "от 15% до 30%" && (s.Discount >= 0.15 && s.Discount < 0.30)) ||
                    (serviceFilter == "от 30% до 70%" && (s.Discount >= 0.30 && s.Discount < 0.70)) ||
                    (serviceFilter == "от 70% до 100%" && (s.Discount >= 0.70 && s.Discount < 1)))
                .Where(s =>
                    (s.Title.ToLower().Contains(serviceSearch)) ||
                    (s.Description != null && s.Description.ToLower().Contains(serviceSearch)))
                .ToList();
            // сортировка
            if (!string.IsNullOrEmpty(servicesSortBy))
                servicesFiltrList = SortServices(servicesFiltrList, servicesSortBy);

            servicesCount = servicesFiltrList.Count();
            ServicesGrid.ItemsSource = servicesFiltrList;

            TBCountServices.Text = $"{servicesCount} из {servicesFullCount}";

        }
        public void RefreshServiceList()
        {
            LoadServiceGrid();
            DisplayServices();
        }
        private List<Service> SortServices(List<Service> services, string sortBy)
        {
            switch (sortBy)
            {
                case "По возрастанию":
                    return services.OrderBy(s => s.FinalCost).ToList();
                case "По убыванию":
                    return services.OrderByDescending(s => s.FinalCost).ToList();
                default:
                    return services;
            }
        }
        public static decimal CalculateDiscount(decimal originalPrice, decimal discountPercentage)
        {
            decimal discountAmount = originalPrice * discountPercentage;
            decimal finalPrice = originalPrice - discountAmount;
            return finalPrice;
        }
        public static string ConvertSecondsToHoursAndMinutes(int durationInSeconds)
        {
            int hours = durationInSeconds / 3600;
            int minutes = (durationInSeconds % 3600) / 60;

            string hoursText = "";
            if (hours > 0)
            {
                if (hours % 10 == 1 && hours % 100 != 11)
                    hoursText = $"{hours} час";
                else if (hours % 10 >= 2 && hours % 10 <= 4 && (hours % 100 < 10 || hours % 100 >= 20))
                    hoursText = $"{hours} часа";
                else
                    hoursText = $"{hours} часов";
            }

            string minutesText = "";
            if (minutes > 0) 
            { 
                if (minutes % 10 == 1 && minutes % 100 != 11)
                    minutesText = $"{minutes} минута";
                else if (minutes % 10 >= 2 && minutes % 10 <= 4 && (minutes % 100 < 10 || minutes % 100 >= 20))
                    minutesText = $"{minutes} минуты";
                else
                    minutesText = $"{minutes} минут";
            }

            if (hours > 0 && minutes > 0)
                return $"{hoursText} {minutesText}";
            else if (hours > 0)
                return hoursText;
            else
                return minutesText;
        }
        private void DeleteFromDatabase(Service service)
        {
            try
            {
                foreach (var photo in service.ServicePhoto.ToList())
                {
                    service.ServicePhoto.Remove(photo);
                }
                Helper.GetContext().Service.Remove(service);
                Helper.GetContext().SaveChanges();
                MessageBox.Show("Удаление информации об услуге завершено!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting service: {ex.Message}");
            }
        }
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            Service selected = ServicesGrid.SelectedItem as Service;
            if (selected == null)
            {
                MessageBox.Show("Выберите услугу для изменения");
                return;
            }
            ServiceForm dlg = new ServiceForm(this, selected);
            dlg.ShowDialog();
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, выбрана ли услуга
            Service selected = ServicesGrid.SelectedItem as Service;
            if (selected == null)
            {
                MessageBox.Show("Выберите сервис для удаления");
                return;
            }

            // Проверяем, есть ли у услуги записи
            if (selected.ClientService.Count > 0)
            {
                MessageBox.Show("Невозможно удалить услугу с записями");
                return;
            }

            // Подтверждение удаления
            MessageBoxResult result = MessageBox.Show($"Вы уверены, что хотите удалить услугу {selected.Title}?", "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                // Удаляем клиента из базы данных
                DeleteFromDatabase(selected);

                // Обновляем список клиентов
                RefreshServiceList();
            }
        }
        private void CBSortBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            ComboBoxItem selectedItem = (ComboBoxItem)comboBox.SelectedItem;
            var selectedSort = selectedItem.Content.ToString();
            servicesSortBy = selectedSort;
            DisplayServices();
        }
        private void CBFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            ComboBoxItem selectedItem = (ComboBoxItem)comboBox.SelectedItem;
            var selectedGender = selectedItem.Content.ToString();
            serviceFilter = selectedGender;
            DisplayServices();
        }
        private void TBSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            serviceSearch = textBox.Text.ToLower();
            DisplayServices();
        }

        private void BtnAddService_Click(object sender, RoutedEventArgs e)
        {
            ServiceForm dlg = new ServiceForm(this, null);
            dlg.ShowDialog();
        }

        private void BtnSignUpClient_Click(object sender, RoutedEventArgs e)
        {
            Service selected = ServicesGrid.SelectedItem as Service;
            if (selected == null)
            {
                MessageBox.Show("Выберите услугу для записи");
                return;
            }
            SignUpClient dlg = new SignUpClient(selected);
            dlg.ShowDialog();
        }

        private void BtnUpcoming_Click(object sender, RoutedEventArgs e)
        {
            Upcoming dlg = new Upcoming();
            dlg.ShowDialog();
        }
    }
}
