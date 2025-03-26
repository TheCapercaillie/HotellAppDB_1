
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

        }
    }
}
