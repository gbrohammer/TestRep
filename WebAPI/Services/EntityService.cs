using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;
using MongoDB.Bson;
using MongoDB.Driver;
using WebAPI.Models;

namespace WebAPI.Services;

public class EntityService
{
    const string DBNAME = "test";
    const string COLLECTIONNAME = "entities";
    const int TENANTID = 270;

    private readonly IConfiguration _configuration;
    private readonly MongoClient _mongoClient;

    public EntityService(IConfiguration configuration)
    {
        _configuration = configuration;
        var connectionString = _configuration.GetConnectionString("MongoDBConnection");
        _mongoClient = new MongoClient(connectionString);
    }

    public async Task<IQueryable<Entity>> GetQueryable(ODataQueryOptions<Entity> queryOptions)
    {
        var collection = _mongoClient.GetDatabase(DBNAME).GetCollection<BsonDocument>(COLLECTIONNAME);

        // TODO: Add repository pattern/middleware/authentication with maybe some context info to limit tenantIds on all queries.
        // based on the user context
        // for now it's just hard-coded to filter by mapped tenantId/farmID
        var filter = Builders<BsonDocument>.Filter.Eq("tenantId", TENANTID);

        // TODO: Add OData filtering
        // Not limiting because for now OData filtering is happening in memory after the query execution and data will be partially filtered
        // breaking grouping and aggregations later in memory.
        // var find = collection.Find(filter);
        // if (queryOptions.Top != null)
        // {
        //     find = find.Limit(queryOptions.Top.Value);
        // }
        // if (queryOptions.Skip != null)
        // {
        //     find = find.Skip(queryOptions.Skip.Value);
        // }
        // var orderBy = queryOptions.OrderBy?.OrderByClause;
        // var filter = queryOptions.Filter?.FilterClause;
        // var projection = queryOptions.SelectExpand?.SelectExpandClause;
        // var aggregation = queryOptions.Apply?.ApplyClause;

        var documents = await collection
            .Find(filter)
            .ToListAsync();

        var entities = documents.Select(doc =>
        {
            return new Entity
            {
                Id = doc.GetValue("_id").AsObjectId.ToString(),
                TenantId = doc.GetValue("tenantId").AsInt32,
                Properties = ConvertBsonDocumentToDictionary(doc)
            };
        }).AsQueryable();

        return entities;
    }

    private Dictionary<string, object> ConvertBsonDocumentToDictionary(BsonDocument document, string? prefix = null)
    {
        var dictionary = new Dictionary<string, object>();
        foreach (var element in document.Elements)
        {
            // Skip the "_id" fields as they are already handled.
            if (element.Name == "_id" || element.Name == "tenantId")
            {
                continue;
            }

            var key = prefix != null ? $"{prefix}_{element.Name}" : element.Name;

            switch (element.Value?.BsonType)
            {
                case BsonType.ObjectId:
                    dictionary[key] = element.Value.AsObjectId.ToString();
                    break;
                case BsonType.String:
                    dictionary[key] = element.Value.AsString;
                    break;
                case BsonType.Int32:
                    dictionary[key] = element.Value.AsInt32;
                    break;
                case BsonType.Double:
                    dictionary[key] = element.Value.AsDouble;
                    break;
                case BsonType.Boolean:
                    dictionary[key] = element.Value.AsBoolean;
                    break;
                case BsonType.DateTime:
                    dictionary[key] = element.Value.ToUniversalTime().ToString("o");
                    break;
                case BsonType.Document:
                    var subDocument = element.Value.AsBsonDocument;
                    var subDictionary = ConvertBsonDocumentToDictionary(subDocument, key);
                    foreach (var kvp in subDictionary)
                    {
                        dictionary[kvp.Key] = kvp.Value;
                    }
                    break;
                default:
#pragma warning disable CS8601 // Possible null reference assignment.
                    dictionary[key] = element.Value?.ToString() ?? null;
#pragma warning restore CS8601 // Possible null reference assignment.
                    break;
            }
        }

        return dictionary;
    }

    public async Task<EdmModel?> GetSchema()
    {
        var collection = _mongoClient.GetDatabase(DBNAME).GetCollection<BsonDocument>(COLLECTIONNAME);

        // Get the first document to infer the schema
        var document = await collection.Find(new BsonDocument()).FirstOrDefaultAsync();

        if (document == null) return null;
        var model = new EdmModel();
        var entity = new EdmComplexType("WebApiDocNS", nameof(Entity));

        foreach (var element in document.Elements)
        {
            if (element.Name == "_id")
            {
                continue;
            }

            switch (element.Value?.BsonType)
            {
                case BsonType.Int32:
                    entity.AddStructuralProperty(element.Name, EdmPrimitiveTypeKind.Int32);
                    break;
                case BsonType.Double:
                    entity.AddStructuralProperty(element.Name, EdmPrimitiveTypeKind.Double);
                    break;
                case BsonType.Boolean:
                    entity.AddStructuralProperty(element.Name, EdmPrimitiveTypeKind.Boolean);
                    break;
                default:
                    entity.AddStructuralProperty(element.Name, EdmPrimitiveTypeKind.String);
                    break;
            }
        }

        model.AddElement(entity);

        return model;
    }
}