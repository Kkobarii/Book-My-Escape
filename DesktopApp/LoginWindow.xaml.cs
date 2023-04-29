using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using DataLayer;
using DataLayer.Models;

namespace DesktopApp
{
    public partial class LoginWindow : Window
    {
        string Email = string.Empty;
        string Password = string.Empty;
        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Email = txtEmail.Text;
            Password = Encryption.Encrypt(txtPassword.Password);

            Debug.WriteLine(Password);

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Please enter email and password");
                return;
            }

            var users = Database.Select<User>("Email", Email);

            if (users.Count == 0)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("User not found");
                return;
            }

            var user = users.First();

            if (user.Password != Password)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Wrong password");
                return;
            }

            if (!user.IsAdmin)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("You are not admin");
                return;
            }

            var mainWindow = new MainWindow(user);
            mainWindow.Show();
            this.Close();
        }
    }
}
