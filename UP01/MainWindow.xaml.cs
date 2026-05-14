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
using UP01.Pages.Admin;
using UP01.Pages.Author;
using UP01.Pages.Catalog;
using UP01.Pages.Lists;
using UP01.Pages.Profile;

namespace UP01
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UpdateSidebar();
            MainFrame.Navigate(new CatalogPage());
        }

        // Вызывается после входа/смены роли
        public void UpdateSidebar()
        {
            var user = Core.AuthUser;
            if (user == null) return;

            string role = user.Role?.RoleName ?? "";

            BtnAuthorSide.Visibility = role == "Автор" ? Visibility.Visible : Visibility.Collapsed;
            BtnAdminSide.Visibility = role == "Администратор" ? Visibility.Visible : Visibility.Collapsed;
            BtnFrozenWarn.Visibility = user.IsFrozen ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnCatalog_Click(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new CatalogPage());

        private void BtnLists_Click(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new ListsPage());

        private void BtnProfile_Click(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new ProfilePage());

        private void BtnAuthor_Click(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new AuthorPage());

        private void BtnAdmin_Click(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new AdminPage());

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            Core.AuthUser = null;
            var login = new Pages.Auth.LoginPage();
            // Открываем окно входа, закрываем главное
            var authWin = new Window
            {
                Title = "JokeAndKing — Вход",
                Width = 420,
                Height = 520,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Background = System.Windows.Media.Brushes.Transparent
            };
            var frame = new System.Windows.Controls.Frame
            {
                NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden,
                Content = login
            };
            authWin.Content = frame;
            authWin.Show();
            this.Close();
        }
    }
}
