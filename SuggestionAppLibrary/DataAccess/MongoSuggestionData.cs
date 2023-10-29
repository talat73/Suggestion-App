namespace SuggestionAppLibrary.DataAccess;

public class MongoSuggestionData : ISuggestionData {
	private readonly IDbConnection _db;
	private readonly IUserData userData;
	private readonly IMemoryCache cache;

	private readonly IMongoCollection<SuggestionModel> Suggetions;
	private const string CacheName = "SuggestionData";
	public MongoSuggestionData (IDbConnection db, IUserData userData, IMemoryCache cache)
	{
		this._db = db;
		this.userData = userData;
		this.cache = cache;
		Suggetions = db.SuggestionCollection;
	}

	public async Task<List<SuggestionModel>> GetAllSuggestions ()
	{
		var output = cache.Get<List<SuggestionModel>> (CacheName);
		if (output is null) {
			var results = await Suggetions.FindAsync (s => s.Archived == false);
			output = results.ToList ();

			cache.Set (CacheName, output, TimeSpan.FromMinutes (1));
		}

		return output;
	}

	public async Task<List<SuggestionModel>> GetUserSuggestions  (string userId)
	{
		var output = cache.Get<List<SuggestionModel>> (userId);
		if(output is null) {
			var results = await Suggetions.FindAsync (s => s.Author.Id == userId);
			output = results.ToList ();

			cache.Set (userId, output, TimeSpan.FromMinutes (1));
		}

		return output;
	}

	public async Task<List<SuggestionModel>> GetAllApprovedSuggestions ()
	{
		var output = await GetAllSuggestions ();
		return output.Where (s => s.ApprovedForRelease).ToList ();
	}

	public async Task<SuggestionModel> GetSuggestion (string id)
	{
		var result = await Suggetions.FindAsync (s => s.Id == id);
		return result.FirstOrDefault ();
	}

	public async Task<List<SuggestionModel>> GetAllSuggestionsWaitingForApproval ()
	{
		var output = await GetAllSuggestions ();
		return output.Where (s =>
		    s.ApprovedForRelease == false
		    && s.Rejeted == false)
		    .ToList ();
	}

	public async Task UpdateSuggestion (SuggestionModel suggestion)
	{
		await Suggetions.ReplaceOneAsync (s => s.Id == suggestion.Id, suggestion);
		cache.Remove (CacheName);
	}

	public async Task UpvoteSuggestion (string suggestionId, string userId)
	{
		var client = _db.Client;

		using var session = await client.StartSessionAsync ();

		//session.StartTransaction ();
		try {
			var db = client.GetDatabase (_db.DbName);
			var suggestionsInTransaction = db.GetCollection<SuggestionModel> (_db.SuggestionCollectionName);
			var suggestion = (await suggestionsInTransaction.FindAsync (s => s.Id == suggestionId)).First ();

			bool isUpvote = suggestion.UserVotes.Add (userId);
			if (isUpvote == false) {
				suggestion.UserVotes.Remove (userId);
			}
			await suggestionsInTransaction.ReplaceOneAsync (session, s => s.Id == suggestionId, suggestion);

			var userInTransaction = db.GetCollection<UserModel> (_db.UserCollectionName);
			var user = await userData.GetUser (userId);

			if (isUpvote) {
				user.VotedOnSuggestions.Add (new BasicSuggestionModel (suggestion));
			} else {
				var suggestionToRemove = user.VotedOnSuggestions.Where (s => s.Id == suggestionId).First ();
				user.VotedOnSuggestions.Remove (suggestionToRemove);
			}

			await userInTransaction.ReplaceOneAsync (session, u => u.Id == userId, user);

			//await session.CommitTransactionAsync ();

			cache.Remove (CacheName);
		} catch (Exception ex) {
			//await session.AbortTransactionAsync ();
			throw;
		}
	}

	public async Task CreateSuggestion (SuggestionModel suggestion)
	{
		var client = _db.Client;

		using var session = await client.StartSessionAsync();

		//session.StartTransaction();

		try {
			var db = client.GetDatabase (_db.DbName);
			var suggestionInTransaction = db.GetCollection<SuggestionModel> (_db.SuggestionCollectionName);
			await suggestionInTransaction.InsertOneAsync (session, suggestion);

			var usersInTransaction = db.GetCollection<UserModel> (_db.UserCollectionName);
			var user = await userData.GetUser (suggestion.Author.Id);
			user.AuthoredSuggestions.Add (new BasicSuggestionModel (suggestion));
			await usersInTransaction.ReplaceOneAsync (session, u => u.Id == user.Id, user);

			//await session.CommitTransactionAsync();
		} catch (Exception ex) {
			//await session.AbortTransactionAsync();
			throw;
		}
	}
}

