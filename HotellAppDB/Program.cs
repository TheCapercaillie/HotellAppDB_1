
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
                Console.WriteLine("Tillgängliga kunder:");
                foreach (var c in db.Customers)
                    Console.WriteLine($"Namn: {c.Name}, E-post: {c.Email}");

                Console.Write("Ange kundens e-post: ");
                string email = Console.ReadLine()!;
                var customer = db.Customers.FirstOrDefault(c => c.Email == email);

                if (customer == null)
                {
                    Console.WriteLine("Kund hittades inte.");
                    return;
                }

                
                Console.WriteLine("Tillgängliga rum:");
                foreach (var r in db.Rooms)
                    Console.WriteLine($"Rumsnummer: {r.RoomNumber}, Typ: {r.Type}");

                Console.Write("Ange rumsnummer (1-20): ");
                string roomNumber = Console.ReadLine()!;
                var room = db.Rooms.FirstOrDefault(r => r.RoomNumber == roomNumber);

                if (room == null)
                {
                    Console.WriteLine("Rummet hittades inte.");
                    return;
                }

                DateTime checkIn;
                DateTime checkOut;
                Booking overlappingBooking;

                while (true)
                {
                    // Incheckning
                    while (true)
                    {
                        Console.Write("Incheckningsdatum (YYYY-MM-DD): ");
                        if (DateTime.TryParse(Console.ReadLine(), out checkIn))
                        {
                            if (checkIn.Date >= DateTime.Today.Date)
                                break;
                            else
                                Console.WriteLine("Incheckningsdatum kan inte vara tidigare än dagens datum. Försök igen.");
                        }
                        else
                        {
                            Console.WriteLine("Ogiltigt datumformat. Försök igen.");
                        }
                    }

                    // Utcheckning
                    while (true)
                    {
                        Console.Write("Utcheckningsdatum (YYYY-MM-DD): ");
                        if (DateTime.TryParse(Console.ReadLine(), out checkOut))
                        {
                            if (checkOut.Date > checkIn.Date && checkOut.Date >= DateTime.Today.Date)
                                break;
                            else
                                Console.WriteLine("Utcheckningsdatum måste vara efter incheckningsdatum och inte tidigare än idag. Försök igen.");
                        }
                        else
                        {
                            Console.WriteLine("Ogiltigt datumformat. Försök igen.");
                        }
                    }

                    overlappingBooking = db.Bookings
                        .FirstOrDefault(b =>
                            b.RoomId == room.Id &&
                            b.CheckInDate < checkOut &&
                            b.CheckOutDate > checkIn);

                    if (overlappingBooking == null)
                        break; 

                    Console.WriteLine("Det här rummet är redan bokat under vald period. Försök med andra datum.");
                }

                var roomToBook = db.Rooms.FirstOrDefault(r => r.RoomNumber == roomNumber);
                if (roomToBook == null)
                {
                    Console.WriteLine("Rummet hittades inte.");
                    return;
                }

                // Beräkna antal nätter
                int nights = (checkOut - checkIn).Days;

                // Beräkna priset
                decimal pricePerNight = room.GetPricePerNight();
                decimal totalPrice = pricePerNight * nights;

                Console.WriteLine($"Bokningen kommer att kosta {totalPrice} kr för {nights} nätter.");

                
                db.Bookings.Add(new Booking
                {
                    CustomerId = customer.Id,
                    RoomId = room.Id,
                    CheckInDate = checkIn,
                    CheckOutDate = checkOut,
                    TotalPrice = totalPrice 
                });

                db.SaveChanges();
                Console.WriteLine("Bokning tillagd!");
            }



            static void ShowRooms(ApplicationDbContext db)
            {
                var rooms = db.Rooms.ToList();
                foreach (var room in rooms)
                {
                    Console.WriteLine($"ID: {room.Id}, Rum: {room.RoomNumber}, Typ: {room.Type}, Pris per natt: {room.GetPricePerNight()} kr");
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
                    int nights = (booking.CheckOutDate - booking.CheckInDate).Days;
                    decimal pricePerNight = booking.Room.GetPricePerNight();
                    decimal totalPrice = pricePerNight * nights;

                    Console.WriteLine(
                        $"Bokning ID: {booking.Id}, Kund: {booking.Customer.Name}, Rum: {booking.Room.RoomNumber}, " +
                        $"In: {booking.CheckInDate.ToShortDateString()}, Ut: {booking.CheckOutDate.ToShortDateString()}, " +
                        $"Antal nätter: {nights}, Totalpris: {totalPrice} kr");
                }
            }

        }
    }
}