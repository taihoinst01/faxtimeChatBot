using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

using System.Diagnostics;

namespace SecCsChatBotDemo
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            //HttpResponseMessage response;

            if (activity.Type == ActivityTypes.ConversationUpdate)
            {   
                //  && activity.MembersAdded.Any(m => m.Id == activity.Recipient.Id)
                Debug.WriteLine("* activity.Type : " + activity.Type);
                Debug.WriteLine("* ActivityTypes.ConversationUpdate : " + ActivityTypes.ConversationUpdate);
                //Debug.WriteLine("* m => m.Id : " + m => m.Id);
                Debug.WriteLine("* activity.Recipient.Id : " + activity.Recipient.Id);
                Debug.WriteLine("* activity.ServiceUrl : " + activity.ServiceUrl);
            }
            else if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
                Debug.WriteLine("* activity.Type == ActivityTypes.Message ");

                string orgMent = "";
                orgMent = activity.Text;
                Debug.WriteLine("* orgMent : "+ orgMent);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}