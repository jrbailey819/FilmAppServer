using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace FilmAppServer.Films
{
    public class Functions
    {
        public const string ID_QUERY_STRING_NAME = "Id";
        IFilmRepository Repository { get; set; }

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
        {
            this.Repository = new FilmRepository();
        }

        /// <summary>
        /// Constructor used for testing passing in a preconfigured DynamoDB client.
        /// </summary>
        /// <param name="ddbClient"></param>
        /// <param name="tableName"></param>
        public Functions(IAmazonDynamoDB ddbClient, string tableName)
        {
            this.Repository = new FilmRepository(ddbClient, tableName);
        }

        /// <summary>
        /// A Lambda function that returns back a page worth of film posts.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The list of films</returns>
        public async Task<APIGatewayProxyResponse> GetFilmsAsync(APIGatewayProxyRequest request, ILambdaContext context)
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

        /// <summary>
        /// A Lambda function that returns the film identified by filmId
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<APIGatewayProxyResponse> GetFilmAsync(APIGatewayProxyRequest request, ILambdaContext context)
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

        /// <summary>
        /// A Lambda function that adds a film.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<APIGatewayProxyResponse> AddFilmAsync(APIGatewayProxyRequest request, ILambdaContext context)
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

        /// <summary>
        /// A Lambda function that updates a film.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<APIGatewayProxyResponse> UpdateFilmAsync(APIGatewayProxyRequest request, ILambdaContext context)
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

        /// <summary>
        /// A Lambda function that removes a film from the repository.
        /// </summary>
        /// <param name="request"></param>
        public async Task<APIGatewayProxyResponse> RemoveFilmAsync(APIGatewayProxyRequest request, ILambdaContext context)
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

        #region Private Methods

        private string GetFilmIdFromRequest(APIGatewayProxyRequest request)
        {
            string filmId = null;
            if (request.PathParameters != null && request.PathParameters.ContainsKey(ID_QUERY_STRING_NAME))
                filmId = request.PathParameters[ID_QUERY_STRING_NAME];
            else if (request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey(ID_QUERY_STRING_NAME))
                filmId = request.QueryStringParameters[ID_QUERY_STRING_NAME];

            return filmId;
        }

        private Dictionary<string, string> GetResponseHeaders(string contentType = null)
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

        #endregion
    }
}
