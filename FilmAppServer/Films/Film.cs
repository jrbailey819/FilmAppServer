using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FilmAppServer.Films
{
    public class Film
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Director { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedTimestamp { get; set; }
    }
}
