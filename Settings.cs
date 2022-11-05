using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

/*
* ------ PARTIAL CLASS FÖR INSTÄLLNINGAR ------
* Innehåll:
* - Möjlighet att skräddarsy din restaurang
* - Nya listor för nya tider och bordsantal
* - Iterering
* - Selektion
* - Felhantering
* ---------------------------------------------
*/

namespace Labb_3___WPF_Booking_System
{
    public partial class MainWindow
    {
        List<string> restaurantOpens = new() { "06.00", "07.00", "08:00", "09.00", "10.00", "11.00", "12.00" };
        List<string> restaurantCloses = new() { "13.00", "14.00", "15.00", "16.00", "15.00", "16.00", "17.00", "18.00", "19.00", "20.00", "21.00", "22.00" };
        List<int> amountOfTables = new List<int>();

        private void ManageSettings()
        {
            for (int i = 1; i < 21; i++)
            {
                amountOfTables.Add(i);
            }

            this.ComboBox_OpenTime.SelectedIndex = 0;
            this.ComboBox_CloseTime.SelectedIndex = 0;
            this.ComboBox_TableAmount.SelectedIndex = 0;

            if (restaurantName == "")
                TextBox_SetRestaurantName.Text = "Restaurant Booking System";
            else
                TextBox_SetRestaurantName.Text = restaurantName;
        }
        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("Är du säker på att du vill genomföra ändringarna?", "Varning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                List<string> newTimes = new List<string>();
                List<int> newTableAmount = new List<int>();

                int tidsstart = ComboBox_OpenTime.SelectedIndex;
                int tidsstop = ComboBox_CloseTime.SelectedIndex +1;
                int bordsantal = ComboBox_TableAmount.SelectedIndex + 2;                

                for (int i = tidsstart; i < restaurantOpens.Count; i++)
                {
                    newTimes.Add(restaurantOpens[i]);
                }
                for (int i = 0; i < tidsstop; i++)
                {
                    newTimes.Add(restaurantCloses[i]);
                }

                for (int i = 1; i < bordsantal; i++)
                {
                    newTableAmount.Add(i);
                }

                ComboBox_BookTable.ItemsSource = newTableAmount;
                ComboBox_BookTime.ItemsSource = newTimes;

                ResetDefaultValues();
            }
            else
            {
                return;
            }
        }
        private void SaveNameThemeSettings(object sender, RoutedEventArgs e)
        {
            HandleRestaurantTheme();

            if (TextBox_SetRestaurantName.Text == "")
            {
                LabelRestaurantName.Content = "Restaurant Booking System";
            }
            else
            {
                LabelRestaurantName.Content = TextBox_SetRestaurantName.Text;               
            }

            System.Windows.MessageBox.Show("Dina ändringar är genomförda.", "Information", MessageBoxButton.OK);
        }
        private void RestoreAllSettings(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("Alla inställningar och bokningar kommer att återställas.\n" +
                "Fortsätta?", "Varning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                TextBox_SetRestaurantName.Text = "Restaurant Booking System";
                LabelRestaurantName.Content = "Restaurant Booking System";
                GreyTheme.IsChecked = true;

                this.ComboBox_OpenTime.SelectedIndex = 0;
                this.ComboBox_CloseTime.SelectedIndex = 0;
                this.ComboBox_TableAmount.SelectedIndex = 0;

                bookings.Clear();
                bookings.Add(new Customer("Ove Sundberg", "Fisk", new DateTime(2022, 12, 10).ToShortDateString(), "13.00", 2));
                bookings.Add(new Customer("Frodo Baggins", "Gluten", new DateTime(2022, 12, 10).ToShortDateString(), "14.00", 2));
                bookings.Add(new Customer("Rhaenyra Targaryen", "", new DateTime(2022, 12, 11).ToShortDateString(), "15.00", 4));

                this.ComboBox_BookTime.ItemsSource = defaultTimes;
                this.ComboBox_BookTable.ItemsSource = defaultTableAmount;
                ResetDefaultValues();
                HandleRestaurantTheme();
            }
            else
                return;
        }
        private void HandleRestaurantTheme()
        {
            SettingsHeadBorder_Gray.Visibility = Visibility.Hidden;
            SettingsHeadBorder_Blue.Visibility = Visibility.Hidden;
            SettingsHeadBorder_Pink.Visibility = Visibility.Hidden;

            SettingsFootBorder_Gray.Visibility = Visibility.Hidden;
            SettingsFootBorder_Blue.Visibility = Visibility.Hidden;
            SettingsFootBorder_Pink.Visibility = Visibility.Hidden;

            HeadBorder_Gray.Visibility = Visibility.Hidden;
            HeadBorder_Blue.Visibility = Visibility.Hidden;
            HeadBorder_Pink.Visibility = Visibility.Hidden;

            if (BlueTheme.IsChecked == true)
            {
                SettingsHeadBorder_Blue.Visibility = Visibility.Visible;
                SettingsFootBorder_Blue.Visibility = Visibility.Visible;
                HeadBorder_Blue.Visibility = Visibility.Visible;
            }
            else if (PinkTheme.IsChecked == true)
            {
                SettingsHeadBorder_Pink.Visibility = Visibility.Visible;
                SettingsFootBorder_Pink.Visibility = Visibility.Visible;
                HeadBorder_Pink.Visibility = Visibility.Visible;
            }
            else
            {
                SettingsHeadBorder_Gray.Visibility = Visibility.Visible;
                SettingsFootBorder_Gray.Visibility = Visibility.Visible;
                HeadBorder_Gray.Visibility = Visibility.Visible;
            }
        }
    }
}
