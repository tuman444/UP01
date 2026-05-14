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

namespace UP01.Pages.Admin
{
    /// <summary>
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            LoadComplaints();
        }

        // Переключение вкладок
        private void Tab_Click(object sender, RoutedEventArgs e)
        {
            string tag = ((Button)sender).Tag.ToString();

            IcComplaints.Visibility = Visibility.Collapsed;
            IcUnfreezes.Visibility = Visibility.Collapsed;
            IcRoleRequests.Visibility = Visibility.Collapsed;
            IcUsers.Visibility = Visibility.Collapsed;
            PanelFrozen.Visibility = Visibility.Collapsed;

            switch (tag)
            {
                case "complaints": LoadComplaints(); IcComplaints.Visibility = Visibility.Visible; break;
                case "unfreezes": LoadUnfreezes(); IcUnfreezes.Visibility = Visibility.Visible; break;
                case "roles": LoadRoleRequests(); IcRoleRequests.Visibility = Visibility.Visible; break;
                case "users": LoadUsers(); IcUsers.Visibility = Visibility.Visible; break;
                case "frozen": LoadFrozen(); PanelFrozen.Visibility = Visibility.Visible; break;
            }
        }

        // Загрузка данных 
        private void LoadComplaints()
        {
            // Complaint: ComplaintId, UserId, TargetBookId, TargetReviewId, Reason
            IcComplaints.ItemsSource = Core.DB.Complaint
                .Include("AppUser")
                .Include("Book")
                .Include("Review")
                .ToList();
        }

        private void LoadUnfreezes()
        {
            // UnfreezeRequest: RequestId, UserId, TargetBookId, IsAccountUnfreeze, Reason, RequestDate
            IcUnfreezes.ItemsSource = Core.DB.UnfreezeRequest
                .Include("AppUser")
                .Include("Book")
                .ToList();
        }

        private void LoadRoleRequests()
        {
            // RoleRequest: RequestId, UserId, RequestDate
            IcRoleRequests.ItemsSource = Core.DB.RoleRequest
                .Include("AppUser")
                .ToList();
        }

        private void LoadUsers()
        {
            IcUsers.ItemsSource = Core.DB.AppUser
                .Include("Role")
                .OrderBy(u => u.DisplayName)
                .ToList();
        }

        private void LoadFrozen()
        {
            IcFrozenBooks.ItemsSource = Core.DB.Book
                .Include("AppUser")
                .Where(b => b.IsFrozen)
                .ToList();

            IcFrozenUsers.ItemsSource = Core.DB.AppUser
                .Where(u => u.IsFrozen)
                .ToList();

            // У Review нет IsFrozen в схеме — эта секция пустая
            IcFrozenReviews.ItemsSource = null;
        }

        // Жалобы
        private void BtnAcceptComplaint_Click(object sender, RoutedEventArgs e)
        {
            var c = (Complaint)((Button)sender).Tag;

            // Замораживаем цель жалобы
            if (c.TargetBookId.HasValue)
            {
                var book = Core.DB.Book.Find(c.TargetBookId.Value);
                if (book != null) book.IsFrozen = true;
            }
            else if (c.TargetReviewId.HasValue)
            {
                // У Review нет IsFrozen — удаляем отзыв
                var review = Core.DB.Review.Find(c.TargetReviewId.Value);
                if (review != null) Core.DB.Review.Remove(review);
            }

            // Удаляем жалобу после обработки
            var dbC = Core.DB.Complaint.Find(c.ComplaintId);
            if (dbC != null) Core.DB.Complaint.Remove(dbC);

            Core.DB.SaveChanges();
            LoadComplaints();
        }

        private void BtnRejectComplaint_Click(object sender, RoutedEventArgs e)
        {
            var c = (Complaint)((Button)sender).Tag;
            var dbC = Core.DB.Complaint.Find(c.ComplaintId);
            if (dbC != null) Core.DB.Complaint.Remove(dbC);
            Core.DB.SaveChanges();
            LoadComplaints();
        }

        // Заявки на разморозку
        private void BtnAcceptUnfreeze_Click(object sender, RoutedEventArgs e)
        {
            var req = (UnfreezeRequest)((Button)sender).Tag;
            var dbReq = Core.DB.UnfreezeRequest.Find(req.RequestId);
            if (dbReq == null) return;

            if (dbReq.IsAccountUnfreeze)
            {
                var user = Core.DB.AppUser.Find(dbReq.UserId);
                if (user != null) user.IsFrozen = false;
            }
            else if (dbReq.TargetBookId.HasValue)
            {
                var book = Core.DB.Book.Find(dbReq.TargetBookId.Value);
                if (book != null) book.IsFrozen = false;
            }

            Core.DB.UnfreezeRequest.Remove(dbReq);
            Core.DB.SaveChanges();
            LoadUnfreezes();
        }

        private void BtnRejectUnfreeze_Click(object sender, RoutedEventArgs e)
        {
            var req = (UnfreezeRequest)((Button)sender).Tag;
            var dbReq = Core.DB.UnfreezeRequest.Find(req.RequestId);
            if (dbReq != null) Core.DB.UnfreezeRequest.Remove(dbReq);
            Core.DB.SaveChanges();
            LoadUnfreezes();
        }

        // Заявки на роль автора
        private void BtnAcceptRole_Click(object sender, RoutedEventArgs e)
        {
            var req = (RoleRequest)((Button)sender).Tag;
            var authorRole = Core.DB.Role.FirstOrDefault(r => r.RoleName == "Автор");
            var user = Core.DB.AppUser.Find(req.UserId);

            if (authorRole != null && user != null)
                user.RoleId = authorRole.RoleId;

            var dbReq = Core.DB.RoleRequest.Find(req.RequestId);
            if (dbReq != null) Core.DB.RoleRequest.Remove(dbReq);

            Core.DB.SaveChanges();
            LoadRoleRequests();
        }

        private void BtnRejectRole_Click(object sender, RoutedEventArgs e)
        {
            var req = (RoleRequest)((Button)sender).Tag;
            var dbReq = Core.DB.RoleRequest.Find(req.RequestId);
            if (dbReq != null) Core.DB.RoleRequest.Remove(dbReq);
            Core.DB.SaveChanges();
            LoadRoleRequests();
        }

        // Пользователи
        private void BtnFreezeUser_Click(object sender, RoutedEventArgs e)
        {
            var user = (AppUser)((Button)sender).Tag;
            var u = Core.DB.AppUser.Find(user.UserId);
            if (u != null) { u.IsFrozen = true; Core.DB.SaveChanges(); }
            LoadUsers();
        }

        private void BtnUnfreezeUser_Click(object sender, RoutedEventArgs e)
        {
            var user = (AppUser)((Button)sender).Tag;
            var u = Core.DB.AppUser.Find(user.UserId);
            if (u != null) { u.IsFrozen = false; Core.DB.SaveChanges(); }
            LoadUsers();
        }

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var user = (AppUser)((Button)sender).Tag;
            var dlg = new PasswordChangeDialog(user);
            dlg.ShowDialog();
            LoadUsers();
        }

        // Замороженные
        private void BtnUnfreezeBook_Click(object sender, RoutedEventArgs e)
        {
            var book = (Book)((Button)sender).Tag;
            var b = Core.DB.Book.Find(book.BookId);
            if (b != null) { b.IsFrozen = false; Core.DB.SaveChanges(); }
            LoadFrozen();
        }
    }
}
