using System.Data;
using WorshipInfra.Database.Interfaces;

namespace WorshipInfra.Database
{
    public abstract class GenericRepository
    {
        protected readonly IDbConnection _dbConnection;

        protected GenericRepository(IDbContext dbContext) {  _dbConnection = dbContext.Connection; }
    }
}
