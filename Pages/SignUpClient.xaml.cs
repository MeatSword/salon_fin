using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfBeauty.Models;

namespace WpfBeauty.Pages
{
    /// <summary>
    /// Логика взаимодействия для SignUpClient.xaml
    /// </summary>
    public partial class SignUpClient : Window
    {
        private Service _service;
        private List<Client> _clients = Helper.GetContext().Client.ToList();

        public SignUpClient(Service service)
        {
            InitializeComponent();
            _service = service;
            LoadData();
        }
        void LoadData()
        {
            TBTitle.Text = _service.Title;
            TBTime.Text = ConvertSecondsToMinutes(_service.DurationInSeconds);
            ClientComboBox.ItemsSource = _clients;
            

        }
        public static string ConvertSecondsToMinutes(int durationInSeconds)
        {
            int minutes = SecondToMinutes(durationInSeconds);
            string minutesText = GetDeclension(minutes, "минута", "минуты", "минут");
            return $"{minutes} {minutesText}";
        }
        public static int SecondToMinutes(int seconds)
        {
            int minutes = seconds / 60;
            return minutes;
        }

        private static string GetDeclension(int number, string one, string twoFour, string five)
        {
            int n = Math.Abs(number) % 100;
            int n1 = n % 10;
            if (n > 10 && n < 20) return five;
            if (n1 > 1 && n1 < 5) return twoFour;
            if (n1 == 1) return one;
            return five;
        }
        private bool ValidateTimeInput(string timeInput)
        {
            // Проверка формата времени (HH:mm)
            if (!Regex.IsMatch(timeInput, @"^([01]?[0-9]|2[0-3]):([0-5][0-9])$"))
            {
                MessageBox.Show("Введенное время должно быть в формате HH:mm.", "Ошибка");
                return false;
            }

            return true;
        }
        private string CalculateEndTime(string startTime, int durationInMinutes)
        {
            if (TimeSpan.TryParse(startTime, out TimeSpan startTimeSpan))
            {
                TimeSpan duration = TimeSpan.FromMinutes(durationInMinutes);
                TimeSpan endTimeSpan = startTimeSpan + duration;

                return endTimeSpan.ToString(@"hh\:mm");
            }

            return string.Empty;
        }
        private void SaveClientService()
        {
            if (DPService.SelectedDate.HasValue && ValidateTimeInput(TBStartTime.Text))
            {
                DateTime selectedDate = DPService.SelectedDate.Value;
                TimeSpan startTime = TimeSpan.Parse(TBStartTime.Text);
                DateTime startDateTime = selectedDate.Date + startTime;

                ClientService clientService = new ClientService
                {
                    ClientID = ((Client)ClientComboBox.SelectedItem).ID,
                    ServiceID = _service.ID,
                    StartTime = startDateTime,
                    Comment = null
                };

                try
                {
                    Helper.ent.ClientService.Add(clientService);
                    Helper.ent.SaveChanges();

                    MessageBox.Show("Запись успешно сохранена.", "Успех");
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка сохранения:" + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите корректные данные.", "Ошибка");
            }
        }

        private void TBStartTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            var time = textBox.Text.ToLower();

            TBEndTime.Text = CalculateEndTime(time, SecondToMinutes(_service.DurationInSeconds));
        }
        private void BtnSignUp_Click(object sender, RoutedEventArgs e)
        {
            SaveClientService();
        }
    }
}
