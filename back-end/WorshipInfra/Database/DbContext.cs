using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Data;
using WorshipInfra.Database.Interfaces;

namespace WorshipInfra.Database
{
    public class DbContext : IDbContext
    {
        private readonly string ConnectionString;

        public IDbConnection Connection => new MySqlConnection(ConnectionString);

        public DbContext(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("mysql");
        }
    }
}
