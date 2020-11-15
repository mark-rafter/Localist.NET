using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Localist.Shared;

namespace Localist.Server.Helpers
{
    public static class MongoCollectionExtensions
    {
        // https://web.archive.org/web/20200928103657/https://kevsoft.net/2020/01/27/paging-data-in-mongodb-with-csharp.html
        public static async Task<(int totalPages, IReadOnlyList<TDocument> documentList)> AggregateByPage<TDocument>(
            this IMongoCollection<TDocument> collection,
            int page = 1,
            int pageSize = 50,
            FilterDefinition<TDocument>? filterDefinition = null,
            SortDefinition<TDocument>? sortDefinition = null)
            where TDocument : DbEntity
        {
            filterDefinition ??= Builders<TDocument>.Filter.Empty;
            sortDefinition ??= Builders<TDocument>.Sort.Descending(x => x.Id);

            var countFacet = AggregateFacet.Create("count",
                PipelineDefinition<TDocument, AggregateCountResult>.Create(new[]
                {
                    PipelineStageDefinitionBuilder.Count<TDocument>()
                }));

            var dataFacet = AggregateFacet.Create("data",
                PipelineDefinition<TDocument, TDocument>.Create(new[]
                {
                    PipelineStageDefinitionBuilder.Sort(sortDefinition),
                    PipelineStageDefinitionBuilder.Skip<TDocument>((page - 1) * pageSize),
                    PipelineStageDefinitionBuilder.Limit<TDocument>(pageSize),
                }));


            var aggregation = await collection.Aggregate()
                .Match(filterDefinition)
                .Facet(countFacet, dataFacet)
                .ToListAsync();

            var count = aggregation.First()
                .Facets.First(x => x.Name == "count")
                .Output<AggregateCountResult>()
                //.First()
                .Count;

            var totalPages = (int)Math.Ceiling((double)count / pageSize);

            var documentList = aggregation.First()
                .Facets.First(x => x.Name == "data")
                .Output<TDocument>();

            return (totalPages, documentList);
        }

        public static async Task<TDocument?> FirstOrDefaultAsync<TDocument>(
            this IMongoCollection<TDocument> collection,
            string id)
            where TDocument : DbEntity
        {
            return (await collection.FindAsync(p => p.Id == id)).FirstOrDefault();
        }

        public static async Task<TDocument?> SingleOrDefaultAsync<TDocument>(
            this IMongoCollection<TDocument> collection,
            string id)
            where TDocument : DbEntity
        {
            return (await collection.FindAsync(p => p.Id == id)).SingleOrDefault();
        }

        public static async Task<TDocument> Upsert<TDocument>(
            this IMongoCollection<TDocument> collection,
            TDocument entity)
            where TDocument : DbEntity
        {
            if (entity.Id is null)
            {
                var insertedEntity = entity with { CreatedOn = DateTimeOffset.UtcNow };
                await collection.InsertOneAsync(insertedEntity);
                return insertedEntity;
            }
            else
            {
                var updatedEntity = entity with { ModifiedOn = DateTimeOffset.UtcNow };
                var result = await collection.ReplaceOneAsync(x => x.Id == entity.Id, updatedEntity);
                // todo: check result.IsAcknowledged and throw on failure
                return updatedEntity;
            }
        }
    }
}