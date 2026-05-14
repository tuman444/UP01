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

namespace UP01.Pages.Lists
{
    /// <summary>
    /// Логика взаимодействия для ListsPage.xaml
    /// </summary>
    public partial class ListsPage : Page
    {
        private string _activeSection = "Читаю";
        private string _sortMode = "Name";
        public ListsPage()
        {
            InitializeComponent();
            LoadBooks();

        }
        private void LoadBooks()
        {
            if (Core.AuthUser == null) return;

            string search = TbSearch?.Text?.Trim() ?? "";

            var query = Core.DB.ReadingList
                .Include("Book")
                .Include("Book.AppUser")
                .Include("Book.Review")
                .Include("Book.Genre")
                .Where(rl => rl.UserId == Core.AuthUser.UserId
                          && rl.Section == _activeSection);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(rl =>
                    rl.Book.Title.Contains(search) ||
                    rl.Book.AppUser.DisplayName.Contains(search));

            var items = query.ToList();

            if (_sortMode == "Rating")
                items = items.OrderByDescending(rl =>
                    rl.Book.Review.Any() ? rl.Book.Review.Average(r => r.Rating) : 0).ToList();
            else
                items = items.OrderBy(rl => rl.Book.Title).ToList();

            WpBooks.Children.Clear();
            foreach (var rl in items)
                WpBooks.Children.Add(BuildCard(rl));
        }

        private UIElement BuildCard(ReadingList rl)
        {
            var book = rl.Book;
            double avg = book.Review.Any() ? book.Review.Average(r => r.Rating) : 0;

            var card = new Border
            {
                Width = 165,
                Margin = new Thickness(6),
                Background = new SolidColorBrush(Color.FromRgb(49, 50, 68)),
                CornerRadius = new CornerRadius(12)
            };

            var sp = new StackPanel();

            // Обложка — CoverPath (строка)
            var imgBorder = new Border
            {
                Height = 200,
                CornerRadius = new CornerRadius(12, 12, 0, 0),
                ClipToBounds = true,
                Background = new SolidColorBrush(Color.FromRgb(69, 71, 90))
            };

            if (!string.IsNullOrEmpty(book.CoverPath))
            {
                try
                {
                    var bi = new BitmapImage(new System.Uri(book.CoverPath, System.UriKind.Absolute));
                    imgBorder.Child = new Image { Source = bi, Stretch = Stretch.UniformToFill };
                }
                catch
                {
                    imgBorder.Child = MakeEmoji();
                }
            }
            else
            {
                imgBorder.Child = MakeEmoji();
            }

            sp.Children.Add(imgBorder);

            var info = new StackPanel { Margin = new Thickness(8, 6, 8, 8) };

            info.Children.Add(new TextBlock
            {
                Text = book.Title,
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold,
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                MaxHeight = 36
            });

            info.Children.Add(new TextBlock
            {
                Text = avg > 0 ? string.Format("★ {0:F1}", avg) : "—",
                Foreground = new SolidColorBrush(Color.FromRgb(249, 226, 175)),
                FontSize = 11,
                Margin = new Thickness(0, 2, 0, 4)
            });

            // Комбобокс для перемещения
            var cb = new ComboBox
            {
                Background = new SolidColorBrush(Color.FromRgb(69, 71, 90)),
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 4),
                FontSize = 11
            };
            foreach (var s in new[] { "Читаю", "Прочитано", "В планах", "Заброшено" })
                cb.Items.Add(new ComboBoxItem { Content = s, IsSelected = s == _activeSection });

            var btnMove = new Button
            {
                Content = "Переместить",
                Background = new SolidColorBrush(Color.FromRgb(137, 180, 250)),
                Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 46)),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0, 4, 0, 4),
                Cursor = System.Windows.Input.Cursors.Hand,
                FontSize = 11,
                Tag = new object[] { rl, cb }
            };

            btnMove.Click += (s, e) =>
            {
                var arr = (object[])((Button)s).Tag;
                var entry = (ReadingList)arr[0];
                var combo = (ComboBox)arr[1];
                string target = (combo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "В планах";

                var dbEntry = Core.DB.ReadingList.FirstOrDefault(r =>
                    r.UserId == entry.UserId && r.BookId == entry.BookId);

                if (dbEntry != null)
                {
                    dbEntry.Section = target;
                    Core.DB.SaveChanges();
                    LoadBooks();
                }
            };

            info.Children.Add(cb);
            info.Children.Add(btnMove);
            sp.Children.Add(info);
            card.Child = sp;
            return card;
        }

        private static TextBlock MakeEmoji() => new TextBlock
        {
            Text = "📖",
            FontSize = 40,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        private void Tab_Click(object sender, RoutedEventArgs e)
        {
            _activeSection = ((Button)sender).Tag.ToString();
            LoadBooks();
        }

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadBooks();

        private void BtnSortName_Click(object sender, RoutedEventArgs e)
        { _sortMode = "Name"; LoadBooks(); }

        private void BtnSortRating_Click(object sender, RoutedEventArgs e)
        { _sortMode = "Rating"; LoadBooks(); }
    }
}
    