
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

            
            using (var dbContextForSeeding = new ApplicationDbContext(options.Options))
            {
                
                dbContextForSeeding.SeedData();
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
                Console.WriteLine("7. Redigera rum");
                Console.WriteLine("8. Redigera kund");
                Console.WriteLine("9. Redigera bokning");
                Console.WriteLine("10. Ta bort bokning, kund och rum");
                Console.WriteLine("0. Avsluta");
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
                    case "7": EditRooms(db); break;
                    case "8": EditCustomers(db); break;
                    case "9": EditBookings(db); break;
                    case "10":DeleteBookingCustomerRoom(db); break;
                    case "0": return;
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

                Console.WriteLine("Välj rumstyp:");
                Console.WriteLine("1. Enkel (250 kr per natt)");
                Console.WriteLine("2. Dubbel (500 kr per natt)");

                string roomTypeChoice = Console.ReadLine();
                Room room = new Room(roomNumber.ToString(), "Enkel");

                switch (roomTypeChoice)
                {
                    case "1":
                        room.Type = "Enkel";
                        
                        break;
                    case "2":
                        room.Type = "Dubbel";
                        
                        break;
                    default:
                        Console.WriteLine("Ogiltigt val.");
                        return;
                }

                if (room.Type.Contains("Dubbel"))
                {
                    Console.WriteLine("Välj antal extra sängar 0-2 (100 kr per säng per natt): ");
                    int extraBeds = int.Parse(Console.ReadLine() ?? "0");

                    if (extraBeds < 0 || extraBeds > 2)
                    {
                        Console.WriteLine("Ogiltigt antal extra sängar, försök igen.");
                        return;
                    }

                    
                    if (extraBeds == 0)
                    {
                        room.Type = "Dubbel";
                        room.ExtraBeds = 0;
                    }
                    else if (extraBeds == 1)
                    {
                        room.Type = "Dubbel med en extra säng";
                        room.ExtraBeds = 1;
                    }
                    else
                    {
                        room.Type = "Dubbel med två extra sängar";
                        room.ExtraBeds = 2;
                    }
                }

                
                db.Rooms.Add(room);
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

                
                int nights = (checkOut - checkIn).Days;

                
                decimal pricePerNight = room.PricePerNight;
                decimal totalPrice = pricePerNight * nights;

                Console.WriteLine($"Bokningen kommer att kosta {totalPrice} kr för {nights} nätter.");

                
                db.Bookings.Add(new Booking
                {
                    CustomerId = customer.Id,
                    RoomId = room.Id,
                    CheckInDate = checkIn,
                    CheckOutDate = checkOut,
                    
                });

                db.SaveChanges();
                Console.WriteLine("Bokning tillagd!");
            }



            static void ShowRooms(ApplicationDbContext db)
            {
                
                var rooms = db.Rooms.ToList();
                foreach (var room in rooms)
                {
                    int price = GetRoomPrice(room);
                    Console.WriteLine($"ID: {room.Id}, Rum: {room.RoomNumber}, Typ: {room.Type}, Pris per natt: {room.PricePerNight} kr");
                }
            }

            static int GetRoomPrice(Room room)
            {
                
                if (room.Type.Contains("Dubbel med två extra sängar"))
                {
                    return 700;
                }
                else if (room.Type.Contains("Dubbel med en extra säng"))
                {
                    return 600;
                }
                else if (room.Type.Contains("Dubbel"))
                {
                    return 500;
                }
                else if (room.Type.Contains("Enkel"))
                {
                    return 250;
                }
                return 0; 
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
                    decimal pricePerNight = booking.Room.PricePerNight;
                    decimal totalPrice = pricePerNight * nights;

                    Console.WriteLine(
                        $"Bokning ID: {booking.Id}, Kund: {booking.Customer.Name}, Rum: {booking.Room.RoomNumber}, " +
                        $"In: {booking.CheckInDate.ToShortDateString()}, Ut: {booking.CheckOutDate.ToShortDateString()}, " +
                        $"Antal nätter: {nights}, Totalpris: {totalPrice} kr");
                }
            }

            static void EditRooms(ApplicationDbContext db)
            {
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("Redigera rum");
                    Console.Write("Ange rum-ID för att redigera (eller tryck på '0' för att gå tillbaka): ");

                    if (int.TryParse(Console.ReadLine(), out int roomId) && roomId != 0)
                    {
                        var room = db.Rooms.FirstOrDefault(r => r.Id == roomId);
                        if (room == null)
                        {
                            Console.WriteLine("Rummet hittades inte.");
                        }
                        else
                        {
                            Console.WriteLine($"Redigerar rum: {room.RoomNumber}");

                            
                            Console.WriteLine($"Nuvarande rumstyp: {room.Type}, Pris: {room.PricePerNight} kr");

                            
                            Console.WriteLine("Välj ny rumstyp:");
                            Console.WriteLine("1. Enkel (250 kr per natt)");
                            Console.WriteLine("2. Dubbel (500 kr per natt)");

                            string roomTypeChoice = Console.ReadLine();

                            switch (roomTypeChoice)
                            {
                                case "1":
                                    room.Type = "Enkel";
                                    
                                    break;
                                case "2":
                                    room.Type = "Dubbel";
                                    
                                    break;
                                default:
                                    Console.WriteLine("Ogiltigt val.");
                                    continue; 
                            }

                            
                            if (room.Type.Contains("Dubbel"))
                            {
                                Console.WriteLine("Välj antal extra sängar 0-2 (100 kr per säng per natt): ");
                                int extraBeds = int.Parse(Console.ReadLine() ?? "0");

                                if (extraBeds < 0 || extraBeds > 2)
                                {
                                    Console.WriteLine("Ogiltigt antal extra sängar, försök igen.");
                                    continue;
                                }

                                
                                if (extraBeds == 0)
                                {
                                    room.Type = "Dubbel";
                                    room.ExtraBeds = 0;
                                }
                                else if (extraBeds == 1)
                                {
                                    room.Type = "Dubbel med en extra säng";
                                    room.ExtraBeds = 1;
                                }
                                else
                                {
                                    room.Type = "Dubbel med två extra sängar";
                                    room.ExtraBeds = 2;
                                }
                            }

                            
                            db.SaveChanges();
                            Console.WriteLine("Rummet har uppdaterats.");

                            
                            Console.WriteLine("\nTryck på valfri tangent för att återgå till huvudmenyn...");
                            Console.ReadKey();
                            break; 
                        }
                    }
                    else if (roomId == 0)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Ogiltigt ID. Försök igen.");
                    }

                }
            }



            static void EditCustomers(ApplicationDbContext db)
            {
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("Redigera kundinformation");
                    Console.Write("Ange kund-ID för att redigera (eller tryck på '0' för att gå tillbaka): ");

                    if (int.TryParse(Console.ReadLine(), out int customerId) && customerId != 0)
                    {
                        var customer = db.Customers.FirstOrDefault(c => c.Id == customerId);
                        if (customer == null)
                        {
                            Console.WriteLine("Kunden hittades inte.");
                        }
                        else
                        {
                            Console.WriteLine($"Redigerar kund: {customer.Name}");

                            
                            Console.WriteLine($"Nuvarande namn: {customer.Name}");
                            Console.WriteLine($"Nuvarande e-post: {customer.Email}");
                            Console.WriteLine($"Nuvarande telefonnummer: {customer.PhoneNumber}");

                            
                            Console.Write("Ange nytt namn (eller tryck Enter för att behålla nuvarande): ");
                            string newName = Console.ReadLine();
                            if (!string.IsNullOrWhiteSpace(newName)) customer.Name = newName;

                            
                            Console.Write("Ange ny e-post (eller tryck Enter för att behålla nuvarande): ");
                            string newEmail = Console.ReadLine();
                            if (!string.IsNullOrWhiteSpace(newEmail)) customer.Email = newEmail;

                            
                            Console.Write("Ange nytt telefonnummer (eller tryck Enter för att behålla nuvarande): ");
                            string newPhoneNumber = Console.ReadLine();
                            if (!string.IsNullOrWhiteSpace(newPhoneNumber)) customer.PhoneNumber = newPhoneNumber;

                            
                            db.SaveChanges();
                            Console.WriteLine("Kundinformation har uppdaterats.");

                            Console.WriteLine("\nTryck på valfri tangent för att återgå till huvudmenyn...");
                            Console.ReadKey();
                            break;
                        }
                    }
                    else if (customerId == 0)
                    {
                        break; 
                    }
                    else
                    {
                        Console.WriteLine("Ogiltigt ID. Försök igen.");
                    }

                    Console.WriteLine("\nTryck på valfri tangent för att återgå till huvudmenyn...");
                    Console.ReadKey();
                }
            }


            static void EditBookings(ApplicationDbContext db)
            {
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("Redigera bokning");
                    Console.Write("Ange boknings-ID för att redigera (eller tryck på '0' för att gå tillbaka): ");

                    if (int.TryParse(Console.ReadLine(), out int bookingId) && bookingId != 0)
                    {
                        var booking = db.Bookings.FirstOrDefault(b => b.Id == bookingId);
                        if (booking == null)
                        {
                            Console.WriteLine("Bokningen hittades inte.");
                        }
                        else
                        {
                            Console.WriteLine($"Redigerar bokning: {booking.Id}");
                            Console.WriteLine($"Nuvarande incheckningsdatum: {booking.CheckInDate.ToShortDateString()}, Utcheckningsdatum: {booking.CheckOutDate.ToShortDateString()}");

                            DateTime newCheckIn;
                            DateTime newCheckOut;

                            
                            while (true)
                            {
                                Console.Write("Ange nytt incheckningsdatum (YYYY-MM-DD): ");
                                if (DateTime.TryParse(Console.ReadLine(), out newCheckIn))
                                {
                                    if (newCheckIn.Date >= DateTime.Today.Date) 
                                    {
                                        break; 
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
                                Console.Write("Ange nytt utcheckningsdatum (YYYY-MM-DD): ");
                                if (DateTime.TryParse(Console.ReadLine(), out newCheckOut))
                                {
                                    if (newCheckOut.Date > newCheckIn.Date) 
                                    {
                                        if (newCheckOut.Date >= DateTime.Today.Date) 
                                        {
                                            break; 
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

                            
                            bool isRoomBooked = db.Bookings
                                .Any(b =>
                                    b.RoomId == booking.RoomId &&
                                    newCheckIn < b.CheckOutDate &&
                                    newCheckOut > b.CheckInDate &&
                                    b.Id != bookingId); 

                            if (isRoomBooked)
                            {
                                Console.WriteLine("Det här rummet är redan bokat under den perioden. Försök med ett annat datum.");
                                Console.WriteLine("\nTryck på valfri tangent för att fortsätta...");
                                Console.ReadKey(); 
                                continue;
                            }

                            
                            booking.CheckInDate = newCheckIn;
                            booking.CheckOutDate = newCheckOut;

                            
                            db.SaveChanges();
                            Console.WriteLine("Bokningen har uppdaterats.");

                            
                            Console.WriteLine("\nTryck på valfri tangent för att återgå till huvudmenyn...");
                            Console.ReadKey();
                            break; 
                        }
                    }
                    else if (bookingId == 0)
                    {
                        break; 
                    }
                    else
                    {
                        Console.WriteLine("Ogiltigt ID. Försök igen.");
                    }
                }
            }

            static void DeleteBookingCustomerRoom(ApplicationDbContext db)
            {
                Console.Clear();
                Console.WriteLine("Ta bort bokning, kund och rum.");
                Console.Write("Ange boknings-ID (eller tryck på '0' för att gå tillbaka): ");

                string input = Console.ReadLine();
                if (input == "0") return;

                if (!int.TryParse(input, out int bookingId))
                {
                    Console.WriteLine("Ogiltigt ID.");
                    Console.WriteLine("\nTryck på valfri tangent för att återgå till huvudmenyn...");
                    Console.ReadKey();
                    return;
                }

                var booking = db.Bookings.FirstOrDefault(b => b.Id == bookingId);
                if (booking == null)
                {
                    Console.WriteLine("Bokningen hittades inte.");
                    Console.WriteLine("\nTryck på valfri tangent för att återgå till huvudmenyn...");
                    Console.ReadKey();
                    return;
                }

                var customer = db.Customers.FirstOrDefault(c => c.Id == booking.CustomerId);
                var room = db.Rooms.FirstOrDefault(r => r.Id == booking.RoomId);

                if (customer == null || room == null)
                {
                    Console.WriteLine("Kund eller rum kopplad till bokningen hittades inte.");
                    Console.WriteLine("\nTryck på valfri tangent för att återgå till huvudmenyn...");
                    Console.ReadKey();
                    return;
                }

                
                bool customerHasOtherBookings = db.Bookings
                    .Any(b => b.CustomerId == customer.Id && b.Id != booking.Id);

                
                bool roomHasOtherBookings = db.Bookings
                    .Any(b => b.RoomId == room.Id && b.Id != booking.Id);

                
                if (!customerHasOtherBookings && !roomHasOtherBookings)
                {
                    db.Bookings.Remove(booking);
                    db.Customers.Remove(customer);
                    db.Rooms.Remove(room);

                    db.SaveChanges();
                    Console.WriteLine("Bokning, kund och rum har tagits bort.");
                }
                else if (!customerHasOtherBookings)
                {
                    db.Bookings.Remove(booking);
                    db.Customers.Remove(customer);

                    db.SaveChanges();
                    Console.WriteLine("Bokning och kund har tagits bort. Rummet finns kvar eftersom det används av andra bokningar.");
                }
                else if (!roomHasOtherBookings)
                {
                    db.Bookings.Remove(booking);
                    db.Rooms.Remove(room);

                    db.SaveChanges();
                    Console.WriteLine("Bokning och rum har tagits bort. Kunden finns kvar eftersom den har andra bokningar.");
                }
                else
                {
                    db.Bookings.Remove(booking);
                    db.SaveChanges();
                    Console.WriteLine("Endast bokningen har tagits bort. Kunden och rummet används i andra bokningar.");
                }

                Console.WriteLine("\nTryck på valfri tangent för att återgå till huvudmenyn...");
                Console.ReadKey();
            }

            

        }
    }
}