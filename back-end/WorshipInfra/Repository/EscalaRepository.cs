using Dapper;
using WorshipDomain.Core.Entities;
using WorshipDomain.DTO.Escala;
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

        public ResultFilter<EscalaOverviewDTO> GetListPaged(ApiRequest<EscalaFilterDTO> request)
        {
            var builder = new SqlBuilder();

            var selector = builder.AddTemplate($@"
SELECT SQL_CALC_FOUND_ROWS
    data, evento
FROM escala
/**WHERE**/
/**ORDERBY**/
LIMIT {(request.Page - 1) * request.Length}, {request.Length};

SELECT FOUND_ROWS() AS TotalRecords;");

            if (DateTime.TryParse(request.Filters.DataInicio, out var dataInicio))
                builder.Where("data >= @dataInicio", new { dataInicio });

            if (DateTime.TryParse(request.Filters.DataFim, out var dataFim))
                builder.Where("data <= @dataFim", new { dataFim });

            builder.Where("evento = @evento AND evento < 10", new { evento = request.Filters.Evento });

            builder.OrderBy(request.GetSorting("data"));

            IEnumerable<EscalaOverviewDTO> escalaOverviewDTO = null;
            int count = 0;

            using (var multiReader = _dbConnection.QueryMultiple(selector.RawSql, selector.Parameters))
            {
                var escalaList = multiReader.Read<(DateTime Data, int Evento)>();
                escalaOverviewDTO = escalaList.Select(escala => new EscalaOverviewDTO
                {
                    Data = escala.Data.ToString("dd/MM/yy"),
                    Evento = escala.Evento
                });

                count = multiReader.ReadSingle<int>();
            }

            var resultFilter = new ResultFilter<EscalaOverviewDTO>
            {
                Data = escalaOverviewDTO,
                TotalRecords = count
            };

            return resultFilter;
        }
    }
}
