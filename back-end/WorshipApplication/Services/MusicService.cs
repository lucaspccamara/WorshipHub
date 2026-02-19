using Microsoft.AspNetCore.Mvc;
using WorshipDomain.Core.Entities;
using WorshipDomain.DTO.Music;
using WorshipDomain.Entities;
using WorshipDomain.Repository;

namespace WorshipApplication.Services
{
    public class MusicService : ServiceBase<int, Music, IMusicRepository>
    {
        public MusicService(IMusicRepository repository) : base(repository)
        {
        }

        public ResultFilter<MusicOverviewDTO> GetListPaged(ApiRequest<MusicFilterDTO> request)
        {
            return _repository.GetListPaged(request);
        }

        public ActionResult<MusicDTO> GetMusic(int id)
        {
            var music = _repository.Get(id);

            return new MusicDTO
            {
                Id = id,
                Title = music.Title,
                Artist = music.Artist,
                Album = music.Album,
                NoteBase = music.NoteBase,
                NoteMode = music.NoteMode,
                TimeSignature = music.TimeSignature,
                Duration = DurationSecondsToStringDuration(music.DurationSeconds),
                Bpm = music.Bpm,
                VideoUrl = music.VideoUrl,
                ImageUrl = music.ImageUrl
            };
        }

        public ActionResult Create(MusicCreationDTO musicCreationDTO)
        {
            if (string.IsNullOrWhiteSpace(musicCreationDTO.Title))
                return new BadRequestObjectResult("Título é obrigatório.");

            var music = new Music
            {
                Title = musicCreationDTO.Title,
                Artist = musicCreationDTO.Artist,
                Album = musicCreationDTO.Album,
                NoteBase = musicCreationDTO.NoteBase,
                NoteMode = musicCreationDTO.NoteMode,
                Bpm = musicCreationDTO.Bpm,
                TimeSignature = musicCreationDTO.TimeSignature,
                DurationSeconds = StringDurationToDurationSeconds(musicCreationDTO.Duration),
                VideoUrl = musicCreationDTO.VideoUrl,
                ImageUrl = musicCreationDTO.ImageUrl
            };

            _repository.Insert(music);
            return new OkResult();
        }

        public ActionResult Update(MusicDTO musicDto)
        {
            if (string.IsNullOrWhiteSpace(musicDto.Title))
                return new BadRequestObjectResult("Título é obrigatório.");

            var music = _repository.Get(musicDto.Id);

            music.Title = musicDto.Title;
            music.Artist = musicDto.Artist;
            music.Album = musicDto.Album;
            music.NoteBase = musicDto.NoteBase;
            music.NoteMode = musicDto.NoteMode;
            music.Bpm = musicDto.Bpm;
            music.VideoUrl = musicDto.VideoUrl;
            music.ImageUrl = musicDto.ImageUrl;

            _repository.Update(music);
            return new NoContentResult();
        }

        private int? StringDurationToDurationSeconds(string? duration)
        {
            if (string.IsNullOrWhiteSpace(duration))
                return null;

            var parts = duration.Split(':');

            try
            {
                if (parts.Length == 2)
                {
                    // Formato mm:ss
                    int minutes = int.Parse(parts[0]);
                    int seconds = int.Parse(parts[1]);
                    return (minutes * 60) + seconds;
                }
                else if (parts.Length == 3)
                {
                    // Formato hh:mm:ss
                    int hours = int.Parse(parts[0]);
                    int minutes = int.Parse(parts[1]);
                    int seconds = int.Parse(parts[2]);
                    return (hours * 3600) + (minutes * 60) + seconds;
                }
                else
                {
                    throw new FormatException("Formato inválido de duração. Use mm:ss ou hh:mm:ss.");
                }
            }
            catch (Exception)
            {
                throw new FormatException("Não foi possível converter a duração. Verifique se está no formato correto (mm:ss ou hh:mm:ss).");
            }
        }

        private string? DurationSecondsToStringDuration(int? durationSeconds)
        {
            if (!durationSeconds.HasValue)
                return null;

            int totalSeconds = durationSeconds.Value;

            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            // Se tiver horas, retorna no formato hh:mm:ss
            if (hours > 0)
                return $"{hours:D2}:{minutes:D2}:{seconds:D2}";

            // Caso contrário, apenas mm:ss
            return $"{minutes:D2}:{seconds:D2}";
        }
    }
}
