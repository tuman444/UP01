using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace UP01.Pages.Admin
{
    public class PasswordChangeDialog : Window
    {
        private readonly AppUser _user;
        private PasswordBox _pb;

        private static Color FromHex(string hex)
        {
            return (Color)ColorConverter.ConvertFromString(hex);
        }

        private static SolidColorBrush BrushFromHex(string hex)
        {
            return new SolidColorBrush(FromHex(hex));
        }

        public PasswordChangeDialog(AppUser user)
        {
            _user = user;
            Title = $"Смена пароля — {user.DisplayName}";
            Width = 340;
            Height = 220;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;

            
            var bg = new Border
            {
                Background = BrushFromHex("White"),  // фон
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(20)
            };

            var sp = new StackPanel();

            // Заголовок "Новый пароль:"
            sp.Children.Add(new TextBlock
            {
                Text = "Новый пароль:",
                Foreground = BrushFromHex("#2C3E50"),  // текст
                Margin = new Thickness(0, 0, 0, 6)
            });

            // Поле ввода пароля
            _pb = new PasswordBox
            {
                FontWeight = FontWeights.Bold,
                Background = BrushFromHex("White"),  // фон поля
                Foreground = BrushFromHex("Black"),  // текст в поле
                Padding = new Thickness(8),
                BorderThickness = new Thickness(0),
                Margin = new Thickness(0, 0, 0, 12)
            };
            sp.Children.Add(_pb);

            // Кнопка "Сохранить"
            var btn = new Button
            {
                Content = "Сохранить",
                Background = BrushFromHex("#2C3E50"),  // кнопка
                Foreground = BrushFromHex("White"),  // текст
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0, 8, 0, 8),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.Medium
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

            if (newPass.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var u = Core.DB.AppUser.Find(_user.UserId);
            if (u != null)
            {
                u.Password = newPass; 
                Core.DB.SaveChanges();
            }

            MessageBox.Show("Пароль изменён", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }
    }
}