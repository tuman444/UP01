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

namespace UP01.Pages.Author
{
    /// <summary>
    /// Логика взаимодействия для AuthorPage.xaml
    /// </summary>
    public partial class AuthorPage : Page
    {
        public AuthorPage()
        {
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            if (Core.AuthUser == null) return;
 
            var books = Core.DB.Book
                .Include("Review")
                .Where(b => b.AuthorId == Core.AuthUser.UserId)
                .ToList();
 
            // Опубликованные (не замороженные)
            WpBooks.Children.Clear();
            foreach (var book in books.Where(b => !b.IsFrozen))
                WpBooks.Children.Add(BuildCard(book));
 
            // Замороженные
            IcFrozen.ItemsSource = books.Where(b => b.IsFrozen).ToList();
        }
 
        private UIElement BuildCard(Book book)
        {
            double avg = book.Review.Any() ? book.Review.Average(r => r.Rating) : 0;
 
            var card = new Border
            {
                Width        = 150,
                Margin       = new Thickness(6),
                Background   = new SolidColorBrush(Color.FromRgb(49, 50, 68)),
                CornerRadius = new CornerRadius(10)
            };
 
            var sp = new StackPanel { Margin = new Thickness(10) };
 
            sp.Children.Add(new TextBlock
            {
                Text         = book.Title,
                Foreground   = Brushes.White,
                FontWeight   = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap
            });
 
            sp.Children.Add(new TextBlock
            {
                Text       = avg > 0 ? string.Format("★ {0:F1}", avg) : "Нет оценок",
                Foreground = new SolidColorBrush(Color.FromRgb(249, 226, 175)),
                FontSize   = 11,
                Margin     = new Thickness(0, 4, 0, 6)
            });
 
            var btnEdit = new Button
            {
                Content         = "✏️ Редактировать",
                Background      = new SolidColorBrush(Color.FromRgb(137, 180, 250)),
                Foreground      = new SolidColorBrush(Color.FromRgb(30, 30, 46)),
                BorderThickness = new Thickness(0),
                Padding         = new Thickness(0, 5, 0, 5),
                Cursor          = System.Windows.Input.Cursors.Hand,
                Tag             = book
            };
            btnEdit.Click += (s, e) =>
                NavigationService?.Navigate(new EditBookPage((Book)((Button)s).Tag));
 
            sp.Children.Add(btnEdit);
            card.Child = sp;
            return card;
        }
 
        private void BtnAddBook_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new EditBookPage(null));
 
        private void BtnUnfreezeBook_Click(object sender, RoutedEventArgs e)
        {
            if (Core.AuthUser == null) return;
            var book = (Book)((Button)sender).Tag;
 
            bool already = Core.DB.UnfreezeRequest.Any(r =>
                r.UserId == Core.AuthUser.UserId && r.TargetBookId == book.BookId);
 
            if (already)
            {
                MessageBox.Show("Заявка уже подана", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
 
            // UnfreezeRequest: RequestId, UserId, TargetBookId, IsAccountUnfreeze, Reason, RequestDate
            Core.DB.UnfreezeRequest.Add(new UnfreezeRequest
            {
                UserId           = Core.AuthUser.UserId,
                TargetBookId     = book.BookId,
                IsAccountUnfreeze = false,
                Reason           = "Прошу разморозить книгу",
                RequestDate      = System.DateTime.Now
            });
            Core.DB.SaveChanges();
 
            MessageBox.Show("Заявка отправлена администратору", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
