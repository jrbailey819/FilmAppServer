using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FilmAppServer.Films
{
    public interface IFilmRepository
    {
        Task<List<Film>> GetListAsync();
        Task<Film> GetByIdAsync(string id);
        Task AddAsync(Film film);
        Task UpdateAsync(Film film);
        Task DeleteAsync(string id);
    }
}
