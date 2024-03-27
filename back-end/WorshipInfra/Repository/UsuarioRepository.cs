using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorshipDomain.Entities;
using WorshipDomain.Repository;

namespace WorshipInfra.Repository
{
    public class UsuarioRepository : IUsuarioRepository
    {
        public bool AutenticarUsuario(string usuario, string senha)
        {
            if (usuario == "usuario" && senha == "senha")
                return true;
            return false;
        }

        public Guid CadastrarUsuario(Usuario usuario)
        {
            throw new NotImplementedException();
        }
    }
}
