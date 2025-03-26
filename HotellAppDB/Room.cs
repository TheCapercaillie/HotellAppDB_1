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
        public string Type { get; set; } = string.Empty; // Enkel, Dubbel, Svit etc.
        public decimal PricePerNight { get; set; }
        public bool IsAvailable { get; set; } = true;

        public List<Booking> Bookings { get; set; } = new();
    }
}
