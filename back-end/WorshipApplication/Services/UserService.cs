using Microsoft.AspNetCore.Mvc;
using WorshipDomain.Core.Entities;
using WorshipDomain.DTO.Schedule;
using WorshipDomain.DTO.User;
using WorshipDomain.Entities;
using WorshipDomain.Repository;

namespace WorshipApplication.Services
{
    public class UserService : ServiceBase<int, User, IUserRepository>
    {
        private readonly AuthService _authService;
        public UserService(IUserRepository repository, AuthService authService) : base(repository)
        {
            _authService = authService;
        }

        public ResultFilter<UserOverviewDTO> GetListPaged(ApiRequest<UserFilterDTO> request)
        {
            return _repository.GetListPaged(request);
        }

        public ActionResult Create(UserCreationDTO userCreationDTO)
        {
            if (string.IsNullOrWhiteSpace(userCreationDTO.Name) || string.IsNullOrWhiteSpace(userCreationDTO.Email))
                return new BadRequestObjectResult("Nome e Email são obrigatórios.");

            userCreationDTO.Password = _authService.GenerateHashPassword(userCreationDTO.Password);

            _repository.Create(userCreationDTO);
            return new OkResult();
        }
    }
}
