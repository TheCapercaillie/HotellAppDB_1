
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
                Console.Write("Rumsnummer: ");
                string roomNumber = Console.ReadLine()!;

                Console.Write("Rumstyp (Enkel/Dubbel/Svit): ");
                string type = Console.ReadLine()!;

                Console.Write("Pris per natt: ");
                decimal price = decimal.Parse(Console.ReadLine()!);

                db.Rooms.Add(new Room { RoomNumber = roomNumber, Type = type, PricePerNight = price });
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

                Console.Write("Incheckningsdatum (YYYY-MM-DD): ");
                DateTime checkIn = DateTime.Parse(Console.ReadLine()!);

                Console.Write("Utcheckningsdatum (YYYY-MM-DD): ");
                DateTime checkOut = DateTime.Parse(Console.ReadLine()!);

                db.Bookings.Add(new Booking { CustomerId = customerId, RoomId = roomId, CheckInDate = checkIn, CheckOutDate = checkOut });
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
