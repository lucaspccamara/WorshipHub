﻿using Dapper;
using WorshipDomain.Entities;
using WorshipDomain.Repository;
using WorshipInfra.Database;
using WorshipInfra.Database.Interfaces;

namespace WorshipInfra.Repository
{
    public class UsuarioRepository : GenericRepository<int, Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(IContextRepository dbContext) : base(dbContext) { }

        public string GetSenhaHashPorEmail(string email)
        {
            const string Sql = @"
                SELECT Senha
                FROM Usuario
                WHERE Email = @Email;";

            return _dbConnection.QuerySingle<string>(Sql, new { Email = email});
        }
    }
}
