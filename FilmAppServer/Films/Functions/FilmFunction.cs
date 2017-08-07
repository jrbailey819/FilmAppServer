using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace FilmAppServer.Films.Functions
{
    public abstract class FilmFunction
    {
        protected IFilmRepository Repository { get; set; }
        public const string ID_QUERY_STRING_NAME = "Id";

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public FilmFunction()
        {
            this.Repository = new FilmRepository();
        }

        /// <summary>
        /// Constructor used for testing passing in a mock repository
        /// </summary>
        /// <param name="ddbClient"></param>
        /// <param name="tableName"></param>
        public FilmFunction(IFilmRepository repository)
        {
            this.Repository = repository;
        }

        public abstract Task<APIGatewayProxyResponse> PerformAsync(APIGatewayProxyRequest request, ILambdaContext context);

        protected string GetFilmIdFromRequest(APIGatewayProxyRequest request)
        {
            string filmId = null;
            if (request.PathParameters != null && request.PathParameters.ContainsKey(ID_QUERY_STRING_NAME))
                filmId = request.PathParameters[ID_QUERY_STRING_NAME];
            else if (request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey(ID_QUERY_STRING_NAME))
                filmId = request.QueryStringParameters[ID_QUERY_STRING_NAME];

            return filmId;
        }

        protected Dictionary<string, string> GetResponseHeaders(string contentType = null)
        {
            var headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" }
            };

            if (!string.IsNullOrWhiteSpace(contentType))
            {
                headers["Content-Type"] = contentType;

            }

            return headers;
        }

    }
}
