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
        public string Type { get; set; } = string.Empty; // Enkel och Dubbel rum
        public decimal PricePerNight { get; set; }
        public bool IsAvailable { get; set; } = true;

        public int ExtraBeds { get; set; } = 0; // Gäller bara för dubbelrum

        public List<Booking> Bookings { get; set; } = new();

        public decimal GetPricePerNight()
        {
            if (Type == "Enkel")
            {
                return 250; // Pris för enkelrum
            }
            else if (Type == "Dubbel")
            {
                if (ExtraBeds == 0)
                    return 500; // Pris för dubbelrum utan extra säng
                else if (ExtraBeds == 1)
                    return 600; // Pris för dubbelrum med 1 extra säng
                else if (ExtraBeds == 2)
                    return 700; // Pris för dubbelrum med 2 extra sängar
            }

            return 0;
        }

    }
}
