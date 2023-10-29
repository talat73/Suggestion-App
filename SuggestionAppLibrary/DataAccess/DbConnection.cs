using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace SuggestionAppLibrary.DataAccess;

public class DbConnection : IDbConnection
{
    private readonly IConfiguration Config;
    private readonly IMongoDatabase Db;
    private string ConnectionId = "MongoDB";
    public string DbName
    {
        get;
        private set;
    }
    public string CategoryCollectionName
    {
        get;
        private set;
    } = "categories";
    public string StatusCollectionName
    {
        get;
        private set;
    } = "statuses";
    public string UserCollectionName
    {
        get;
        private set;
    } = "users";
    public string SuggestionCollectionName
    {
        get;
        private set;
    } = "suggestions";

    public MongoClient Client
    {
        get;
        private set;
    }
    public IMongoCollection<CategoryModel> CategoryCollection
    {
        get;
        private set;
    }
    public IMongoCollection<StatusModel> StatusCollection
    {
        get;
        private set;
    }
    public IMongoCollection<UserModel> UserCollection
    {
        get;
        private set;
    }
    public IMongoCollection<SuggestionModel> SuggestionCollection
    {
        get;
        private set;
    }


    public DbConnection(IConfiguration config)
    {
        Config = config;
        Client = new MongoClient(Config.GetConnectionString(ConnectionId));
        DbName = Config["DatabaseName"];
        Db = Client.GetDatabase(DbName);

        CategoryCollection = Db.GetCollection<CategoryModel>(CategoryCollectionName);
        StatusCollection = Db.GetCollection<StatusModel>(StatusCollectionName);
        UserCollection = Db.GetCollection<UserModel>(UserCollectionName);
        SuggestionCollection = Db.GetCollection<SuggestionModel>(SuggestionCollectionName);
    }
}

