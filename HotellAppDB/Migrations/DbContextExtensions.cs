using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace HotellAppDB.Data
{
    public static class DbContextExtensions
    {
        public static void SeedData(this ApplicationDbContext context)
        {
            if (!context.Customers.Any())
            {
                context.Customers.AddRange(
                    new Customer { Name = "Anton Antonsson", Email = "anton@gmail.com", PhoneNumber = "0701234567" },
                    new Customer { Name = "Ben Bensson", Email = "ben@gmail.com", PhoneNumber = "07098765" },
                    new Customer { Name = "Cecilia Cederroth", Email = "cecilia@gmail.com", PhoneNumber = "0701112233" },
                    new Customer { Name = "Daniel Danielsson", Email = "daniel@gmail.com", PhoneNumber = "0709998877" }
                );
                context.SaveChanges();
            }

            if (!context.Rooms.Any())
            {
                context.Rooms.AddRange(
                    new Room("1", "Enkel", 0),
                    new Room("4", "Dubbel", 0),
                    new Room("8", "Dubbel med en extra säng", 1),
                    new Room("8", "Dubbel med två extra sängar", 2)
                );
                context.SaveChanges();
            }

            if (!context.Bookings.Any())
            {
                var rooms = context.Rooms.ToDictionary(r => r.Id);

                var bookings = new List<Booking>
                {
                    new Booking
                    {
                        CustomerId = 1,
                        RoomId = context.Rooms.First(r => r.RoomNumber == "1").Id,
                        CheckInDate = DateTime.Parse("2025-04-12"),
                        CheckOutDate = DateTime.Parse("2025-04-16")
                    },
                    new Booking
                    {
                        CustomerId = 2,
                        RoomId = context.Rooms.First(r => r.RoomNumber == "4").Id,
                        CheckInDate = DateTime.Parse("2025-04-13"),
                        CheckOutDate = DateTime.Parse("2025-04-15")
                    },
                    new Booking
                    {
                        CustomerId = 3,
                        RoomId = context.Rooms.First(r => r.RoomNumber == "8").Id,
                        CheckInDate = DateTime.Parse("2025-04-14"),
                        CheckOutDate = DateTime.Parse("2025-04-16")
                    },
                    new Booking
                    {
                        CustomerId = 4,
                        RoomId = context.Rooms.First(r => r.RoomNumber == "8").Id,
                        CheckInDate = DateTime.Parse("2025-04-18"),
                        CheckOutDate = DateTime.Parse("2025-04-20")
                    }
                };

                context.SaveChanges();
            }
        }
    }
}

