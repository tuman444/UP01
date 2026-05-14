using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace UP01.Pages.Admin
{
    public class PasswordChangeDialog : Window
    {
        private readonly AppUser _user;
        private System.Windows.Controls.PasswordBox _pb;

        public PasswordChangeDialog(AppUser user)
        {
            _user = user;
            Title = $"Смена пароля — {user.DisplayName}";
            Width = 340; Height = 200;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = System.Windows.Media.Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;

            var bg = new System.Windows.Controls.Border
            {
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(49, 50, 68)),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(20)
            };

            var sp = new System.Windows.Controls.StackPanel();

            sp.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = "Новый пароль:",
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 0, 0, 6)
            });

            _pb = new System.Windows.Controls.PasswordBox
            {
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(69, 71, 90)),
                Foreground = System.Windows.Media.Brushes.White,
                Padding = new Thickness(8),
                BorderThickness = new Thickness(0),
                Margin = new Thickness(0, 0, 0, 12)
            };
            sp.Children.Add(_pb);

            var btn = new System.Windows.Controls.Button
            {
                Content = "Сохранить",
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(203, 166, 247)),
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(30, 30, 46)),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0, 8, 0, 8),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            btn.Click += Save_Click;
            sp.Children.Add(btn);

            bg.Child = sp;
            Content = bg;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string newPass = _pb.Password;
            if (string.IsNullOrWhiteSpace(newPass))
            {
                MessageBox.Show("Введите пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var u = Core.DB.AppUser.Find(_user.UserId);
            if (u != null)
            {
                u.Password = newPass; // хешировать при необходимости
                Core.DB.SaveChanges();
            }

            DialogResult = true;
            Close();
        }
    }
}
