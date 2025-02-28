using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Threading;
using WpfBeauty.Models;
using System.Data.Entity;

namespace WpfBeauty.Pages
{
    /// <summary>
    /// Логика взаимодействия для Upcoming.xaml
    /// </summary>
    public partial class Upcoming : Window
    {
        List<ClientService> _cservices = new List<ClientService>();
        private DispatcherTimer _timer;

        public Upcoming()
        {
            InitializeComponent();
            InitializeTimer();
            LoadAppointments();
        }

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            LoadAppointments();
        }

        private void LoadAppointments()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var cserviceList = Helper.GetContext().ClientService
                                    .Include(cs => cs.Service)
                                    .Include(cs => cs.Client)
                                    .ToList();
            _cservices.Clear();


            foreach (ClientService cs in cserviceList)
            {
                if (cs.StartTime >= today && cs.StartTime < tomorrow.AddDays(1))
                {
                    if (cs.StartTime > DateTime.Now)
                    {
                        if ((cs.StartTime - DateTime.Now).TotalMinutes < 60)
                            cs.HasTime = true; // если услуга через час
                        else
                            cs.HasTime = false;

                        cs.TimeRemaining = (cs.StartTime - DateTime.Now).ToString(@"hh\:mm");
                    }
                    else
                    {
                        cs.TimeRemaining = "+"; // или null, если время услуги уже прошло
                    }
                    _cservices.Add(cs);
                }
            }
            Grid.ItemsSource = _cservices;
        }
    }

}
