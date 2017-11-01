﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using SecCsChatBotDemo.DB;
using SecCsChatBotDemo.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace SecCsChatBotDemo
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {

        public readonly string TEXTDLG = "2";
        public readonly string CARDDLG = "3";
        public readonly string MEDIADLG = "4";
        int userDataNum = 0;

        int sessionNo = 0;
        string[] strIntent = { };
        string[] strEntity = { };




        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            //HttpResponseMessage response;

            var intentList = new List<string>();
            var entityList = new List<string>();

            if (activity.Type == ActivityTypes.ConversationUpdate && activity.MembersAdded.Any(m => m.Id == activity.Recipient.Id))
            {
                DateTime startTime = DateTime.Now;
                Debug.WriteLine("* DB conn : " + activity.Type);
                //Db
                DbConnect db = new DbConnect();
                List<DialogList> dlg = db.SelectInitDialog();
                

                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                Debug.WriteLine("* dlg.Count : " + dlg.Count);
                for (int n = 0; n < dlg.Count; n++)
                {
                    Debug.WriteLine("* dlgId : " + n + "." + dlg[n].dlgId);
                    Activity reply2 = activity.CreateReply();
                    reply2.Recipient = activity.From;
                    reply2.Type = "message";
                    reply2.Attachments = new List<Attachment>();
                    reply2.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                    List<CardList> card = db.SelectDialogCard(dlg[n].dlgId);
                    List<TextList> text = db.SelectDialogText(dlg[n].dlgId);
                    List<MediaList> media = db.SelectDialogMedia(dlg[n].dlgId);

                    for (int i = 0; i < text.Count; i++)
                    {
                        HeroCard plCard = new HeroCard()
                        {
                            Title = text[i].cardTitle,
                            Subtitle = text[i].cardText
                        };

                        Attachment plAttachment = plCard.ToAttachment();
                        reply2.Attachments.Add(plAttachment);
                    }

                    for (int i = 0; i < card.Count; i++)
                    {
                        List<CardImage> cardImages = new List<CardImage>();
                        List<CardAction> cardButtons = new List<CardAction>();

                        if (card[i].imgUrl != null)
                        {
                            cardImages.Add(new CardImage(url: card[i].imgUrl));
                        }

                        if (card[i].btn1Type != null)
                        {
                            CardAction plButton = new CardAction()
                            {
                                Value = card[i].btn1Context,
                                Type = card[i].btn1Type,
                                Title = card[i].btn1Title
                            };

                            cardButtons.Add(plButton);
                        }

                        if (card[i].btn2Type != null)
                        {
                            CardAction plButton = new CardAction()
                            {
                                Value = card[i].btn2Context,
                                Type = card[i].btn2Type,
                                Title = card[i].btn2Title
                            };

                            cardButtons.Add(plButton);
                        }

                        if (card[i].btn3Type != null)
                        {
                            CardAction plButton = new CardAction()
                            {
                                Value = card[i].btn3Context,
                                Type = card[i].btn3Type,
                                Title = card[i].btn3Title
                            };

                            cardButtons.Add(plButton);
                        }

                        HeroCard plCard = new HeroCard()
                        {
                            Title = card[i].cardTitle,
                            Subtitle = card[i].cardSubTitle,
                            Images = cardImages,
                            Buttons = cardButtons
                        };

                        Attachment plAttachment = plCard.ToAttachment();
                        reply2.Attachments.Add(plAttachment);
                    }

                    for (int i = 0; i < media.Count; i++)
                    {
                        List<MediaUrl> mediaURL = new List<MediaUrl>();
                        List<CardAction> cardButtons = new List<CardAction>();

                        if (media[i].mediaUrl != null)
                        {
                            mediaURL.Add(new MediaUrl(url: media[i].mediaUrl));
                        }

                        if (media[i].btn1Type != null)
                        {
                            CardAction plButton = new CardAction()
                            {
                                Value = media[i].btn1Context,
                                Type = media[i].btn1Type,
                                Title = media[i].btn1Title
                            };

                            cardButtons.Add(plButton);
                        }

                        if (media[i].btn2Type != null)
                        {
                            CardAction plButton = new CardAction()
                            {
                                Value = media[i].btn2Context,
                                Type = media[i].btn2Type,
                                Title = media[i].btn2Title
                            };

                            cardButtons.Add(plButton);
                        }

                        if (media[i].btn3Type != null)
                        {
                            CardAction plButton = new CardAction()
                            {
                                Value = media[i].btn3Context,
                                Type = media[i].btn3Type,
                                Title = media[i].btn3Title
                            };

                            cardButtons.Add(plButton);
                        }

                        VideoCard plCard = new VideoCard()
                        {
                            Title = media[i].cardTitle,
                            Text = media[i].cardText,
                            Media = mediaURL,
                            Buttons = cardButtons,
                            Autostart = false
                        };

                        Attachment plAttachment = plCard.ToAttachment();
                        reply2.Attachments.Add(plAttachment);
                    }

                    var reply1 = await connector.Conversations.SendToConversationAsync(reply2);
                }
                DateTime endTime = DateTime.Now;
                Debug.WriteLine("프로그램 수행시간 : {0}/ms", ((endTime - startTime).Milliseconds));
                Debug.WriteLine("* activity.Type : " + activity.Type);
                Debug.WriteLine("* activity.Recipient.Id : " + activity.Recipient.Id);
                Debug.WriteLine("* activity.ServiceUrl : " + activity.ServiceUrl);
                var welcome = "";
                var welcomeMsg = "";

            }
            else if (activity.Type == ActivityTypes.Message)
            {
                //await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
                Debug.WriteLine("* activity.Type == ActivityTypes.Message ");

                string orgMent = "";
                string orgENGMent_history = "";
                JObject Luis = new JObject();
                DbConnect db = new DbConnect();
                StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                
                orgMent = activity.Text;
                Debug.WriteLine("* orgMent : "+ orgMent);

                Luis = await GetIntentFromBotLUIS(orgMent);
                
                float luisScore = (float)Luis["intents"][0]["score"];
                int luisEntityCount = (int)Luis["entities"].Count();
                Debug.WriteLine("* Luis Entity Count : " + luisEntityCount);
                Debug.WriteLine("* Luis Intents Score : " + luisScore);

                //
 

                if (luisScore > 0 && luisEntityCount > 0)
                {
                    string intent = (string)Luis["intents"][0]["intent"];
                    string entity = (string)Luis["entities"][0]["entity"];
                    Debug.WriteLine("* 1.intent : " + intent + " || entity : "+ entity);
                    intent = intent.Replace("\"", "");
                    entity = entity.Replace("\"", "");
                    entity = entity.Replace(" ", "");
                    //Debug.WriteLine("* sessionNo : " + sessionNo ); 
                    //Debug.WriteLine("LUIS_INTENT : " + (string)(context.Session["LUIS_INTENT"]));
                    //Debug.WriteLine("LUIS_ENTITY : " + (string)(context.Session["LUIS_ENTITY"]));

                    //
                    sessionNo = sessionNo + 1;
 
                    //
                    intentList.Add(intent);
                    entityList.Add(entity);

                    // 
                    if (intent == "customerInfo")
                    {
                        var luisCustomerInfo = userData.GetProperty<string>("luisCustomerInfo");
                        if (entity == "김설현")
                        {
                            luisCustomerInfo = "";
                        }
                        //var luisCustomerInfo = userData.GetProperty<string>("luisCustomerInfo");
                        luisCustomerInfo = luisCustomerInfo + "\n" + orgMent;
                        userData.SetProperty<string>("luisCustomerInfo", luisCustomerInfo);
                        Debug.WriteLine("1-1.intent : customerInfo || input : " + orgMent + " || entity : " + entity + " || luisCustomerInfo : " + luisCustomerInfo);

                    }


                    //userData.SetProperty<List>(IList, intentList);
                    userData.SetProperty<string>("luisIntent", intent);
                    userData.SetProperty<string>("luisEntity", entity);

                    userData.SetProperty<string>(intent, orgENGMent_history);
                    Debug.WriteLine("* 2. orgENGMent_history : " + orgENGMent_history);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                    Debug.WriteLine("* activity.ChannelId : " + activity.ChannelId + " || activity.From.Id : " + activity.From.Id);

                    List<LuisList> LuisDialogID = db.SelectLuis(intent, entity);

                    if (LuisDialogID.Count == 0)
                    {
                        Debug.WriteLine("* NO LuisDialogID");
                        Activity reply_err = activity.CreateReply();
                        reply_err.Recipient = activity.From;
                        reply_err.Type = "message";
                        reply_err.Text = "I'm sorry. I do not know what you mean.";
                        var reply1 = await connector.Conversations.SendToConversationAsync(reply_err);
                    }
                    else
                    {
                        Debug.WriteLine("* YES LuisDialogID || LuisDialogID.Count : " + LuisDialogID.Count);

                        for (int i = 0; i < LuisDialogID.Count; i++)
                        {
                            Activity reply2 = activity.CreateReply();
                            reply2.Recipient = activity.From;
                            reply2.Type = "message";
                            reply2.Attachments = new List<Attachment>();
                            reply2.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                            int dlgID = LuisDialogID[i].dlgId;

                            

                            List<DialogList> dlg = db.SelectDialog(dlgID);

                            for (int n = 0; n < dlg.Count; n++)
                            {
                                string dlgType = dlg[n].dlgType;

                                if (dlgType == TEXTDLG)
                                {
                                    List<TextList> text = db.SelectDialogText(dlg[n].dlgId);

                                    for (int j = 0; j < text.Count; j++)
                                    {
                                        HeroCard plCard = new HeroCard()
                                        {
                                            Title = text[j].cardTitle,
                                            Subtitle = text[j].cardText
                                        };

                                        Attachment plAttachment = plCard.ToAttachment();
                                        reply2.Attachments.Add(plAttachment);
                                    }
                                }
                                else if (dlgType == CARDDLG)
                                {
                                    List<CardList> card = db.SelectDialogCard(dlg[n].dlgId);

                                    for (int j = 0; j < card.Count; j++)
                                    {
                                        List<CardImage> cardImages = new List<CardImage>();
                                        List<CardAction> cardButtons = new List<CardAction>();

                                        if (card[j].imgUrl != null)
                                        {
                                            cardImages.Add(new CardImage(url: card[j].imgUrl));
                                        }

                                        if (card[j].btn1Type != null)
                                        {
                                            CardAction plButton = new CardAction()
                                            {
                                                Value = card[j].btn1Context,
                                                Type = card[j].btn1Type,
                                                Title = card[j].btn1Title
                                            };

                                            cardButtons.Add(plButton);
                                        }

                                        if (card[j].btn2Type != null)
                                        {
                                            CardAction plButton = new CardAction()
                                            {
                                                Value = card[j].btn2Context,
                                                Type = card[j].btn2Type,
                                                Title = card[j].btn2Title
                                            };

                                            cardButtons.Add(plButton);
                                        }

                                        if (card[j].btn3Type != null)
                                        {
                                            CardAction plButton = new CardAction()
                                            {
                                                Value = card[j].btn3Context,
                                                Type = card[j].btn3Type,
                                                Title = card[j].btn3Title
                                            };

                                            cardButtons.Add(plButton);
                                        }

                                        var strCardSubTitle = card[j].cardSubTitle;
                                        if (card[j].dlgId == 2009)
                                        {
                                            //var customerInfo = userData.GetProperty<string>("luisCustomerInfo");
                                            strCardSubTitle = userData.GetProperty<string>("luisCustomerInfo");
                                        }
                                        
                                        HeroCard plCard = new HeroCard()
                                        {
                                            Title = card[j].cardTitle,
                                            //Subtitle = card[j].cardSubTitle,
                                            Subtitle = strCardSubTitle,
                                            Images = cardImages,
                                            Buttons = cardButtons
                                        };

                                        Attachment plAttachment = plCard.ToAttachment();
                                        reply2.Attachments.Add(plAttachment);
                                    }
                                }
                                else if (dlgType == MEDIADLG)
                                {
                                    List<MediaList> media = db.SelectDialogMedia(dlg[n].dlgId);

                                    for (int j = 0; j < media.Count; j++)
                                    {
                                        List<MediaUrl> mediaURL = new List<MediaUrl>();
                                        List<CardAction> cardButtons = new List<CardAction>();

                                        if (media[j].mediaUrl != null)
                                        {
                                            mediaURL.Add(new MediaUrl(url: media[j].mediaUrl));
                                        }

                                        if (media[j].btn1Type != null)
                                        {
                                            CardAction plButton = new CardAction()
                                            {
                                                Value = media[j].btn1Context,
                                                Type = media[j].btn1Type,
                                                Title = media[j].btn1Title
                                            };

                                            cardButtons.Add(plButton);
                                        }

                                        if (media[j].btn2Type != null)
                                        {
                                            CardAction plButton = new CardAction()
                                            {
                                                Value = media[j].btn2Context,
                                                Type = media[j].btn2Type,
                                                Title = media[j].btn2Title
                                            };

                                            cardButtons.Add(plButton);
                                        }

                                        if (media[j].btn3Type != null)
                                        {
                                            CardAction plButton = new CardAction()
                                            {
                                                Value = media[j].btn3Context,
                                                Type = media[j].btn3Type,
                                                Title = media[j].btn3Title
                                            };

                                            cardButtons.Add(plButton);
                                        }

                                        VideoCard plCard = new VideoCard()
                                        {
                                            Title = media[j].cardTitle,
                                            Text = media[j].cardText,
                                            Media = mediaURL,
                                            Buttons = cardButtons,
                                            Autostart = false
                                        };

                                        Attachment plAttachment = plCard.ToAttachment();
                                        reply2.Attachments.Add(plAttachment);
                                    }
                                }
                            }

                            var reply1 = await connector.Conversations.SendToConversationAsync(reply2);

                        }

                    }
                }
                else
                {
                    Debug.WriteLine("* NO Luis Score.. ");
                    Debug.WriteLine(userData.ToString());

                    //
                    var intentArray = intentList.ToArray();
 
                    foreach (string intent in intentList)
                    {
                        Debug.WriteLine("* intent[] :" + intent);
                        //System.Console.WriteLine(prime);
                    }
                    Debug.WriteLine("intentList" + intentList);
                    
                    Activity reply_err = activity.CreateReply();
 
                    //
                    var luisIntent = userData.GetProperty<string>("luisIntent");
                    var luisEntity = userData.GetProperty<string>("luisEntity");
                    Debug.WriteLine("** orgMent : " + orgMent + "| luisIntent : " + luisIntent + " | luisEntity : " + luisEntity);

                    List<LuisList> LuisDialogID = db.SelectLuisSecond(activity.Text, luisIntent, luisEntity);

                    if (LuisDialogID.Count == 0)
                    {
                        Debug.WriteLine("** NO LuisDialogID");
                        //Activity reply_err = activity.CreateReply();
                        reply_err.Recipient = activity.From;
                        reply_err.Type = "message";
                        reply_err.Text = "I'm sorry. I do not know what you mean.";
                        var reply1 = await connector.Conversations.SendToConversationAsync(reply_err);
                        
                    }
                    else
                    {
                        Debug.WriteLine("** SelectLuisSecond() YES | LuisDialogID | LuisDialogID.Count : " + LuisDialogID.Count);

                        for (int i = 0; i < LuisDialogID.Count; i++)
                        {
                            Activity reply2 = activity.CreateReply();
                            reply2.Recipient = activity.From;
                            reply2.Type = "message";
                            reply2.Attachments = new List<Attachment>();
                            reply2.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                            int dlgID = LuisDialogID[i].dlgId;

                            string dlgIntent = LuisDialogID[i].dlgIntent;
                            string dlgEntities = LuisDialogID[i].dlgEntities;
                            
                            userData.SetProperty<string>("luisIntent", dlgIntent);
                            userData.SetProperty<string>("luisEntity", dlgEntities);
                            Debug.WriteLine("** userData.SetProperty > ("+ dlgID+") luisIntent: " + dlgIntent + "| luisEntity:" + dlgEntities);

                            await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);

                            List <DialogList> dlg = db.SelectDialog(dlgID);

                            for (int n = 0; n < dlg.Count; n++)
                            {
                                string dlgType = dlg[n].dlgType;

                                if (dlgType == TEXTDLG)
                                {
                                    List<TextList> text = db.SelectDialogText(dlg[n].dlgId);

                                    for (int j = 0; j < text.Count; j++)
                                    {
                                        HeroCard plCard = new HeroCard()
                                        {
                                            Title = text[j].cardTitle,
                                            Subtitle = text[j].cardText
                                        };

                                        Attachment plAttachment = plCard.ToAttachment();
                                        reply2.Attachments.Add(plAttachment);
                                    }
                                }
                                else if (dlgType == CARDDLG)
                                {
                                    List<CardList> card = db.SelectDialogCard(dlg[n].dlgId);

                                    for (int j = 0; j < card.Count; j++)
                                    {
                                        List<CardImage> cardImages = new List<CardImage>();
                                        List<CardAction> cardButtons = new List<CardAction>();

                                        if (card[j].imgUrl != null)
                                        {
                                            cardImages.Add(new CardImage(url: card[j].imgUrl));
                                        }

                                        if (card[j].btn1Type != null)
                                        {
                                            CardAction plButton = new CardAction()
                                            {
                                                Value = card[j].btn1Context,
                                                Type = card[j].btn1Type,
                                                Title = card[j].btn1Title
                                            };

                                            cardButtons.Add(plButton);
                                        }

                                        if (card[j].btn2Type != null)
                                        {
                                            CardAction plButton = new CardAction()
                                            {
                                                Value = card[j].btn2Context,
                                                Type = card[j].btn2Type,
                                                Title = card[j].btn2Title
                                            };

                                            cardButtons.Add(plButton);
                                        }

                                        if (card[j].btn3Type != null)
                                        {
                                            CardAction plButton = new CardAction()
                                            {
                                                Value = card[j].btn3Context,
                                                Type = card[j].btn3Type,
                                                Title = card[j].btn3Title
                                            };

                                            cardButtons.Add(plButton);
                                        }

                                        HeroCard plCard = new HeroCard()
                                        {
                                            Title = card[j].cardTitle,
                                            Subtitle = card[j].cardSubTitle,
                                            Images = cardImages,
                                            Buttons = cardButtons
                                        };

                                        Attachment plAttachment = plCard.ToAttachment();
                                        reply2.Attachments.Add(plAttachment);
                                    }
                                }
                                else if (dlgType == MEDIADLG)
                                {
                                    List<MediaList> media = db.SelectDialogMedia(dlg[n].dlgId);

                                    for (int j = 0; j < media.Count; j++)
                                    {
                                        List<MediaUrl> mediaURL = new List<MediaUrl>();
                                        List<CardAction> cardButtons = new List<CardAction>();

                                        if (media[j].mediaUrl != null)
                                        {
                                            mediaURL.Add(new MediaUrl(url: media[j].mediaUrl));
                                        }

                                        if (media[j].btn1Type != null)
                                        {
                                            CardAction plButton = new CardAction()
                                            {
                                                Value = media[j].btn1Context,
                                                Type = media[j].btn1Type,
                                                Title = media[j].btn1Title
                                            };

                                            cardButtons.Add(plButton);
                                        }

                                        if (media[j].btn2Type != null)
                                        {
                                            CardAction plButton = new CardAction()
                                            {
                                                Value = media[j].btn2Context,
                                                Type = media[j].btn2Type,
                                                Title = media[j].btn2Title
                                            };

                                            cardButtons.Add(plButton);
                                        }

                                        if (media[j].btn3Type != null)
                                        {
                                            CardAction plButton = new CardAction()
                                            {
                                                Value = media[j].btn3Context,
                                                Type = media[j].btn3Type,
                                                Title = media[j].btn3Title
                                            };

                                            cardButtons.Add(plButton);
                                        }

                                        VideoCard plCard = new VideoCard()
                                        {
                                            Title = media[j].cardTitle,
                                            Text = media[j].cardText,
                                            Media = mediaURL,
                                            Buttons = cardButtons,
                                            Autostart = false
                                        };

                                        Attachment plAttachment = plCard.ToAttachment();
                                        reply2.Attachments.Add(plAttachment);
                                    }
                                }
                            }

                            var reply1 = await connector.Conversations.SendToConversationAsync(reply2);
                            
                        }


                    }
                }
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

        private static async Task<JObject> GetIntentFromBotLUIS(string Query)
        {
            Query = Uri.EscapeDataString(Query);
            JObject jsonObj = new JObject();
            string[] RequestURI = new string[1];
            //RequestURI[0] = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/ac08a04f-3a5a-4bae-9eaa-47fe069d01b5?subscription-key=7489b95cf3fb4797939ea70ce94a4b11" + "&timezoneOffset=0&verbose=true&q=" + Query;
            //RequestURI[1] = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/b1437ec6-3301-4c24-8bcb-1af58ee2c47c?subscription-key=7efb093087dd48918b903885b944740c" + "&timezoneOffset=0&verbose=true&q=" + Query;
            //RequestURI[0] = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/83f7cb60-6027-4fbb-b3da-d6d4d4b5a40f?subscription-key=03067a6e4dca40ee9766a8fa9da6d864" + "&timezoneOffset=0&verbose=true&q=" + Query;
            RequestURI[0] = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/5ba3e658-7b1a-45bc-92a8-faa29f553f2a?subscription-key=03067a6e4dca40ee9766a8fa9da6d864" + "&timezoneOffset=0&verbose=true&q=" + Query;
            using (HttpClient client = new HttpClient())
            {
                for (int i = 0; i < RequestURI.Length; i++)
                {
                    HttpResponseMessage msg = await client.GetAsync(RequestURI[i]);

                    if (msg.IsSuccessStatusCode)
                    {
                        var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                        jsonObj = JObject.Parse(JsonDataResponse);
                    }
                    msg.Dispose();

                    if (jsonObj["entities"].Count() != 0 && (float)jsonObj["intents"][0]["score"] > 0.3)
                    {
                        break;
                    }
                }
            }
            return jsonObj;
        }




    }
}