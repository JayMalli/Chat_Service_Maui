namespace chat_service_maui;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
		
		// call function for get chats...
		// string access_token="Your_user_access_token";
		// string refresh_tokenn="Your_user_refresh_token";
		// ChatService ChatService=new ChatService(access_token,refresh_token);
		// chatService.GetChats();
	}
}

