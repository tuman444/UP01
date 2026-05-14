using Microsoft.Win32;
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
using System.IO;

namespace UP01.Pages.Author
{
    /// <summary>
    /// Логика взаимодействия для EditBookPage.xaml
    /// </summary>
    public partial class EditBookPage : Page
    {
        private Book _book;          // null = новая книга
        private string _newCoverPath; // путь к новой обложке
        private List<CheckBox> _genreCbs = new List<CheckBox>();
        public EditBookPage(Book book)
        {
            InitializeComponent();
            _book = book;

            if (_book != null)
            {
                TbPageTitle.Text = "Редактировать книгу";
                TbTitle.Text = _book.Title;
                TbDesc.Text = _book.Description;
                // TextContent — реальное поле
                TbContent.Text = _book.TextContent;
            }

            LoadGenres();
        }
        private void LoadGenres()
        {
            var allGenres = Core.DB.Genre.OrderBy(g => g.GenreName).ToList();

            // Жанры книги — навигационное свойство Genre (Many-to-Many)
            var bookGenreIds = new List<int>();
            if (_book != null)
            {
                var bookWithGenres = Core.DB.Book
                    .Include("Genre")
                    .FirstOrDefault(b => b.BookId == _book.BookId);
                bookGenreIds = bookWithGenres?.Genre.Select(g => g.GenreId).ToList()
                               ?? new List<int>();
            }

            WpGenres.Children.Clear();
            _genreCbs.Clear();

            foreach (var g in allGenres)
            {
                var cb = new CheckBox
                {
                    Content = g.GenreName,
                    Tag = g.GenreId,
                    Foreground = System.Windows.Media.Brushes.White,
                    Margin = new Thickness(4),
                    IsChecked = bookGenreIds.Contains(g.GenreId)
                };
                _genreCbs.Add(cb);
                WpGenres.Children.Add(cb);
            }
        }

        private void BtnPickCover_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Выберите обложку"
            };
            if (dlg.ShowDialog() == true)
            {
                _newCoverPath = dlg.FileName;
                TbCoverName.Text = Path.GetFileName(dlg.FileName);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TbTitle.Text))
            {
                TbError.Text = "Укажите название книги";
                TbError.Visibility = Visibility.Visible;
                return;
            }

            var selectedGenres = _genreCbs
                .Where(cb => cb.IsChecked == true)
                .Select(cb => Core.DB.Genre.Find((int)cb.Tag))
                .Where(g => g != null)
                .ToList();

            if (_book == null)
            {
                // Новая книга
                var newBook = new Book
                {
                    AuthorId = Core.AuthUser.UserId,
                    Title = TbTitle.Text.Trim(),
                    Description = TbDesc.Text.Trim(),
                    TextContent = TbContent.Text.Trim(),
                    CoverPath = _newCoverPath ?? "",
                    IsFrozen = false
                };

                // Добавляем жанры через навигационное свойство
                foreach (var g in selectedGenres)
                    newBook.Genre.Add(g);

                Core.DB.Book.Add(newBook);
                Core.DB.SaveChanges();
            }
            else
            {
                // Редактирование
                var dbBook = Core.DB.Book
                    .Include("Genre")
                    .FirstOrDefault(b => b.BookId == _book.BookId);

                if (dbBook == null) return;

                dbBook.Title = TbTitle.Text.Trim();
                dbBook.Description = TbDesc.Text.Trim();
                dbBook.TextContent = TbContent.Text.Trim();
                if (_newCoverPath != null)
                    dbBook.CoverPath = _newCoverPath;

                // Обновляем жанры
                dbBook.Genre.Clear();
                foreach (var g in selectedGenres)
                    dbBook.Genre.Add(g);

                Core.DB.SaveChanges();
            }

            MessageBox.Show("Книга сохранена!", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationService?.Navigate(new AuthorPage());
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new AuthorPage());
    }
}

