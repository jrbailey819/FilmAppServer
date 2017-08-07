using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FilmAppServer.Films
{
    public class FilmRepository : IFilmRepository
    {
        // This const is the name of the environment variable that the serverless.template will use to set
        // the name of the DynamoDB table used to store films.
        const string TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP = "FilmTable";

        IDynamoDBContext DDBContext { get; set; }

        #region Constructors
        public FilmRepository()
        {
            // Check to see if a table name was passed in through environment variables and if so 
            // add the table mapping.
            var tableName = System.Environment.GetEnvironmentVariable(TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP);
            if (!string.IsNullOrEmpty(tableName))
            {
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(Film)] = new Amazon.Util.TypeMapping(typeof(Film), tableName);
            }

            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            this.DDBContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        public FilmRepository(IAmazonDynamoDB ddbClient, string tableName)
        {
            if (!string.IsNullOrEmpty(tableName))
            {
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(Film)] = new Amazon.Util.TypeMapping(typeof(Film), tableName);
            }

            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            this.DDBContext = new DynamoDBContext(ddbClient, config);
        }

        #endregion  

        #region IFilmRepository Methods
        public async Task<List<Film>> GetListAsync()
        {
            var search = this.DDBContext.ScanAsync<Film>(null);
            var list = await search.GetNextSetAsync();

            return list;
        }

        public async Task<Film> GetByIdAsync(string id)
        {
            var film = await DDBContext.LoadAsync<Film>(id);

            return film;
        }

        public async Task AddAsync(Film film)
        {
            await DDBContext.SaveAsync<Film>(film);
        }

        public async Task UpdateAsync(Film film)
        {
            await DDBContext.SaveAsync<Film>(film);
        }

        public async Task DeleteAsync(string id)
        {
            await this.DDBContext.DeleteAsync<Film>(id);
        }

        #endregion
    }
}
