using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
using Path = System.IO.Path;

namespace WpfBeauty.Pages
{
    /// <summary>
    /// Логика взаимодействия для ServiceForm.xaml
    /// </summary>
    public partial class ServiceForm : Window
    {
        private Service _service;
        private ServicesGRUD _servicesGRUD;
        private List<Service> _services = Helper.GetContext().Service.ToList();
        private ICollection<ServicePhoto> _servicesPhoto;
        private string newMainPhotoPath;
        private string destinationDirectory = @"C:\Users\User\Downloads\SalonFinal\beauty-main\Услуги салона красоты";
        public ServiceForm(ServicesGRUD servicesGRUD, Service service)
        {
            InitializeComponent();
            _service = service;
            _servicesGRUD = servicesGRUD;

            if (_service != null)
            {
                _servicesPhoto = _service.ServicePhoto;
                PanelID.Visibility = Visibility.Visible;
                LoadClient();
            }
        }
        private void LoadClient()
        {
            try
            {
                TBID.Text = _service.ID.ToString();
                TBTitle.Text = _service.Title;
                TBCost.Text = decimal.Truncate(_service.Cost).ToString();
                TBTime.Text = _service.DurationInSeconds.ToString(); // вывод в секундах
                TBDescription.Text = _service.Description;
                TBDiscount.Text = $"{_service.Discount * 100}";
                LBPhoto.ItemsSource = _servicesPhoto;

                // Загрузка основной фотографии
                if (!string.IsNullOrEmpty(_service.MainImagePath))
                {
                    try
                    {
                        string imagePath = System.IO.Path.Combine(_service.MainImagePath);
                        PhotoMainPreview.Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
                        newMainPhotoPath = imagePath;
                    }
                    catch (UriFormatException ex)
                    {
                        MessageBox.Show("Ошибка в пути к изображению: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }

        }
        private void ButtonSelectMainImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png|All Files|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                PhotoMainPreview.Source = new BitmapImage(new Uri(openFileDialog.FileName));

                string filePath = openFileDialog.FileName;
                string fileName = System.IO.Path.GetFileName(filePath);

                CopyFileToProjectDirectory(filePath, destinationDirectory);

                newMainPhotoPath = Path.Combine(destinationDirectory, fileName);
            }
        }

        void UpdatePhotoListBox()
        {
            LBPhoto.ItemsSource = null; // Обнуляем источник данных
            LBPhoto.ItemsSource = _servicesPhoto;
        }
        private void CopyFileToProjectDirectory(string sourceFilePath, string destinationDirectory)
        {
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            string fileName = System.IO.Path.GetFileName(sourceFilePath);
            string destinationFilePath = System.IO.Path.Combine(destinationDirectory, fileName);

            try
            {
                File.Copy(sourceFilePath, destinationFilePath, true);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка копирования файла: {ex.Message}");
                
            }
        }
        private void BtnAddPhoto_Click(object sender, RoutedEventArgs e)
        {
            if (_service != null)
            {
                try
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog
                    {
                        Filter = "Image Files|*.jpg;*.jpeg;*.png|All Files|*.*"
                    };

                    if (openFileDialog.ShowDialog() == true)
                    {
                        string filePath = openFileDialog.FileName;

                        // Копируем файл в директорию проекта
                        CopyFileToProjectDirectory(filePath, destinationDirectory);

                        string fileName = Path.GetFileName(filePath);
                        string photoPath = Path.Combine(destinationDirectory, fileName);

                        // Создаем экземпляр ServicePhoto и добавляем его в коллекцию
                        ServicePhoto servicePhoto = new ServicePhoto
                        {
                            PhotoPath = photoPath
                        };

                        _service.ServicePhoto.Add(servicePhoto);
                        UpdatePhotoListBox(); // Обновляем отображение фотографий
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding photo: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Услуга еще не создана, создайте услугу и повторите", "Предупреждение.");
            }
        }
        private void BtnRemovePhoto_Click(object sender, RoutedEventArgs e)
        {
            if (_service != null && _service.ServicePhoto != null)
            {
                try
                {
                    if (LBPhoto.SelectedItem is ServicePhoto selectedPhoto)
                    {
                        var photoToRemove = _service.ServicePhoto.FirstOrDefault(s => s.PhotoPath == selectedPhoto.PhotoPath);
                            // Подтверждение удаления
                            MessageBoxResult result = MessageBox.Show($"Вы уверены, что хотите удалить фото {photoToRemove.PhotoPath} у услуги {_service.Title}?", "Подтверждение", MessageBoxButton.YesNo);
                            if (result == MessageBoxResult.Yes)
                            {
                                // Удаление фотографии из базы данных
                                Helper.GetContext().ServicePhoto.Remove(photoToRemove);

                                // Удаление фотографии из коллекции
                                _service.ServicePhoto.Remove(photoToRemove);

                                UpdatePhotoListBox(); // Обновляем отображение фотографий
                            }
                            else
                            {
                                MessageBox.Show("Вы передумали");
                            }
                    }
                    else
                    {
                        MessageBox.Show("Выберите фотографию для удаления.", "Предупреждение.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении фотографии: {ex.Message}", "Ошибка.");
                }
            }
            else
            {
                MessageBox.Show("Нет фотографий для удаления.", "Предупреждение.");
            }
        }

        private bool ValidateInput(string title, string duration, string cost, string discount)
        {
            // Проверка уникальности названия услуги
            if (IsServiceNameUnique(title) && _service == null)
            {
                MessageBox.Show("Услуга с таким названием уже существует.", "Ошибка");
                return false;
            }

            // Проверка длительности услуги
            if (!int.TryParse(duration, out int durationInSeconds) || durationInSeconds <= 0 || durationInSeconds > 14400)
            {
                MessageBox.Show("Длительность услуги должна быть в диапазоне от 1 до 14400 секунд (4 часа).", "Ошибка");
                return false;
            }

            // Проверка стоимости
            if (!decimal.TryParse(cost, out decimal costValue) || costValue < 0 || !IsValidCostFormat(cost))
            {
                MessageBox.Show("Стоимость должна быть положительным числом и соответствовать формату 1 000.", "Ошибка");
                return false;
            }

            // Проверка скидки
            if (!string.IsNullOrWhiteSpace(discount))
            {
                if (!int.TryParse(discount, out int discountValue) || discountValue < 0)
                {
                    MessageBox.Show("Скидка должна быть целым положительным числом.", "Ошибка");
                    return false;
                }
            }

            return true;
        }

        private bool IsServiceNameUnique(string title)
        {
            return _services.Any(service => service.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsValidCostFormat(string cost)
        {
            // Проверка формата стоимости
            // return Regex.IsMatch(cost, @"^\d+(\,\d{0})?$");
            return Regex.IsMatch(cost, @"^\d+$");
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput(TBTitle.Text, TBTime.Text, TBCost.Text, TBDiscount.Text))
            {
                try
                {
                    if (_service == null)
                        _service = new Service();

                    LBPhoto.ItemsSource = _servicesPhoto;

                    _service.Title = TBTitle.Text;
                    _service.Cost = decimal.Parse(TBCost.Text);
                    _service.DurationInSeconds = int.Parse(TBTime.Text);
                    _service.Discount = double.Parse(TBDiscount.Text) / 100;
                    _service.MainImagePath = newMainPhotoPath;

                    if (TBDescription.Text == null || TBDescription.Text == "")
                        _service.Description = null;
                    else
                        _service.Description = TBDescription.Text;


                    SaveServiceToDatabase(_service);
                    _servicesGRUD.RefreshServiceList(); // Обновляем список в главном окне
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка.");
                }

                
            }
        }
        void SaveServiceToDatabase(Service _service)
        {
            try
            {
                if (_service.ID > 0)
                {
                    Helper.GetContext().Entry(_service).State = EntityState.Modified;
                    Helper.GetContext().SaveChanges();
                    MessageBox.Show("Обновление информации об услуге завершено");
                }
                else
                {
                    Helper.ent.Service.Add(_service);
                    Helper.ent.SaveChanges();
                    MessageBox.Show("Добавление информации об услуге завершено");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка.");
            }
        }
    }
}
