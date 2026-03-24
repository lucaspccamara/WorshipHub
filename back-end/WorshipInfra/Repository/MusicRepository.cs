using Dapper;
using WorshipDomain.Core.Entities;
using WorshipDomain.DTO.Music;
using WorshipDomain.Entities;
using WorshipDomain.Repository;
using WorshipInfra.Database;
using WorshipInfra.Database.Interfaces;

namespace WorshipInfra.Repository
{
    public class MusicRepository : GenericRepository<int, Music>, IMusicRepository
    {
        public MusicRepository(IContextRepository dbContext) : base(dbContext) { }

        public ResultFilter<MusicOverviewDTO> GetListPaged(ApiRequest<MusicFilterDTO> request)
        {
            var builder = new SqlBuilder();

            var selector = builder.AddTemplate($@"
SELECT SQL_CALC_FOUND_ROWS
    id, title, artist, album, note_base, note_mode, bpm
FROM musics
/**where**/
/**orderby**/
LIMIT {(request.Page - 1) * request.Length}, {request.Length};

SELECT FOUND_ROWS() AS TotalRecords;");

            if (!string.IsNullOrEmpty(request.Filters.Title))
                builder.Where("title LIKE @title", new { title = $"%{request.Filters.Title}%" });

            if (!string.IsNullOrEmpty(request.Filters.Artist))
                builder.Where("artist LIKE @artist", new { artist = $"%{request.Filters.Artist}%" });

            if (!string.IsNullOrEmpty(request.Filters.Album))
                builder.Where("album LIKE @album", new { album = $"%{request.Filters.Album}%" });

            builder.OrderBy(request.GetSorting("title"));

            IEnumerable<MusicOverviewDTO> musicOverviewDTO;
            int count = 0;

            using (var multiReader = _dbConnection.QueryMultiple(selector.RawSql, selector.Parameters))
            {
                var musicList = multiReader.Read<(int Id, string Title, string Artist, string Album, string? NoteBase, string? NoteMode, decimal? Bpm)>();
                musicOverviewDTO = musicList.Select(music => new MusicOverviewDTO
                {
                    Id = music.Id,
                    Title = music.Title,
                    Artist = music.Artist,
                    Album = music.Album,
                    NoteBase = music.NoteBase,
                    NoteMode = music.NoteMode,
                    Bpm = music.Bpm
                });

                count = multiReader.ReadSingle<int>();
            }

            var resultFilter = new ResultFilter<MusicOverviewDTO>
            {
                Data = musicOverviewDTO,
                TotalRecords = count
            };

            return resultFilter;
        }
    }
}
