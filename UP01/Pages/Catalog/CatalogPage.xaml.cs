using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Логика взаимодействия для CatalogPage.xaml
    /// </summary>
    public partial class CatalogPage : Page
    {
        private List<BookViewModel> _displayBooks = new List<BookViewModel>();
        private string _sortMode = "Name";
        public CatalogPage()
        {
            InitializeComponent();
            LoadGenres();
            LoadBooks();
        }
        private void LoadGenres()
        {
            var genres = Core.DB.Genre.OrderBy(g => g.GenreName).ToList();
            LbGenres.Items.Clear();
            LbGenres.Items.Add(new Genre { GenreId = 0, GenreName = "Все" });
            foreach (var g in genres)
                LbGenres.Items.Add(g);
            LbGenres.SelectedIndex = 0;
        }

        private void LoadBooks()
        {
            string search = TbSearch?.Text?.Trim() ?? "";
            int genreId = 0;

            if (LbGenres.SelectedItem is Genre sg)
                genreId = sg.GenreId;

            var query = Core.DB.Book
                .Include("AppUser")
                .Include("Review")
                .Include("Genre")
                .Where(b => !b.IsFrozen);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(b =>
                    b.Title.Contains(search) ||
                    b.AppUser.DisplayName.Contains(search));

            if (genreId > 0)
                query = query.Where(b => b.Genre.Any(g => g.GenreId == genreId));

            // Преобразуем данные во ViewModel 
            _displayBooks = query.ToList().Select(b => new BookViewModel
            {
                BookData = b,
                Title = b.Title,
                CoverPath = string.IsNullOrWhiteSpace(b.CoverPath) ? null : b.CoverPath,
                AppUser = b.AppUser,
                AverageRating = b.Review.Any() ? b.Review.Average(r => r.Rating) : 0
            }).ToList();

            // Сортировка
            if (_sortMode == "Rating")
                _displayBooks = _displayBooks.OrderByDescending(b => b.AverageRating).ToList();
            else
                _displayBooks = _displayBooks.OrderBy(b => b.Title).ToList();

            // Отправляем список в ItemsControl
            IcBooks.ItemsSource = _displayBooks;
        }

        // Клик по самой карточке книги переход на страницу книги
        private void BookCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is BookViewModel clickedBook)
            {
                NavigationService?.Navigate(new BookPage(clickedBook.BookData));
            }
        }

        private void BtnAddToList_Click(object sender, RoutedEventArgs e)
        {
            if (Core.AuthUser == null) return;

            var button = sender as Button;
            if (button?.Tag is BookViewModel bookVm)
            {
                var book = bookVm.BookData;

                bool exists = Core.DB.ReadingList.Any(rl =>
                    rl.UserId == Core.AuthUser.UserId && rl.BookId == book.BookId);

                if (!exists)
                {
                    Core.DB.ReadingList.Add(new ReadingList
                    {
                        UserId = Core.AuthUser.UserId,
                        BookId = book.BookId,
                        Section = "В планах"
                    });
                    Core.DB.SaveChanges();
                    MessageBox.Show("Добавлено в «В планах»", "Готово",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Книга уже есть в вашем списке", "Уже добавлено",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            // Важно, чтобы клик по кнопке внутри карточки не вызывал клик по самой карточке
            e.Handled = true;
        }

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadBooks();
        private void LbGenres_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadBooks();

        private void BtnSortName_Click(object sender, RoutedEventArgs e)
        { _sortMode = "Name"; LoadBooks(); }

        private void BtnSortRating_Click(object sender, RoutedEventArgs e)
        { _sortMode = "Rating"; LoadBooks(); }
    }

    public class BookViewModel
    {
        public Book BookData { get; set; }
        public string Title { get; set; }
        public string CoverPath { get; set; }
        public AppUser AppUser { get; set; }
        public double AverageRating { get; set; }
        public string DisplayRating => AverageRating > 0 ? string.Format("★ {0:F1}", AverageRating) : "—";
    }
}
