
using Microsoft.Extensions.Configuration;
using HotellAppDB.Data;
using Microsoft.EntityFrameworkCore;


namespace HotellAppDB
{

    internal class Program
    {

        static void Main(string[] args)
        {

            var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json", true, true);

            var config = builder.Build();

            var connectionString = "Server=localhost;Database=HotellAppDB;Trusted_Connection=True;TrustServerCertificate=true;MultipleActiveResultSets=true";

            var options = new DbContextOptionsBuilder<ApplicationDbContext>();

            options.UseSqlServer(connectionString);

            using (var dBContext = new ApplicationDbContext(options.Options))
            {
                dBContext.Database.Migrate();
            }

            using var db = new ApplicationDbContext(options.Options);

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Välkommen till HotellApp!");
                Console.WriteLine("1. Lägg till rum");
                Console.WriteLine("2. Lägg till kund");
                Console.WriteLine("3. Lägg till bokning");
                Console.WriteLine("4. Visa alla rum");
                Console.WriteLine("5. Visa alla kunder");
                Console.WriteLine("6. Visa alla bokningar");
                Console.WriteLine("7. Avsluta");
                Console.Write("Välj ett alternativ: ");

                var input = Console.ReadLine();

                switch (input)
                {
                    case "1": AddRoom(db); break;
                    case "2": AddCustomer(db); break;
                    case "3": AddBooking(db); break;
                    case "4": ShowRooms(db); break;
                    case "5": ShowCustomers(db); break;
                    case "6": ShowBookings(db); break;
                    case "7": return;
                    default: Console.WriteLine("Felaktigt val!"); break;
                }

                Console.WriteLine("\nTryck på valfri tangent för att fortsätta...");
                Console.ReadKey();
            }
            static void AddRoom(ApplicationDbContext db)
            {
                int roomNumber;
                while (true)
                {
                    Console.Write("Rumsnummer (1-20): ");
                    roomNumber = int.Parse(Console.ReadLine()!);

                    if (roomNumber < 1 || roomNumber > 20)
                    {
                        Console.WriteLine("Ogiltigt rumsnummer. Ange ett nummer mellan 1 och 20.");
                    }
                    else
                    {
                        break;
                    }
                }

                string type;
                while (true)
                {
                    Console.Write("Rumstyp (Enkel/Dubbel): ");
                    type = Console.ReadLine()!;

                    if (type != "Enkel" && type != "Dubbel")
                    {
                        Console.WriteLine("Ogiltig rumstyp. Vänligen ange 'Enkel' eller 'Dubbel'.");
                    }
                    else
                    {
                        break;
                    }
                }

                int extraBeds = 0;
                if (type == "Dubbel")
                {
                    while (true)
                    {
                        Console.Write("Antal extra sängar (0-2): ");
                        extraBeds = int.Parse(Console.ReadLine()!);

                        if (extraBeds >= 0 && extraBeds <= 2)
                        {
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Ogiltigt antal extra sängar. Ange ett tal mellan 0 och 2.");
                        }
                    }
                }


                db.Rooms.Add(new Room { RoomNumber = roomNumber.ToString(), Type = type, ExtraBeds = extraBeds });
                db.SaveChanges();
                Console.WriteLine("Rum tillagt!");
            }
            static void AddCustomer(ApplicationDbContext db)
            {
                Console.Write("Namn: ");
                string name = Console.ReadLine()!;

                Console.Write("E-post: ");
                string email = Console.ReadLine()!;

                Console.Write("Telefonnummer: ");
                string phone = Console.ReadLine()!;

                db.Customers.Add(new Customer { Name = name, Email = email, PhoneNumber = phone });
                db.SaveChanges();
                Console.WriteLine("Kund tillagd!");
            }

