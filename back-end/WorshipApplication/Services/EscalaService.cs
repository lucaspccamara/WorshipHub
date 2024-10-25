using FluentResults;
using WorshipDomain.Core.Entities;
using WorshipDomain.DTO.Escala;
using WorshipDomain.Entities;
using WorshipDomain.Repository;


namespace WorshipApplication.Services
{
    public class EscalaService : ServiceBase<int, Escala, IEscalaRepository>
    {
        public EscalaService(IEscalaRepository repository) : base(repository)
        {
        }

        public Result<string> CreateEscala(IEnumerable<EscalaCreationDTO> escalasCreationDTO)
        {
            int cadastradas = 0;
            List<string> mensagensErro = new List<string>();

            foreach (var escalaCreationDTO in escalasCreationDTO)
            {
                if (!DateTime.TryParse(escalaCreationDTO.Data, out DateTime data))
                {
                    mensagensErro.Add($"Data inválida: {escalaCreationDTO.Data}");
                    continue;
                }

                if (_repository.ExisteEscala(data, escalaCreationDTO.Evento))
                {
                    mensagensErro.Add($"Já existe uma escala para a data {data.ToShortDateString()} e evento {escalaCreationDTO.Evento}.");
                    continue;
                }

                var escala = new Escala
                {
                    Data = data,
                    Evento = escalaCreationDTO.Evento,
                    Liberada = false
                };

                try
                {
                    _repository.Insert(escala);
                    cadastradas++;
                }
                catch (Exception ex)
                {
                    mensagensErro.Add($"Erro ao inserir a escala: {ex.Message}");
                }
            }

            string resultadoMensagem = cadastradas > 0
                ? $"{cadastradas} escalas criadas com sucesso."
                : "Nenhuma escala foi cadastrada.";

            if (mensagensErro.Count > 0)
            {
                resultadoMensagem += "\nErros: " + string.Join(", ", mensagensErro);
            }

            return Result.Ok(resultadoMensagem);
        }

        public ResultFilter<EscalaOverviewDTO> GetListPaged(ApiRequest<EscalaFilterDTO> request)
        {
            return _repository.GetListPaged(request);
        }
    }
}
