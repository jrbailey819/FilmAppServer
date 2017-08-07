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
    public class RemoveFilm : FilmFunction
    {
        public RemoveFilm() : base() {}
        public RemoveFilm(IFilmRepository repository) : base(repository) {}

        /// <summary>
        /// A Lambda function that removes a film from the repository.
        /// </summary>
        /// <param name="request"></param>
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

            context.Logger.LogLine($"Deleting film with id {filmId}");
            await this.Repository.DeleteAsync(filmId);

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Headers = this.GetResponseHeaders()
            };
        }
    }
}
