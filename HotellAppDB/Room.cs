using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotellAppDB.Data
{
    public class Room
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; 
        public int ExtraBeds { get; set; } = 0; 

        
        public decimal PricePerNight
        {
            get
            {
                if (Type == "Enkel")
                    return 250;
                if (Type == "Dubbel")
                    return 500;
                if (Type == "Dubbel med en extra säng")
                    return 600;
                if (Type == "Dubbel med två extra sängar")
                    return 700;
                return 0;
            }
        }

        public List<Booking> Bookings { get; set; } = new();

        public Room(string roomNumber, string type, int extraBeds = 0)
        {
            RoomNumber = roomNumber;
            Type = type;
            ExtraBeds = extraBeds;
        }

        
        public decimal CalculateTotalPrice(DateTime checkInDate, DateTime checkOutDate)
        {
            int nights = (checkOutDate - checkInDate).Days;
            return PricePerNight * nights;
        }
    }
}
