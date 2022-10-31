using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
* ------ CLASS FÖR ATT SKAPA EN BOKNING ------
* Innehåll:
* - Konstruktor
* - Arv (från interface (IBooking)
* ---------------------------------------------
*/

namespace Labb_3___WPF_Booking_System
{
    public class Customer : IBooking
    {
        public string customerName { get; set; }
        public string customerAllergies { get; set; }
        public string customerDate { get; set; }
        public string customerTime { get; set; }
        public int customerTable { get; set; }

        public Customer(string customerName, string customerAllergies, string customerDate, string customerTime, int customerTable)
        {
            this.customerName = customerName;
            this.customerAllergies = customerAllergies;
            this.customerDate = customerDate;
            this.customerTime = customerTime;
            this.customerTable = customerTable;
        }
    }
}
