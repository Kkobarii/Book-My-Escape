using DataLayer;
using DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace DesktopApp.ModelWindows
{
    public partial class ViewBase : Window
    {
        public ViewBase()
        {
            InitializeComponent();
        }

        public virtual void Confirm_Click(object sender, RoutedEventArgs e) { }
        public virtual void Cancel_Click(object sender, RoutedEventArgs e) { }
    }

    public partial class ViewGeneric<T> : ViewBase
    {
        T Value { get; set; }
        public ViewGeneric(T? value)
        {
            InitializeComponent();

            if (value != null)
            {
                Value = value;
                ActivityLabel.Content = $"Editing {typeof(T).Name}: {value}";
            }
            else
            {
                ActivityLabel.Content = $"Adding new {typeof(T).Name}";
            }

            InputGrid.Margin = new Thickness(10,10,10,10);
            InputGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            InputGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            int i = 0;
            int row = 0;
            foreach(var property in typeof(T).GetProperties())
            {
                InputGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
                InputGrid.Children.Add(new Label() 
                { 
                    Content = property.Name,
                    Margin = new Thickness(10),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold
                });

                if (Mapper.CheckAttribute<DbPrimaryKeyAttribute>(property))
                {
                    if (value == null)
                        InputGrid.Children.Add(new TextBox() { IsEnabled = false, FontSize = 14, Height = 35, VerticalContentAlignment = VerticalAlignment.Center });
                    else
                        InputGrid.Children.Add(new TextBox() { Text = property.GetValue(value).ToString(), IsEnabled = false, FontSize = 14, Height = 35, VerticalContentAlignment = VerticalAlignment.Center });
                }
                else if (Mapper.CheckAttribute<DbForeignKeyAttribute>(property))
                {
                    Type type = property.PropertyType;

                    // Couldnt figure out how to make a generic list of a generic type
                    if (type == typeof(User))
                    {
                        List<User> users = Database.Select<User>();


                        ComboBox comboBox = new ComboBox() { Height = 35, VerticalContentAlignment = VerticalAlignment.Center };
                        comboBox.FontSize = 14;
                        comboBox.ItemsSource = users;

                        if (value != null)
                        {
                            foreach (var user in users)
                            {
                                if (user.Id == ((User)property.GetValue(value)!).Id)
                                {
                                    comboBox.SelectedItem = user;
                                    break;
                                }
                            }
                        }

                        InputGrid.Children.Add(comboBox);
                    }
                    if (type == typeof(Room))
                    {
                        List<Room> companies = Database.Select<Room>();

                        ComboBox comboBox = new ComboBox() { Height = 35, VerticalContentAlignment = VerticalAlignment.Center };
                        comboBox.FontSize = 14;
                        comboBox.ItemsSource = companies;

                        if (value != null)
                        {
                            foreach (var company in companies)
                            {
                                if (company.Id == ((Room)property.GetValue(value)!).Id)
                                {
                                    comboBox.SelectedItem = company;
                                    break;
                                }
                            }
                        }

                        InputGrid.Children.Add(comboBox);
                    }
                }                
                else if (property.PropertyType == typeof(bool))
                {
                    if (value == null)
                        InputGrid.Children.Add(new CheckBox() { VerticalAlignment = VerticalAlignment.Center, Height = 35, VerticalContentAlignment = VerticalAlignment.Center });
                    else
                        InputGrid.Children.Add(new CheckBox() { IsChecked = (bool)property.GetValue(value)!, VerticalAlignment = VerticalAlignment.Center, Height = 35, VerticalContentAlignment = VerticalAlignment.Center });
                }
                else if (property.PropertyType == typeof(DateTime))
                {
                    if (value == null)
                        InputGrid.Children.Add(new DateTimePicker() { FontSize = 14, Height = 35, VerticalContentAlignment = VerticalAlignment.Center });
                    else
                        InputGrid.Children.Add(new DateTimePicker() { Value = (DateTime)property.GetValue(value)!, FontSize = 14, Height = 35, VerticalContentAlignment = VerticalAlignment.Center });
                }
                else
                {
                    if (value == null)
                        InputGrid.Children.Add(new TextBox() { FontSize = 14, Height = 35, VerticalContentAlignment = VerticalAlignment.Center });
                    else if (property.Name == "Password" && value != null)
                        InputGrid.Children.Add(new TextBox() { IsEnabled = false, Text = (property.GetValue(value) ?? "").ToString(), FontSize = 14, Height = 35, VerticalContentAlignment = VerticalAlignment.Center });
                    else
                        InputGrid.Children.Add(new TextBox() { Text = (property.GetValue(value) ?? "").ToString(), FontSize = 14, Height = 35, VerticalContentAlignment = VerticalAlignment.Center });
                }

                Grid.SetColumn(InputGrid.Children[i], 0);
                Grid.SetRow(InputGrid.Children[i], row);
                i++;

                Grid.SetColumn(InputGrid.Children[i], 1);
                Grid.SetRow(InputGrid.Children[i], row);
                i++;

                row++;
            }
        }

        public override void Confirm_Click(object sender, RoutedEventArgs e)
        {
            bool isNull = false;
            if (Value == null)
            {
                Value = (T)Activator.CreateInstance(typeof(T));
                isNull = true;
            }

            int i = 0;
            foreach(var property in typeof(T).GetProperties())
            {
                if (Mapper.CheckAttribute<DbPrimaryKeyAttribute>(property)) { }
                else if (Mapper.CheckAttribute<DbForeignKeyAttribute>(property))
                {
                    Type type = property.PropertyType;
                    if (type == typeof(User))
                    {
                        ComboBox comboBox = (ComboBox)InputGrid.Children[i + 1];
                        property.SetValue(Value, comboBox.SelectedItem);
                    }
                    if (type == typeof(Room))
                    {
                        ComboBox comboBox = (ComboBox)InputGrid.Children[i + 1];
                        property.SetValue(Value, comboBox.SelectedItem);
                    }
                }
                else if (property.PropertyType == typeof(bool))
                {
                    CheckBox checkBox = (CheckBox)InputGrid.Children[i + 1];
                    property.SetValue(Value, checkBox.IsChecked);
                }
                else if (property.PropertyType == typeof(DateTime))
                {
                    DateTimePicker dateTimePicker = (DateTimePicker)InputGrid.Children[i + 1];
                    property.SetValue(Value, dateTimePicker.Value);
                }
                else
                { 
                    TextBox textBox = (TextBox)InputGrid.Children[i + 1];
                    if (string.IsNullOrEmpty(textBox.Text) && !Mapper.CheckNullable(property))
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show($"{property.Name} cannot be null");
                        return;
                    }

                    if (property.Name == "Password" && isNull == true)
                        property.SetValue(Value, Encryption.Encrypt(textBox.Text));
                    else
                    {

                        try { property.SetValue(Value, Mapper.ConvertFromSqlType(textBox.Text, property)); }
                        catch
                        {
                            Xceed.Wpf.Toolkit.MessageBox.Show($"Invalid input for {property.Name}");
                            return;
                        }
                    }
                    
                }
                i += 2;
            }

            if (isNull)
                Database.Insert(typeof(T), Value);
            else
                Database.Update(typeof(T), Value);

            var result = Xceed.Wpf.Toolkit.MessageBox.Show($"{Value!.GetType().Name} {Value} {(isNull ? "added" : "updated")} successfully", "Success!");

            DialogResult = true;
            Close();
        }

        public override void Cancel_Click(object sender, RoutedEventArgs e)
        {
            var result = Xceed.Wpf.Toolkit.MessageBox.Show("Are you sure? All your edit will be lost.", "Confirmation", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No) { return; }

            DialogResult = false;
            Close();
        }
    }
}
