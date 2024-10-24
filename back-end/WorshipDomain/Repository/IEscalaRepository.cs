using WorshipDomain.Core.Interfaces;
using WorshipDomain.Entities;
using WorshipDomain.Enums;

namespace WorshipDomain.Repository
{
    public interface IEscalaRepository : IGenericRepository<int, Escala>
    {
        bool ExisteEscala(DateTime data, Evento evento);
    }
}
