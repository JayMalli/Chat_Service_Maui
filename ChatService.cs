using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.HangoutsChat.v1;
using Google.Apis.HangoutsChat.v1.Data;
using Google.Apis.PeopleService.v1;
using Google.Apis.PeopleService.v1.Data;
using Google.Apis.Services;

namespace chat_service_maui
{
    public class ChatService
    {
        public HangoutsChatService _hangoutsChatService { get; set; }
        public PeopleServiceService _peopleService { get; set; }
        public ChatService(string access_token, string refresh_token)
        {
            UserCredential userCredential = GenerateCredentialFromAccessToken(access_token, refresh_token);

            _peopleService = new PeopleServiceService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredential,
                ApplicationName = "YOUR_APP_NAME",
            });
            _hangoutsChatService = new HangoutsChatService(new BaseClientService.Initializer() { HttpClientInitializer = userCredential, ApplicationName = "YOUR_APP_NAME" });

        }
        private UserCredential GenerateCredentialFromAccessToken(string accessToken, string refreshToken)
        {
            UserCredential credential;
            string[] Scopes = { PeopleServiceService.Scope.Contacts, PeopleServiceService.Scope.ContactsOtherReadonly, HangoutsChatService.Scope.ChatSpaces, HangoutsChatService.Scope.ChatMessages };

            TokenResponse tokenResponse = new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            var initializer = new GoogleAuthorizationCodeFlow.Initializer
            {
                // get from gcp console under  API & Services >> Credentials section
                ClientSecrets = new ClientSecrets
                {
                    ClientId = "your_app_client_id",
                    ClientSecret = "your_app_client_secret"
                },
                Scopes = Scopes,
            };

            credential = new UserCredential(new GoogleAuthorizationCodeFlow(initializer), userId: "user", tokenResponse);

            return credential;
        }

        private List<Dictionary<string, string>> GetAllSpaces()
        {
            List<Dictionary<string, string>> spaces = new List<Dictionary<string, string>>();
            try
            {
                SpacesResource.ListRequest req = _hangoutsChatService.Spaces.List();
                req.Fields = "spaces(name,displayName,spaceType)";
                ListSpacesResponse res = req.Execute();
                if (res.Spaces.Count > 0)
                {
                    foreach (Space space in res.Spaces)
                    {
                        Dictionary<string, string> dict = new Dictionary<string, string>();
                        dict.Add("id", space.Name);
                        dict.Add("displayName", space.DisplayName);
                        dict.Add("type", space.SpaceType);
                        spaces.Add(dict);
                    }
                }

            }
            catch (GoogleApiException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return spaces;
        }

        public List<Dictionary<string, string>> GetChatsOfSpace(string spaceId)
        {
            List<Dictionary<string, string>> chats = new List<Dictionary<string, string>>();
            try
            {
                SpacesResource.MessagesResource.ListRequest req = _hangoutsChatService.Spaces.Messages.List(spaceId);
                req.Fields = "messages(sender,text,name,thread)";
                ListMessagesResponse res = req.Execute();
                if (res.Messages != null)
                {
                    foreach (Message msg in res.Messages)
                    {
                        Dictionary<string, string> dict = new Dictionary<string, string>();
                        dict.Add("threadId", msg.Thread.Name);
                        dict.Add("text", msg.Text);
                        string senderName = GetSenderOfChat(msg.Sender.Name);
                        dict.Add("senderName", senderName);
                        chats.Add(dict);
                    }
                }

                return chats;

            }
            catch (GoogleApiException ex)
            {
                Console.WriteLine(ex.Message);
                return chats;
            }

        }

        private string GetSenderOfChat(string senderId)
        {
            string resourceName = senderId.Replace("users/", "people/");
            PeopleResource.GetRequest req = _peopleService.People.Get(resourceName);
            req.PersonFields = "names";
            Person person = req.Execute();
            if (person?.Names != null && person.Names.Count > 0)
            {
                return person.Names[0].DisplayName;
            }

            return "Unknown User";

        }

        public void GetChats()
        {
            // get all spaces
            List<Dictionary<string, string>> spaces = GetAllSpaces();
            foreach (Dictionary<string, string> space in spaces)
            {
                List<Dictionary<string, string>> chats = GetChatsOfSpace(space["id"]);
            }

        }   

    }

}