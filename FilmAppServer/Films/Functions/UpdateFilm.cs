using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FilmAppServer.Films.Functions
{
    public class UpdateFilm : FilmFunction
    {
        public UpdateFilm() : base() {}
        public UpdateFilm(IFilmRepository repository) : base(repository) {}

        /// <summary>
        /// A Lambda function that updates a film.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override async Task<APIGatewayProxyResponse> PerformAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string filmId = GetFilmIdFromRequest(request);

            if (string.IsNullOrEmpty(filmId))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = $"Missing required parameter {ID_QUERY_STRING_NAME}",
                    Headers = this.GetResponseHeaders("text/plain")
                };
            }
            var filmData = JsonConvert.DeserializeObject<Film>(request?.Body);

            var film = await this.Repository.GetByIdAsync(filmId);
            if (film == null)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Headers = this.GetResponseHeaders()
                };
            }

            film.Title = filmData.Title;
            film.Director = filmData.Director;
            film.ReleaseDate = filmData.ReleaseDate;
            film.Rating = filmData.Rating;

            context.Logger.LogLine($"Updating film with id {filmId}");
            await this.Repository.UpdateAsync(film);

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Headers = this.GetResponseHeaders()
            };
        }
    }
}
