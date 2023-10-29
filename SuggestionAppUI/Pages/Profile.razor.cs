namespace SuggestionAppUI.Pages;

public partial class Profile {
	private UserModel loggedInUser;
	private List<SuggestionModel> submissions;
	private List<SuggestionModel> approved;
	private List<SuggestionModel> archived;
	private List<SuggestionModel> pending;
	private List<SuggestionModel> rejected;

	protected async override Task OnInitializedAsync ()
	{
		loggedInUser = await authProvider.GetUserFromAuth (userData);

		var results = await suggestionData.GetUserSuggestions (loggedInUser.Id);

		if (loggedInUser is not null && results is not null) {
			submissions = results.OrderByDescending (s => s.DateCreated).ToList ();
			approved = submissions.Where (s => s.ApprovedForRelease && s.Archived == false && s.Rejeted == false).ToList ();
			archived = submissions.Where (s => s.Archived && s.Rejeted == false).ToList ();
			pending = submissions.Where (s => s.ApprovedForRelease == false && s.Rejeted == false).ToList ();
			rejected = submissions.Where (s => s.Rejeted).ToList ();
		}
	}

	private void ClosePage ()
	{
		navManager.NavigateTo ("/");
	}

}

