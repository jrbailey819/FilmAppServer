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
    public class GetFilms : FilmFunction
    {
        public GetFilms() : base() {}
        public GetFilms(IFilmRepository repository) : base(repository) {}

        /// <summary>
        /// A Lambda function that returns back a page worth of film posts.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The list of films</returns>
        public override async Task<APIGatewayProxyResponse> PerformAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Getting films");
            var films = await this.Repository.GetListAsync();
            context.Logger.LogLine($"Found {films.Count} films");

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(films),
                Headers = this.GetResponseHeaders("application/json")
            };

            return response;
        }

    }
}
