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
    public class GetFilm : FilmFunction
    {
        public GetFilm() : base() {}
        public GetFilm(IFilmRepository repository) : base(repository) {}

        /// <summary>
        /// A Lambda function that returns the film identified by filmId
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
                    Headers = this.GetResponseHeaders("test/plain")
                };
            }

            context.Logger.LogLine($"Getting film {filmId}");
            var film = await this.Repository.GetByIdAsync(filmId);
            context.Logger.LogLine($"Found film: {film != null}");

            if (film == null)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Headers = this.GetResponseHeaders()
                };
            }

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(film),
                Headers = this.GetResponseHeaders("application/json")
            };
            return response;
        }


    }
}
