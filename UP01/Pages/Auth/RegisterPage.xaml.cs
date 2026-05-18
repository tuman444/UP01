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

namespace UP01.Pages.Auth
{
    /// <summary>
    /// Логика взаимодействия для RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string displayName = TbDisplayName.Text.Trim();
            string login = TbLogin.Text.Trim();
            string email = TbEmail.Text.Trim();
            string password = PbPassword.Password;

            if (string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(login) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Заполните все поля");
                return;
            }

            if (Core.DB.AppUser.Any(u => u.Login == login))
            {
                ShowError("Логин уже занят");
                return;
            }

            var readerRole = Core.DB.Role.FirstOrDefault(r => r.RoleName == "Читатель");
            if (readerRole == null)
            {
                ShowError("Роль 'Читатель' не найдена в БД");
                return;
            }

            var newUser = new AppUser
            {
                DisplayName = displayName,
                Login = login,
                Email = email,
                Password = password,
                RoleId = readerRole.RoleId,
                IsFrozen = false
            };

            Core.DB.AppUser.Add(newUser);
            Core.DB.SaveChanges();

            Core.AuthUser = Core.DB.AppUser
                .Include("Role")
                .FirstOrDefault(u => u.Login == login);

            var main = new MainWindow();
            main.Show();
            Window.GetWindow(this)?.Close();
        }

        private void BtnGoLogin_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new LoginPage());

        private void ShowError(string msg)
        {
            TbError.Text = msg;
            TbError.Visibility = Visibility.Visible;
        }
    }
}
