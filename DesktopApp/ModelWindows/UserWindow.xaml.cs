using System.Windows;
using DataLayer;
using DataLayer.Models;

namespace DesktopApp.ModelWindows
{
    public partial class UserWindow : Window
    {
        User? user;
        public UserWindow(User? user)
        {
            InitializeComponent();
            ActivityLabel.Content = "Adding new user";

            if (user != null)
            {
                FirstName.Text = user.FirstName;
                LastName.Text = user.LastName;
                Email.Text = user.Email;
                PhoneNumberText.Text = user.PhoneNumber;
                Password.Text = Encryption.Encrypt(user.Password);
                IsAdmin.IsChecked = user.IsAdmin;

                ActivityLabel.Content = $"Editing user {user.Id}: {user.FirstName} {user.LastName}";
            }

            this.user = user;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(FirstName.Text) || string.IsNullOrEmpty(LastName.Text) || string.IsNullOrEmpty(Email.Text) || string.IsNullOrEmpty(PhoneNumberText.Text) || string.IsNullOrEmpty(Password.Text))
            {
                MessageBox.Show("Please enter all fields");
                return;
            }

            if (user != null)
            {
                user.FirstName = FirstName.Text;
                user.LastName = LastName.Text;
                user.Email = Email.Text;
                user.PhoneNumber = PhoneNumberText.Text;
                user.Password = Encryption.Encrypt(Password.Text);
                user.IsAdmin = IsAdmin.IsChecked ?? false;
                Database.Update(user);
                MessageBox.Show("User updated");
                this.Close();
                return;
            }
            else
            {
                user = new User()
                {
                    FirstName = FirstName.Text,
                    LastName = LastName.Text,
                    Email = Email.Text,
                    PhoneNumber = PhoneNumberText.Text,
                    Password = Encryption.Encrypt(Password.Text),
                    IsAdmin = IsAdmin.IsChecked ?? false
                };
                Database.Insert(user);
                MessageBox.Show("User added");
                this.Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // confirmation
            var result = MessageBox.Show("Are you sure? All your edit will be lost.", "Confirmation", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No) { return; }

            this.Close();
        }
    }
}
