namespace SuggestionAppLibrary.DataAccess;

public class MongoCategoryData : ICategoryData
{
    private readonly IMongoCollection<CategoryModel> Categories;
    private readonly IMemoryCache cache;
    private const string CacheName = "CategoryData";

    public MongoCategoryData(IDbConnection db, IMemoryCache cache)
    {
        this.cache = cache;
        Categories = db.CategoryCollection;
    }

    public async Task<List<CategoryModel>> GetAllCategories()
    {
        var output = cache.Get<List<CategoryModel>>(CacheName);
        if (output is null)
        {
            var results = await Categories.FindAsync(_ => true);
            output = results.ToList();

            cache.Set(CacheName, output, TimeSpan.FromDays(1));
        }

        return output;
    }

    public Task CreateCategory(CategoryModel category)
    {
        return Categories.InsertOneAsync(category);
    }



}


