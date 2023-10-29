namespace SuggestionAppLibrary.DataAccess;

public class MongoStatusData : IStatusData
{
    private readonly IMongoCollection<StatusModel> Statuses;
    private readonly IDbConnection db;
    private readonly IMemoryCache cache;
    private const string CacheName = "StatusData";

    public MongoStatusData(IDbConnection db, IMemoryCache cache)
    {
        this.cache = cache;
        Statuses = db.StatusCollection;
    }

    public async Task<List<StatusModel>> GetAllStatuses()
    {
        var output = cache.Get<List<StatusModel>>(CacheName);

        if (output is null)
        {
            var results = await Statuses.FindAsync(_ => true);
            output = results.ToList();

            cache.Set(CacheName, output, TimeSpan.FromDays(1));
        }

        return output;
    }

    public Task CreateStatus(StatusModel status)
    {
        return Statuses.InsertOneAsync(status);
    }
}

