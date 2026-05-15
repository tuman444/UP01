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

namespace UP01.Pages.Catalog
{
    /// <summary>
    /// Логика взаимодействия для BookPage.xaml
    /// </summary>
    public partial class BookPage : Page
    {
        private Book _book;
        // Свойство для биндинга кнопки "Удалить отзыв" в DataTemplate
        public Visibility AdminVisibility =>
            Core.AuthUser?.Role?.RoleName == "Администратор"
                ? Visibility.Visible : Visibility.Collapsed;
        public BookPage(Book book)
        {
            InitializeComponent();
            DataContext = this;
            _book = book;
            LoadBook();
        }
        private void LoadBook()
        {
            _book = Core.DB.Book
                .Include("AppUser")
                .Include("Review")
                .Include("Review.AppUser")
                .Include("Genre")
                .FirstOrDefault(b => b.BookId == _book.BookId);

            if (_book == null) return;

            TbTitle.Text = _book.Title;
            TbAuthor.Text = "Автор: " + (_book.AppUser?.DisplayName ?? "—");
            TbDesc.Text = _book.Description;
            TbContent.Text = _book.TextContent;

            double avg = _book.Review.Any() ? _book.Review.Average(r => r.Rating) : 0;
            TbRating.Text = avg > 0 ? string.Format("★ {0:F1}", avg) : "Нет оценок";

            // Жанры передаются напрямую в ItemsControl
            IcGenres.ItemsSource = _book.Genre.ToList();

            // Обложка
            ImgCover.Source = null;
            if (!string.IsNullOrEmpty(_book.CoverPath))
            {
                try
                {
                    ImgCover.Source = new BitmapImage(new Uri(_book.CoverPath, UriKind.Absolute));
                }
                catch { }
            }

            // Кнопка заморозки книги
            bool isAdmin = Core.AuthUser?.Role?.RoleName == "Администратор";
            BtnFreeze.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            BtnFreeze.Content = _book.IsFrozen ? "🔓 Разморозить книгу" : "🔒 Заморозить книгу";

            // Отзывы
            IcReviews.ItemsSource = _book.Review
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        private void SlRating_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TbSlVal != null)
                TbSlVal.Text = ((int)SlRating.Value).ToString();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new CatalogPage());

        private void BtnToggleRead_Click(object sender, RoutedEventArgs e)
            => ReadPanel.Visibility = ReadPanel.Visibility == Visibility.Visible
                ? Visibility.Collapsed : Visibility.Visible;

        private void BtnAddToList_Click(object sender, RoutedEventArgs e)
        {
            if (Core.AuthUser == null) return;
            string section = (CbSection.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "В планах";

            var entry = Core.DB.ReadingList.FirstOrDefault(rl =>
                rl.UserId == Core.AuthUser.UserId && rl.BookId == _book.BookId);

            if (entry == null)
                Core.DB.ReadingList.Add(new ReadingList
                {
                    UserId = Core.AuthUser.UserId,
                    BookId = _book.BookId,
                    Section = section
                });
            else
                entry.Section = section;

            Core.DB.SaveChanges();
            MessageBox.Show("Книга добавлена в «" + section + "»", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnAddReview_Click(object sender, RoutedEventArgs e)
        {
            if (Core.AuthUser == null) return;

            bool already = Core.DB.Review.Any(r =>
                r.BookId == _book.BookId && r.UserId == Core.AuthUser.UserId);

            if (already)
            {
                MessageBox.Show("Вы уже оставили отзыв на эту книгу",
                    "Ограничение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Core.DB.Review.Add(new Review
            {
                BookId = _book.BookId,
                UserId = Core.AuthUser.UserId,
                Rating = (int)SlRating.Value,
                ReviewText = TbReviewText.Text.Trim(),
                CreatedAt = DateTime.Now
            });
            Core.DB.SaveChanges();
            TbReviewText.Text = "";
            LoadBook();
        }

        private void BtnReportBook_Click(object sender, RoutedEventArgs e)
        {
            if (Core.AuthUser == null) return;
            string reason = TbComplaint.Text.Trim();
            if (string.IsNullOrEmpty(reason)) return;

            Core.DB.Complaint.Add(new Complaint
            {
                UserId = Core.AuthUser.UserId,
                TargetBookId = _book.BookId,
                Reason = reason
            });
            Core.DB.SaveChanges();
            TbComplaint.Text = "";
            MessageBox.Show("Жалоба на книгу отправлена", "Отправлено",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnReportAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (Core.AuthUser == null) return;
            string reason = TbComplaint.Text.Trim();
            if (string.IsNullOrEmpty(reason)) return;

            Core.DB.Complaint.Add(new Complaint
            {
                UserId = Core.AuthUser.UserId,
                TargetBookId = _book.BookId,
                Reason = "Жалоба на автора: " + reason
            });
            Core.DB.SaveChanges();
            TbComplaint.Text = "";
            MessageBox.Show("Жалоба на автора отправлена", "Отправлено",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnFreeze_Click(object sender, RoutedEventArgs e)
        {
            var b = Core.DB.Book.Find(_book.BookId);
            if (b == null) return;
            b.IsFrozen = !b.IsFrozen;
            Core.DB.SaveChanges();
            LoadBook();
        }

        private void BtnReportReview_Click(object sender, RoutedEventArgs e)
        {
            if (Core.AuthUser == null) return;
            var review = (Review)((Button)sender).Tag;

            Core.DB.Complaint.Add(new Complaint
            {
                UserId = Core.AuthUser.UserId,
                TargetReviewId = review.ReviewId,
                Reason = "Жалоба на отзыв"
            });
            Core.DB.SaveChanges();
            MessageBox.Show("Жалоба на отзыв отправлена", "Отправлено",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnFreezeReview_Click(object sender, RoutedEventArgs e)
        {
            var review = (Review)((Button)sender).Tag;
            var r = Core.DB.Review.Find(review.ReviewId);
            if (r == null) return;
            Core.DB.Review.Remove(r);
            Core.DB.SaveChanges();
            LoadBook();
        }
    }
}
