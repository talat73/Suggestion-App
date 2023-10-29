namespace SuggestionAppLibrary.DataAccess;

public class MongoUserData : IUserData
{

    private readonly IMongoCollection<UserModel> Users;

    public MongoUserData(IDbConnection db)
    {
        Users = db.UserCollection;
    }

    public async Task<List<UserModel>> GetUsersAsync()
    {
        var result = await Users.FindAsync(_ => true);
        return result.ToList();
    }

    public async Task<UserModel> GetUser(string id)
    {
        var result = await Users.FindAsync(u => u.Id == id);
        return result.FirstOrDefault();
    }

    public async Task<UserModel> GetUserFromAuthentication(string objectId)
    {
        var result = await Users.FindAsync(u => u.ObjectIdentifier == objectId);
        return result.FirstOrDefault();
    }

    public Task CreateUser(UserModel user)
    {
        return Users.InsertOneAsync(user);
    }

    public Task UpdateUser(UserModel user)
    {
        var filter = Builders<UserModel>.Filter.Eq("Id", user.Id);
        return Users.ReplaceOneAsync(filter, user, new ReplaceOptions { IsUpsert = true });
    }
}

