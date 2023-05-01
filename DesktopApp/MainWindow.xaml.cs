using System.Collections.Generic;
using System;
using System.Windows;
using System.Windows.Controls;
using DataLayer;
using DataLayer.Models;
using System.Windows.Data;
using System.Diagnostics;
using System.Reflection;
using DesktopApp.ModelWindows;
using System.Threading;

namespace DesktopApp
{
    public partial class MainWindow : Window
    {
        User User { get; set; }
        Dictionary<string, DataGrid> DataGrids { get; set; }
        public MainWindow(User user)
        {
            User = user;
            InitializeComponent();
            WelcomeLabel.Content = $"Admin: {User.FirstName} {User.LastName}";

            LoadDataGrids();
        }

        private void LoadDataGrids()
        {
            DataGrids = new Dictionary<string, DataGrid>();            

            DataGrids.Add("Users", new DataGrid()
            {
                ItemsSource = Database.Select<User>(),
                AutoGenerateColumns = false,
                IsReadOnly = true,
                Style = new Style(typeof(DataGrid))
                {
                    Setters =
                        {
                            new Setter(DataGrid.FontSizeProperty, 16.0)
                        }
                },
                GridLinesVisibility = DataGridGridLinesVisibility.None,
                AlternatingRowBackground = System.Windows.Media.Brushes.LightGray
            });
            DataGrids.Add("Rooms", new DataGrid()
            {
                ItemsSource = Database.Select<Room>(),
                AutoGenerateColumns = false,
                IsReadOnly = true,
                Style = new Style(typeof(DataGrid))
                {
                    Setters =
                        {
                            new Setter(DataGrid.FontSizeProperty, 16.0),
                        }

                },
                GridLinesVisibility = DataGridGridLinesVisibility.None,
                AlternatingRowBackground = System.Windows.Media.Brushes.LightGray
            });
            DataGrids.Add("Reservation", new DataGrid()
            {
                ItemsSource = Database.Select<Reservation>(),
                AutoGenerateColumns = false,
                IsReadOnly = true,
                Style = new Style(typeof(DataGrid))
                {
                    Setters =
                        {
                            new Setter(DataGrid.FontSizeProperty, 16.0)
                        }
                },
                GridLinesVisibility = DataGridGridLinesVisibility.None,
                AlternatingRowBackground = System.Windows.Media.Brushes.LightGray
            });
            DataGrids.Add("Reviews", new DataGrid()
            {
                ItemsSource = Database.Select<Review>(),
                AutoGenerateColumns = false,
                IsReadOnly = true,
                Style = new Style(typeof(DataGrid))
                {
                    Setters =
                        {
                            new Setter(DataGrid.FontSizeProperty, 16.0)
                        }
                },
                GridLinesVisibility = DataGridGridLinesVisibility.None,
                AlternatingRowBackground = System.Windows.Media.Brushes.LightGray
            });

            foreach (var item in DataGrids)
            {
                var type = item.Value.ItemsSource.GetType().GetGenericArguments()[0];
                var properties = type.GetProperties();
                foreach (var property in properties)
                {
                    var columnName = property.Name;
                    List<string> stretchCols = new List<string>() { "Description", "FirstName", "LastName", "Text", "CheckIn", "CheckOut" };
                    var column = new DataGridTextColumn()
                    {
                        Header = columnName,
                        Binding = new Binding(columnName)
                    };
                    if (Mapper.CheckAttribute<DbPrimaryKeyAttribute>(property) || property.PropertyType == typeof(int))
                    {
                        column.IsReadOnly = true;
                    }
                    if (stretchCols.Find(x => x == columnName) != null)
                    {
                        column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                    }
                    item.Value.Columns.Add(column);
                }
            }

            TabControl.Items.Clear();
            TabControl.ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(WrapPanel)));

            foreach (var item in DataGrids)
            {
                TabControl.Items.Add(new TabItem()
                {
                    Header = item.Key,
                    Content = item.Value
                });
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            // get data type of current grid
            var currentTab = TabControl.SelectedItem as TabItem;
            var currentDataGrid = (currentTab ?? throw new Exception("No tab selected")).Content as DataGrid;
            var type = currentDataGrid!.ItemsSource.GetType().GetGenericArguments()[0];

            // create thread to open new window
            switch (type.Name)
            {
                case "User":
                    var userWindow = new ViewGeneric<User>(null);
                    userWindow.ShowDialog();
                    break;
                case "Room":
                    var roomWindow = new ViewGeneric<Room>(null);
                    roomWindow.ShowDialog();
                    break;
                case "Reservation":
                    var reservationWindow = new ViewGeneric<Reservation>(null);
                    reservationWindow.ShowDialog();
                    break;
                case "Review":
                    var reviewWindow = new ViewGeneric<Review>(null);
                    reviewWindow.ShowDialog();
                    break;
                default:
                    throw new Exception("Unknown type");
            }

            LoadDataGrids();
        }
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var currentTab = TabControl.SelectedItem as TabItem;
            var currentDataGrid = (currentTab ?? throw new Exception("No tab selected")).Content as DataGrid;
            var selectedItem = currentDataGrid!.SelectedItem;

            if (selectedItem == null) { return; }
            var type = selectedItem.GetType();

            switch (type.Name)
            {
                case "User":
                    var userWindow = new ViewGeneric<User>(selectedItem as User);
                    userWindow.ShowDialog();
                    break;
                case "Room":
                    var roomWindow = new ViewGeneric<Room>(selectedItem as Room);
                    roomWindow.ShowDialog();
                    break;
                case "Reservation":
                    var reservationWindow = new ViewGeneric<Reservation>(selectedItem as Reservation);
                    reservationWindow.ShowDialog();
                    break;
                case "Review":
                    var reviewWindow = new ViewGeneric<Review>(selectedItem as Review);
                    reviewWindow.ShowDialog();
                    break;
                default:
                    throw new Exception("Unknown type");
            }            

            LoadDataGrids();
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var currentTab = TabControl.SelectedItem as TabItem;
            var currentDataGrid = (currentTab ?? throw new Exception("No tab selected")).Content as DataGrid;
            var selectedItem = currentDataGrid!.SelectedItem;

            if (selectedItem == null) { return; }

            var type = selectedItem.GetType();

            var result = Xceed.Wpf.Toolkit.MessageBox.Show($"Are you sure you want to delete {selectedItem}?", "Confirmation", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No) { return; }

            Database.Delete(type, selectedItem);

            LoadDataGrids();
        }
    }
}
