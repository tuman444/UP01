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
    /// Логика взаимодействия для CatalogPage.xaml
    /// </summary>
    public partial class CatalogPage : Page
    {
        private List<Book> _allBooks = new List<Book>();
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

            // Загружаем книги с навигационными свойствами
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

            _allBooks = query.ToList();

            if (_sortMode == "Rating")
                _allBooks = _allBooks
                    .OrderByDescending(b => b.Review.Any()
                        ? b.Review.Average(r => r.Rating) : 0)
                    .ToList();
            else
                _allBooks = _allBooks.OrderBy(b => b.Title).ToList();

            RenderBooks();
        }

        private void RenderBooks()
        {
            WpBooks.Children.Clear();

            foreach (var book in _allBooks)
            {
                double avg = book.Review.Any() ? book.Review.Average(r => r.Rating) : 0;

                var card = new Border
                {
                    Width = 165,
                    Margin = new Thickness(6),
                    Background = new SolidColorBrush(Color.FromRgb(49, 50, 68)),
                    CornerRadius = new CornerRadius(12),
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                var sp = new StackPanel();

                // Обложка
                var imgBorder = new Border
                {
                    Height = 215,
                    CornerRadius = new CornerRadius(12, 12, 0, 0),
                    ClipToBounds = true,
                    Background = new SolidColorBrush(Color.FromRgb(69, 71, 90))
                };

                // CoverPath — путь к файлу (строка), не байты
                if (!string.IsNullOrEmpty(book.CoverPath))
                {
                    try
                    {
                        var bi = new BitmapImage(new System.Uri(book.CoverPath, System.UriKind.Absolute));
                        imgBorder.Child = new Image
                        {
                            Source = bi,
                            Stretch = Stretch.UniformToFill
                        };
                    }
                    catch
                    {
                        imgBorder.Child = MakeBookEmoji();
                    }
                }
                else
                {
                    imgBorder.Child = MakeBookEmoji();
                }

                sp.Children.Add(imgBorder);

                // Информация
                var info = new StackPanel { Margin = new Thickness(10, 8, 10, 10) };

                info.Children.Add(new TextBlock
                {
                    Text = book.Title,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 13,
                    TextWrapping = TextWrapping.Wrap,
                    MaxHeight = 40
                });

                info.Children.Add(new TextBlock
                {
                    Text = book.AppUser?.DisplayName,
                    Foreground = new SolidColorBrush(Color.FromRgb(166, 173, 200)),
                    FontSize = 11,
                    Margin = new Thickness(0, 2, 0, 0)
                });

                info.Children.Add(new TextBlock
                {
                    Text = avg > 0 ? string.Format("★ {0:F1}", avg) : "—",
                    Foreground = new SolidColorBrush(Color.FromRgb(249, 226, 175)),
                    FontSize = 11,
                    Margin = new Thickness(0, 4, 0, 4)
                });

                var btnList = new Button
                {
                    Content = "+ В список",
                    Background = new SolidColorBrush(Color.FromRgb(203, 166, 247)),
                    Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 46)),
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(6, 3, 6, 3),
                    FontSize = 11,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = book
                };
                btnList.Click += BtnAddToList_Click;
                info.Children.Add(btnList);

                sp.Children.Add(info);
                card.Child = sp;

                // Клик по карточке — открыть книгу
                var bookRef = book;
                card.MouseLeftButtonUp += (s, ev) =>
                    NavigationService?.Navigate(new BookPage(bookRef));

                WpBooks.Children.Add(card);
            }
        }

        private static TextBlock MakeBookEmoji() => new TextBlock
        {
            Text = "📖",
            FontSize = 48,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        private void BtnAddToList_Click(object sender, RoutedEventArgs e)
        {
            if (Core.AuthUser == null) return;
            var book = (Book)((Button)sender).Tag;

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

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadBooks();
        private void LbGenres_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadBooks();

        private void BtnSortName_Click(object sender, RoutedEventArgs e)
        { _sortMode = "Name"; LoadBooks(); }

        private void BtnSortRating_Click(object sender, RoutedEventArgs e)
        { _sortMode = "Rating"; LoadBooks(); }
    }
}
