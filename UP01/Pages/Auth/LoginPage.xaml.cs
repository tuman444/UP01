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
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TbLogin.Text.Trim();
            string password = PbPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ShowError("Заполните все поля");
                return;
            }

            var user = Core.DB.AppUser
                .Include("Role")
                .FirstOrDefault(u => u.Login == login && u.Password == password);

            if (user == null)
            {
                ShowError("Неверный логин или пароль");
                return;
            }

            Core.AuthUser = user;

            var main = new MainWindow();
            main.Show();
            Window.GetWindow(this).Close();
        }

        private void BtnGoRegister_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegisterPage());
        }

        private void ShowError(string msg)
        {
            TbError.Text = msg;
            TbError.Visibility = Visibility.Visible;
        }
    }
}
