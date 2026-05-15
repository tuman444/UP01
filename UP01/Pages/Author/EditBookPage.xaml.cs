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
        private List<GenreViewModel> _genreList = new List<GenreViewModel>();
        public EditBookPage(Book book)
        {
            InitializeComponent();
            _book = book;

            if (_book != null)
            {
                TbPageTitle.Text = "Редактировать книгу";
                TbTitle.Text = _book.Title;
                TbDesc.Text = _book.Description;
                TbContent.Text = _book.TextContent;
            }

            LoadGenres();
        }
        private void LoadGenres()
        {
            var allGenres = Core.DB.Genre.OrderBy(g => g.GenreName).ToList();

            var bookGenreIds = new List<int>();
            if (_book != null)
            {
                var bookWithGenres = Core.DB.Book
                    .Include("Genre")
                    .FirstOrDefault(b => b.BookId == _book.BookId);
                bookGenreIds = bookWithGenres?.Genre.Select(g => g.GenreId).ToList()
                               ?? new List<int>();
            }

            // Формируем список моделей для привязки к ItemsControl
            _genreList = allGenres.Select(g => new GenreViewModel
            {
                GenreId = g.GenreId,
                GenreName = g.GenreName,
                IsSelected = bookGenreIds.Contains(g.GenreId)
            }).ToList();

            IcGenres.ItemsSource = _genreList;
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

            // Получаем выбранные жанры из нашей коллекции данных
            var selectedGenres = _genreList
                .Where(gvm => gvm.IsSelected)
                .Select(gvm => Core.DB.Genre.Find(gvm.GenreId))
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

    // Класс-обертка для управления состоянием чекбокса в UI
    public class GenreViewModel
    {
        public int GenreId { get; set; }
        public string GenreName { get; set; }
        public bool IsSelected { get; set; }
    }
}


