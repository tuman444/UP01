using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UP01.Pages.Catalog;

namespace UP01.Pages.Lists
{
    /// <summary>
    /// Логика взаимодействия для ListsPage.xaml
    /// </summary>
    public partial class ListsPage : Page
    {
        private string _activeSection = "Читаю";
        private string _sortMode = "Name";
        private List<ReadingListViewModel> _displayItems = new List<ReadingListViewModel>();
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

            var rawItems = query.ToList();

            // Преобразуем сущности базы данных во ViewModel со средним рейтингом
            _displayItems = rawItems.Select(rl => new ReadingListViewModel
            {
                ReadingListEntry = rl,
                Book = rl.Book,
                SelectedSection = rl.Section, // текущая секция выбрана изначально в ComboBox
                AverageRating = rl.Book.Review.Any() ? rl.Book.Review.Average(r => r.Rating) : 0
            }).ToList();

            // Сортировка
            if (_sortMode == "Rating")
                _displayItems = _displayItems.OrderByDescending(item => item.AverageRating).ToList();
            else
                _displayItems = _displayItems.OrderBy(item => item.Book.Title).ToList();

            // Передаем готовые данные в ItemsControl
            IcBooks.ItemsSource = _displayItems;
        }

        // Переход на страницу подробного описания книги при клике по самой плитке
        private void BookCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is ReadingListViewModel clickedItem)
            {
                NavigationService?.Navigate(new BookPage(clickedItem.Book));
            }
        }

        // Кнопка обработки перемещения книги в другой список
        private void BtnMove_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is ReadingListViewModel itemVm)
            {
                string targetSection = itemVm.SelectedSection;
                var entry = itemVm.ReadingListEntry;

                var dbEntry = Core.DB.ReadingList.FirstOrDefault(r =>
                    r.UserId == entry.UserId && r.BookId == entry.BookId);

                if (dbEntry != null)
                {
                    dbEntry.Section = targetSection;
                    Core.DB.SaveChanges();
                    LoadBooks(); // Обновляем контейнер, чтобы перемещенная книга исчезла из текущей вкладки
                }
            }

            // Блокируем всплытие события, чтобы клик по кнопке не вызвал открытие BookPage
            e.Handled = true;
        }

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

    // Класс представления данных одной плитки в списке пользователя
    public class ReadingListViewModel
    {
        public ReadingList ReadingListEntry { get; set; }
        public Book Book { get; set; }
        public string SelectedSection { get; set; } // Привязано свойством TwoWay к ComboBox
        public double AverageRating { get; set; }
        public string DisplayRating => AverageRating > 0 ? string.Format("★ {0:F1}", AverageRating) : "—";
    }
}
    