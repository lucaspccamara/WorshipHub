using Dapper;
using WorshipDomain.Entities;
using WorshipDomain.Enums;
using WorshipDomain.Repository;
using WorshipInfra.Database;
using WorshipInfra.Database.Interfaces;

namespace WorshipInfra.Repository
{
    public class EscalaRepository : GenericRepository<int, Escala>, IEscalaRepository
    {
        public EscalaRepository(IContextRepository dbContext) : base(dbContext) { }

        public bool ExisteEscala(DateTime data, Evento evento)
        {
            var builder = new SqlBuilder();
            var selector = builder.AddTemplate(@"
SELECT 1
FROM escala
/**WHERE**/
;");
            
            builder.Where("data = @data", new { data });
            builder.Where("evento = @evento", new { evento = evento.GetHashCode() });

            return _dbConnection.ExecuteScalar<int>(selector.RawSql, selector.Parameters) > 0;
        }
    }
}
