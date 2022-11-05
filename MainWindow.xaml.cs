using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
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
using System.IO;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Windows.Forms;

/*
* ------ PARTIAL CLASS FÖR TIDSBOKNING ------
* Innehåll:
* - Lista med kunder, tre kunder som default
* - Listor för tider och bordsantal
* - Iterering
* - Selektion
* - Felhantering
* - Filhantering
* - Asynkrona metoder
* - Felmeddelande vid dubbelbokningar
* - Endast ett visst antal bord per datum tillåts bokas
* ---------------------------------------------
*/

namespace Labb_3___WPF_Booking_System
{
    public partial class MainWindow : Window
    {
        ObservableCollection<Customer> bookings = new ObservableCollection<Customer>()
        {
            new Customer("Ove Sundberg", "Fisk", new DateTime(2022, 12, 10).ToShortDateString(), "13.00", 2),
            new Customer("Frodo Baggins", "Gluten", new DateTime(2022, 12, 10).ToShortDateString(), "14.00", 2),
            new Customer("Rhaenyra Targaryen", "", new DateTime(2022, 12, 11).ToShortDateString(), "15.00", 4)
        };

        List<int> defaultTableAmount = new List<int>() { 1, 2, 3, 4, 5};
        List<string> defaultTimes = new List<string> { "13.00", "14.00", "15.00", "16.00", "17.00", "18.00", "19.00", "20.00", "21.00", "22.00" };

        string restaurantName = "";

        string customerName = "";
        string customerAllergies = "";
        DateTime calendarInput = DateTime.Now;
        string customerDate = "";
        string customerTime = "";
        int customerTable = 0;

