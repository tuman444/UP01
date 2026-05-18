using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace UP01
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Начинаем с окна авторизации
            var loginPage = new Pages.Auth.LoginPage();
            var authWindow = new Window
            {
                Title = "Читанй, пиши и не спеши — Вход",
                Width = 420,
                Height = 520,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Background = System.Windows.Media.Brushes.Transparent
            };
            var frame = new Frame { NavigationUIVisibility = NavigationUIVisibility.Hidden };
            frame.Navigate(loginPage);
            authWindow.Content = frame;
            authWindow.Show();
        }
    }
}
