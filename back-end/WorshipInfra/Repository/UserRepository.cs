using Dapper;
using WorshipDomain.Core.Entities;
using WorshipDomain.DTO.User;
using WorshipDomain.Entities;
using WorshipDomain.Repository;
using WorshipInfra.Database;
using WorshipInfra.Database.Interfaces;

namespace WorshipInfra.Repository
{
    public class UserRepository : GenericRepository<int, User>, IUserRepository
    {
        public UserRepository(IContextRepository dbContext) : base(dbContext) { }

        public ResultFilter<UserOverviewDTO> GetListPaged(ApiRequest<UserFilterDTO> request)
        {
            var builder = new SqlBuilder();

            var selector = builder.AddTemplate($@"
SELECT SQL_CALC_FOUND_ROWS
    id, name, email, status, CASE WHEN fcm_token IS NOT NULL AND fcm_token != '' THEN TRUE ELSE FALSE END AS HasPushNotification
FROM users
/**where**/
/**orderby**/
LIMIT {(request.Page - 1) * request.Length}, {request.Length};

SELECT FOUND_ROWS() AS TotalRecords;");

            if (!string.IsNullOrEmpty(request.Filters.Name))
                builder.Where("name LIKE @name", new { name = request.Filters.Name });

            if (!string.IsNullOrEmpty(request.Filters.Email))
                builder.Where("email LIKE @email", new { email = request.Filters.Email });

            if (request.Filters.Status.HasValue)
                builder.Where("status = @status", new { status = request.Filters.Status });
            else
                builder.Where("status = TRUE");

            builder.OrderBy(request.GetSorting("name"));

            IEnumerable<UserOverviewDTO> userOverviewDTO;
            int count = 0;

            using (var multiReader = _dbConnection.QueryMultiple(selector.RawSql, selector.Parameters))
            {
                var usersList = multiReader.Read<(int Id, string Name, string Email, bool Status, bool HasPushNotification)>();
                userOverviewDTO = usersList.Select(user => new UserOverviewDTO
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Status = user.Status,
                    HasPushNotification = user.HasPushNotification
                });

                count = multiReader.ReadSingle<int>();
            }

            var resultFilter = new ResultFilter<UserOverviewDTO>
            {
                Data = userOverviewDTO,
                TotalRecords = count
            };

            return resultFilter;
        }

        public void Create(UserCreationDTO userCreationDTO)
        {
            var sql = @"INSERT INTO users (name, email, position, password) VALUES (@Name, @Email, @Position, @Password);";
            _dbConnection.Execute(sql, new
            {
                userCreationDTO.Name,
                userCreationDTO.Email,
                Position = string.Join(",", userCreationDTO.Position),
                userCreationDTO.Password
            });
        }

        public void UpdateTimezone(int userId, string timezone)
        {
            var sql = @"UPDATE users SET timezone = @Timezone WHERE id = @Id;";
            _dbConnection.Execute(sql, new { Timezone = timezone, Id = userId });
        }
    }
}
