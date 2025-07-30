using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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

        public ActionResult<UserProfileDTO> GetUserProfile(int id)
        {
            var user = _repository.Get(id);

            return new UserProfileDTO
            {
                Id = id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Position = user.Position.Split(",").Select(int.Parse).ToList(),
                Role = user.Role,
                AvatarUrl = user.AvatarUrl,
                Status = user.Status
            };
        }

        public ActionResult Create(UserCreationDTO userCreationDTO)
        {
            if (string.IsNullOrWhiteSpace(userCreationDTO.Name) || string.IsNullOrWhiteSpace(userCreationDTO.Email) || userCreationDTO.Position.IsNullOrEmpty())
                return new BadRequestObjectResult("Nome, Email e Função são obrigatórios.");

            userCreationDTO.Password = _authService.GenerateHashPassword(userCreationDTO.Password);

            _repository.Create(userCreationDTO);
            return new OkResult();
        }

        public ActionResult UpdateProfile(UserProfileDTO profile)
        {
            var user = _repository.Get(profile.Id);

            user.Name = profile.Name;
            user.PhoneNumber = profile.PhoneNumber;
            user.Email = profile.Email;
            user.Position = string.Join(",", profile.Position);
            user.Role = profile.Role;
            user.AvatarUrl = profile.AvatarUrl;
            user.Status = profile.Status;

            _repository.Update(user);
            return new NoContentResult();
        }
    }
}
