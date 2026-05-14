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

namespace UP01.Pages.Profile
{
    /// <summary>
    /// Логика взаимодействия для ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            Load();
        }
        private void Load()
        {
            var user = Core.AuthUser;
            if (user == null) return;

            user = Core.DB.AppUser.Include("Role").FirstOrDefault(u => u.UserId == user.UserId);
            Core.AuthUser = user;

            TbName.Text = user.DisplayName;
            TbLogin.Text = user.Login;
            TbEmail.Text = user.Email;  
            TbRole.Text = user.Role?.RoleName;

            FreezePanel.Visibility = user.IsFrozen ? Visibility.Visible : Visibility.Collapsed;

            if(user.Role?.RoleName == "Читатель")
            {
                bool alreadyRequested = Core.DB.RoleRequest.Any(r => r.UserId == user.UserId);
                BtnRequestAuthor.Visibility = Visibility.Visible;
                BtnRequestAuthor.IsEnabled = !alreadyRequested;
                if (alreadyRequested)
                    BtnRequestAuthor.Content = "Заявка уже подана";
            }

            IcReviews.ItemsSource = Core.DB.Review
                .Include("Book")
                .Where(r => r.UserId == user.UserId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        private void BtnRequestAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (Core.AuthUser == null) return;

            bool exists = Core.DB.RoleRequest.Any(r => r.UserId == Core.AuthUser.UserId);
            if (exists) return;

            Core.DB.RoleRequest.Add(new RoleRequest
            {
                UserId = Core.AuthUser.UserId,
                RequestDate = System.DateTime.Now
            });
            Core.DB.SaveChanges();

            ShowStatus("Заявка подана! Ожидайте решения администратора.");
            BtnRequestAuthor.IsEnabled = false;
            BtnRequestAuthor.Content = "Заявка уже подана";
        }
        private void BtnRequestUnfreeze_Click(object sender, RoutedEventArgs e)
        {
            if (Core.AuthUser == null) return;
            string reason = TbUnfreezeReason.Text.Trim();
            if (string.IsNullOrEmpty(reason))
            {
                ShowStatus("Укажите причину оспаривания");
                return;
            }

            // UnfreezeRequest: RequestId, UserId, TargetBookId, IsAccountUnfreeze, Reason, RequestDate
            Core.DB.UnfreezeRequest.Add(new UnfreezeRequest
            {
                UserId = Core.AuthUser.UserId,
                IsAccountUnfreeze = true,
                Reason = reason,
                RequestDate = System.DateTime.Now
            });
            Core.DB.SaveChanges();

            TbUnfreezeReason.Text = "";
            ShowStatus("Заявка на разморозку отправлена!");
        }

        private void ShowStatus(string msg)
        {
            TbStatus.Text = msg;
            TbStatus.Visibility = Visibility.Visible;
        }
    }
}
