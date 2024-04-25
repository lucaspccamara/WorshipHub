﻿using WorshipDomain.Core.Entities;
using WorshipDomain.Entities;
using WorshipDomain.Repository;

namespace WorshipApplication.Services
{
    public class UsuarioService : ServiceBase<int, Usuario>
    {
        public UsuarioService(IUsuarioRepository repository) : base(repository)
        {
        }
    }
}
