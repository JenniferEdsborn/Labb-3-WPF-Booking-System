using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labb_3___WPF_Booking_System
{
    internal interface IBooking
    {
        string customerName { get; set; }
        string customerAllergies { get; set; }
        string customerDate { get; set; }
        string customerTime { get; set; }
        int customerTable { get; set; }
    }
}