        public MainWindow()
        {
            InitializeComponent();
            ManageSettings();

            this.Grid_CustomerBookings.ItemsSource = bookings;
            this.ComboBox_BookTable.ItemsSource = defaultTableAmount;
            this.ComboBox_BookTime.ItemsSource = defaultTimes;

            this.ComboBox_BookTable.SelectedIndex = 0;
            this.ComboBox_BookTime.SelectedIndex = 0;

            //Settings default values
            this.ComboBox_OpenTime.ItemsSource = restaurantOpens;
            this.ComboBox_CloseTime.ItemsSource = restaurantCloses;
            this.ComboBox_TableAmount.ItemsSource = amountOfTables;

            LabelRestaurantName.Content = "Restaurant Booking System";
            GreyTheme.IsChecked = true;

        }
        public static async Task WriteToBugFile(string bugmessage)
        {
            await File.AppendAllTextAsync("bugmessages.txt", bugmessage + "\n");
        }
        private void ErrorMessage(string message)
        {
            System.Windows.MessageBox.Show(message, "Fel", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }       
        private void BookTable(object sender, RoutedEventArgs e)
        {           
            customerName = TextBox_CustomerName.Text.Trim();
            customerAllergies = TextBox_CustomerAllergies.Text.Trim();            
            customerTime = ComboBox_BookTime.Text;
            customerTable = ComboBox_BookTable.SelectedIndex + 1;
            customerDate = ConvertCustomerDate();
            try
            {
                if (CheckValidInput(customerName, customerDate, customerTable, customerTime))
                {
                    bookings.Add(new Customer(customerName, customerAllergies, customerDate, customerTime, customerTable));
                    ResetDefaultValues();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex.Message);
                _ = WriteToBugFile("BookTable Method: " + ex.Message);
            }
        }
        private string ConvertCustomerDate()
        {
            if (BookingCalendar.SelectedDate == null)
            {
                calendarInput = DateTime.Now;
            }
            else
            {
                try
                {
                    calendarInput = (DateTime)BookingCalendar.SelectedDate;
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    ErrorMessage(ex.Message);
                    _ = WriteToBugFile("ConvertCustomerDate Method: " + ex.Message);

                }
                catch (InvalidOperationException ex)
                {
                    ErrorMessage(ex.Message);
                    _ = WriteToBugFile("ConvertCustomerDate Method: " + ex.Message);
                }
            }

            string date = calendarInput.ToShortDateString();

            return date;
        }
        private bool CheckValidInput(string customerName, string customerDate, int customerTable, string customerTime)
        {
            if (CheckValidName(customerName) && CheckValidBooking(customerDate, customerTable, customerTime) && CheckValidTable(customerTable))
                return true;
            else
                return false;           
        }
        private bool CheckValidName(string customerName)
        {
            if (customerName == "")
            {
                ErrorMessage("Du måste föra in ett namn på bokningen.");
                return false;
            }
            else
                return true;
        }
        private bool CheckValidTable(int customerTable)
        {
            if (customerTable > 0 && customerTable <= ComboBox_BookTable.Items.Count + 1)
                return true;
            else
            {
                ErrorMessage("Du har inte valt ett bord.");
                return false;
            }
        }
        private bool CheckValidBooking(string customerDate, int customerTable, string customerTime)
        {
            if (bookings.Count > 0)
            {
                for (int i = 0; i < bookings.Count; i++)
                {
                    if (customerDate == bookings[i].customerDate && customerTable == bookings[i].customerTable && customerTime == bookings[i].customerTime)
                    {
                        ErrorMessage("Bordet är redan bokat på angivet datum.");
                        return false;
                    }
                }
            }
            return true;
        }               
        private void CancelBooking(object sender, RoutedEventArgs e)
        {
            if (bookings.Count > 0)
            {
                int previousSelection = Grid_CustomerBookings.SelectedIndex;
                bookings.RemoveAt(Grid_CustomerBookings.SelectedIndex);
                Grid_CustomerBookings.SelectedCells.Clear();
                Grid_CustomerBookings.SelectedIndex = previousSelection-1; // Så man kan klicka avboka om och om igen från valt index
            }
        }
        private async void SaveBookingList(object sender, RoutedEventArgs e)
        {
            if (bookings.Count > 0)
            {
                try
                {
                    System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();

                    dlg.Filter = "Json files(*.json)|*.json";
                    dlg.FilterIndex = 1;
                    dlg.RestoreDirectory = true;

                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        using (FileStream fs = (FileStream)dlg.OpenFile())
                        {
                            await JsonSerializer.SerializeAsync(fs, bookings);
                            await fs.DisposeAsync();
                        }
                    }
                }
                catch (ArgumentNullException ex)
                {
                    ErrorMessage("Någonting gick fel. Försök igen.");
                    _ = WriteToBugFile("SaveBookingList Method: " + ex.Message);
                }
                catch (NotSupportedException ex)
                {
                    ErrorMessage("Någonting gick fel. Försök igen.");
                    _ = WriteToBugFile("SaveBookingList Method: " + ex.Message);
                }
            }
            else
            {
                ErrorMessage("Du har inga registrerade bokningar.");
            }
        }
        private async void ReadBookingList(object sender, RoutedEventArgs e)
        {
            List<Customer>? temp = new List<Customer>();
            bool doubleBooking = false;

            try
            {
                System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                dlg.DefaultExt = ".json";
                dlg.Filter = "Json files(*.json)|*.json";

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (FileStream fs = (FileStream)dlg.OpenFile())
                    {
                        temp = await JsonSerializer.DeserializeAsync<List<Customer>>(fs);
                    }
                }

                // Kollar ifall det blir dubbelbokningar med den nya listan
                if (temp.Count > 0)
                {
                    foreach (Customer customer in temp)
                    {
                        for (int i = 0; i < bookings.Count; i++)
                        {
                            if (bookings.Count > 0)
                            {
                                if (customer.customerTable == bookings[i].customerTable && customer.customerDate == bookings[i].customerDate
                                && customer.customerTime == bookings[i].customerTime)
                                {
                                    doubleBooking = true;
                                }
                            }
                        }
                    }

                    // Ger användaren möjlighet att välja att inte läsa in listan om dubbelbokningar kommer att finnas
                    if (doubleBooking == true)
                    {
                        if (System.Windows.MessageBox.Show("Dubbelbokningar har registrerats. Fortsätta?", "Varning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {
                            foreach (Customer customer in temp)
                            {
                                bookings.Add(customer);
                            }
                        }
                    }
                    else
                    {
                        foreach (Customer customer in temp)
                        {
                            bookings.Add(customer);
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                ErrorMessage("Någonting gick fel vid inläsning.");
                _ = WriteToBugFile("ReadBookingList Method: " + ex.Message);
            }
            catch(NotSupportedException ex)
            {
                ErrorMessage("Någonting gick fel vid inläsning.");
                _ = WriteToBugFile("ReadBookingList Method: " + ex.Message);
            }
            catch(ArgumentNullException ex)
            {
                ErrorMessage("Någonting gick fel vid inläsning.");
                _ = WriteToBugFile("ReadBookingList Method: " + ex.Message);
            }
            catch(Exception ex)
            {
                ErrorMessage("Någonting gick fel vid inläsning.");
                _ = WriteToBugFile("ReadBookingList Method: " + ex.Message);
            }            
        }
        private void ResetDefaultValues()
        {
            ComboBox_BookTime.SelectedIndex = 0;
            ComboBox_BookTable.SelectedIndex = 0;
            TextBox_CustomerName.Clear();
            TextBox_CustomerAllergies.Clear();
        }
        private void ClearList(object sender, RoutedEventArgs e)
        {
            if (bookings.Count > 0)
            {
                if (System.Windows.MessageBox.Show("Alla bokningar kommer att rensans. Fortsätta?", "Varning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    bookings.Clear();
                }
                else
                {
                    return;
                }
            }
        }
        private void QuitApplication(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("Är du säker på att du vill avsluta?", "Avsluta", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                System.Windows.Application.Current.Shutdown();
            }
            else
            {
                return;
            }
        }     
    }
}
