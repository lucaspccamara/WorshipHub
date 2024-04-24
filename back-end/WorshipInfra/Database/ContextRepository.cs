using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Data;
using WorshipInfra.Database.Interfaces;

namespace WorshipInfra.Database
{
    public class ContextRepository : IContextRepository
    {
        private readonly string ConnectionString;

        public IDbConnection Connection {  get; private set; }

        public IDbTransaction Transaction { get; private set; }

        public ContextRepository(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("mysql");
            Connection = new MySqlConnection(ConnectionString);
        }

        public void BeginTransaction()
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            Transaction = Connection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            Transaction.Commit();
            Transaction = null;
        }

        public void RollbackTransaction()
        {
            Transaction.Rollback();
            Transaction = null;
        }
    }
}
