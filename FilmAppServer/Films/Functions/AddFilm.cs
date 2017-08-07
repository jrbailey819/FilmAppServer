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
    public class AddFilm : FilmFunction
    {
        public AddFilm() : base() {}
        public AddFilm(IFilmRepository repository) : base(repository) {}

        /// <summary>
        /// A Lambda function that adds a film.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override async Task<APIGatewayProxyResponse> PerformAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var film = JsonConvert.DeserializeObject<Film>(request?.Body);
            film.Id = Guid.NewGuid().ToString();
            film.CreatedTimestamp = DateTime.Now;

            context.Logger.LogLine($"Saving film with id {film.Id}");
            await this.Repository.AddAsync(film);

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = film.Id.ToString(),
                Headers = this.GetResponseHeaders("text/plain")
            };
            return response;
        }


    }
}