            static void AddBooking(ApplicationDbContext db)
            {
                Console.Write("Kund-ID: ");
                int customerId = int.Parse(Console.ReadLine()!);

                Console.Write("Rum-ID: ");
                int roomId = int.Parse(Console.ReadLine()!);

                DateTime checkIn;
                DateTime checkOut;

                while (true)
                {
                    Console.Write("Incheckningsdatum (YYYY-MM-DD): ");
                    if (DateTime.TryParse(Console.ReadLine(), out checkIn))
                    {
                        if (checkIn.Date >= DateTime.Today.Date) // Incheckning får inte vara på ett förflutet datum
                        {
                            break; // Giltigt incheckningsdatum
                        }
                        else
                        {
                            Console.WriteLine("Incheckningsdatum kan inte vara tidigare än dagens datum. Försök igen.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ogiltigt datumformat. Försök igen.");
                    }
                }

                while (true)
                {
                    Console.Write("Utcheckningsdatum (YYYY-MM-DD): ");
                    if (DateTime.TryParse(Console.ReadLine(), out checkOut))
                    {
                        if (checkOut.Date > checkIn.Date) // Utcheckningen måste vara senare än incheckningen
                        {
                            if (checkOut.Date >= DateTime.Today.Date) // Utcheckning får inte vara på ett förflutet datum
                            {
                                break; // Giltigt utcheckningsdatum
                            }
                            else
                            {
                                Console.WriteLine("Utcheckningsdatum kan inte vara tidigare än dagens datum. Försök igen.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Utcheckningsdatum måste vara efter incheckningsdatum. Försök igen.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ogiltigt datumformat. Försök igen.");
                    }
                }

                // Kolla om rummet redan är bokat under perioden
                var overlappingBooking = db.Bookings
                    .Where(b => b.RoomId == roomId &&
                        b.CheckInDate < checkOut &&
                        b.CheckOutDate > checkIn)
                    .FirstOrDefault();

                if (overlappingBooking != null)
                {
                    Console.WriteLine("Det här rummet är redan bokat under vald period.");
                    return;
                }

                var room = db.Rooms.FirstOrDefault(r => r.Id == roomId);
                if (room == null)
                {
                    Console.WriteLine("Rummet hittades inte.");
                    return;
                }

                // Beräkna antal nätter
                int nights = (checkOut - checkIn).Days;

                // Använd GetPricePerNight-metoden för att beräkna priset
                decimal pricePerNight = room.GetPricePerNight();
                decimal totalPrice = pricePerNight * nights;

                Console.WriteLine($"Bokningen kommer att kosta {totalPrice} kr för {nights} nätter.");

                // Skapa bokningen med det beräknade totalpriset
                db.Bookings.Add(new Booking
                {
                    CustomerId = customerId,
                    RoomId = roomId,
                    CheckInDate = checkIn,
                    CheckOutDate = checkOut,
                    TotalPrice = totalPrice // Sätt det beräknade totalpriset
                });

                db.SaveChanges();
                Console.WriteLine("Bokning tillagd!");
            }



            static void ShowRooms(ApplicationDbContext db)
            {
                var rooms = db.Rooms.ToList();
                foreach (var room in rooms)
                {
                    Console.WriteLine($"ID: {room.Id}, Rum: {room.RoomNumber}, Typ: {room.Type}, Pris: {room.PricePerNight}");
                }
            }
            static void ShowCustomers(ApplicationDbContext db)
            {
                var customers = db.Customers.ToList();
                foreach (var customer in customers)
                {
                    Console.WriteLine($"ID: {customer.Id}, Namn: {customer.Name}, E-post: {customer.Email}, Telefon: {customer.PhoneNumber}");
                }
            }

            static void ShowBookings(ApplicationDbContext db)
            {
                var bookings = db.Bookings.Include(b => b.Customer).Include(b => b.Room).ToList();
                foreach (var booking in bookings)
                {
                    Console.WriteLine($"Bokning ID: {booking.Id}, Kund: {booking.Customer.Name}, Rum: {booking.Room.RoomNumber}, {booking.CheckInDate} - {booking.CheckOutDate}");
                }
            }

        }
    }
}