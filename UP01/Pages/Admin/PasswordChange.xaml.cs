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

namespace UP01.Pages.Admin
{
    /// <summary>
    /// Логика взаимодействия для PasswordChange.xaml
    /// </summary>
    public partial class PasswordChange : Page
    {
        private readonly AppUser _targetUser;
        public PasswordChange(AppUser user = null)
        {
            InitializeComponent();

            _targetUser = user ?? Core.AuthUser;

            if (_targetUser != null)
            {
                TxtHeader.Text = $"Смена пароля: {_targetUser.DisplayName}";
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string newPass =PbNewPassword.Password;

            if (string.IsNullOrWhiteSpace(newPass))
            {
                MessageBox.Show("Введите новый пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPass.Length < 3)
            {
                MessageBox.Show("Пароль должен содержать не менее 3 символов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (_targetUser == null)
            {
                MessageBox.Show("Пользователь для изменения не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var dbuser = Core.DB.AppUser.Find(_targetUser.UserId);
                if(dbuser != null)
                {
                    dbuser.Password = newPass;
                    Core.DB.SaveChanges();

                    MessageBox.Show("Пароль успешно изменён", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService.GoBack();
                }
            }
            catch(Exception ex) 
            {
                MessageBox.Show($"Ошибка при сохранении в базу данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack(); 
        }
    }
}
