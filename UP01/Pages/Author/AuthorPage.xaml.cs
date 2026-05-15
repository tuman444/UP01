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

            // Опубликованные 
            // Проецируем во вспомогательный класс для удобной привязки рейтинга
            var publishedBooks = books.Where(b => !b.IsFrozen).Select(book =>
            {
                double avg = book.Review.Any() ? book.Review.Average(r => r.Rating) : 0;
                return new BookDisplayModel
                {
                    Book = book,
                    RatingText = avg > 0 ? $"★ {avg:F1}" : "Нет оценок"
                };
            }).ToList();

            IcPublishedBooks.ItemsSource = publishedBooks;

            // Замороженные
            IcFrozen.ItemsSource = books.Where(b => b.IsFrozen).ToList();
        }

        private void BtnEditBook_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Book book)
            {
                NavigationService?.Navigate(new EditBookPage(book));
            }
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

            Core.DB.UnfreezeRequest.Add(new UnfreezeRequest
            {
                UserId = Core.AuthUser.UserId,
                TargetBookId = book.BookId,
                IsAccountUnfreeze = false,
                Reason = "Прошу разморозить книгу",
                RequestDate = DateTime.Now
            });
            Core.DB.SaveChanges();

            MessageBox.Show("Заявка отправлена администратору", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // Вспомогательный класс для отображения опубликованной книги с рейтингом
    public class BookDisplayModel
    {
        public Book Book { get; set; }
        public string RatingText { get; set; }
    }
}

