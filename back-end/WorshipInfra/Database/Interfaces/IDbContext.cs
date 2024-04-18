using System.Data;

namespace WorshipInfra.Database.Interfaces
{
    public interface IDbContext
    {
        public IDbConnection Connection { get; }
    }
}
