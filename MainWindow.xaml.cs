using System;
using System.Collections.Generic;
using System.Data;
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
using System.IO;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Runtime.CompilerServices;

/*
* ------ PARTIAL CLASS FÖR TIDSBOKNING ------
* Innehåll:
* - Listor för tider och bordsantal
* - Iterering
* - Selektion
* - Felhantering
* - Filhantering
* - LINQ (kodrad 197)
* - Asynkron metod (kodrad 81)
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
            if (bookings.Count > 0) // fullrow
            {
                bookings.RemoveAt(Grid_CustomerBookings.SelectedIndex);
                Grid_CustomerBookings.SelectedCells.Clear();
            }
            else
                return;
        }
        private void SaveBookingList(object sender, RoutedEventArgs e)
        {
            if (bookings.Count > 0)
            {
                try
                {
                    List<Customer> printList = new List<Customer>(bookings.OrderBy(o => o.customerDate).ToList()); //LINQ

                    System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();

                    dlg.Filter = "txt Files|*.txt";
                    dlg.FilterIndex = 1;
                    dlg.RestoreDirectory = true;

                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        using (FileStream fs = (FileStream)dlg.OpenFile())
                        {
                            using (StreamWriter sw = new StreamWriter(fs, Encoding.Default))
                            {
                                foreach (Customer customer in printList)
                                {
                                    customer.customerName.Trim();
                                }

                                for (int i = 0; i < printList.Count; i++)
                                {
                                    if (printList[i].customerName.Contains(' ') == false)
                                    {
                                        printList[i].customerName = printList[i].customerName + " NoLastName";
                                    }

                                    if (printList[i].customerAllergies == "")
                                    {
                                        printList[i].customerAllergies = "-";
                                    }
                                }

                                foreach (Customer obj in printList)
                                {
                                    sw.WriteLine($"{obj.customerDate} {obj.customerTime} {obj.customerName} {obj.customerTable} {obj.customerAllergies}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage("Någonting gick fel. Försök igen.");
                    _ = WriteToBugFile("SaveBookingList Method: " + ex.Message);
                }
            }
            else
                ErrorMessage("Du har inga registrerade bokningar.");
        }
        private void ReadBookingList(object sender, RoutedEventArgs e)
        {
            string[] tempbookings;
            List<string> templist = new List<string>();

            try
            {
                System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                dlg.DefaultExt = ".txt";
                dlg.Filter = "txt documents (.txt)|*.txt";

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (FileStream fs = (FileStream)dlg.OpenFile())
                    {
                        using (StreamReader sr = new StreamReader(fs, Encoding.Default))
                        {
                            tempbookings = sr.ReadToEnd().Split("\r\n");

                            foreach (string booking in tempbookings) // Adds all strings into a temporary list
                            {
                                templist.Add(booking);
                            }

                            templist.RemoveAt(templist.Count-1); // Removes the last item which is always null (new-line)

                            foreach (string booking in templist) // Works through every fifth item and adds them into the booking-list
                            {
                                
                                string[] t = booking.Split(' ');

                                if (t[3] == "NoLastName")
                                {
                                    t[3] = "";
                                }

                                if (t[5] == "-")
                                {
                                    t[5] = "";
                                }

                                string z = " ";
                                bookings.Add(new Customer(t[2] + z + t[3].Trim(), t[5].Trim(), t[0], t[1], int.Parse(t[4])));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage("Någonting gick fel vid inläsning.");
                _ = WriteToBugFile("ReadBookingList Method: " + ex.Message);
            }
        }
        private void ResetDefaultValues() // Sets ComboBoxes to show their first index, clears TextBoxes
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
