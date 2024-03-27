using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorshipDomain.Entities;

namespace WorshipDomain.Repository
{
    public interface IUsuarioRepository
    {
        Guid CadastrarUsuario(Usuario usuario);
        bool AutenticarUsuario(string usuario, string senha);
    }
}
