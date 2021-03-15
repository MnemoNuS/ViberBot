using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ViberBotLib.Enums;
using ViberBotLib.Models;
using ViberBotLib.Models.Response;
using Newtonsoft;
using ViberBot.Client.Handlers;
using ViberBot.Client.Actions;
using System.Reflection;
using Newtonsoft.Json;
using ViberBot.Client;
using ViberBot.Client.Enums;

namespace ViberBot
{
	public class ViberBotClient : IViberClient
	{
		//https://github.com/Viber/sample-bot-isitup/blob/master/src/index.js
		//ngrok http -host-header=localhost:44337 44337

		public UserBase BotUserData = new UserBase { Name = "Bot", Avatar = "" };

		const string _setWevhookUrl = "https://chatapi.viber.com/pa/set_webhook";
		const string _sendMessageUrl = "https://chatapi.viber.com/pa/send_message";

		public Action<CallbackData> OnCallback;
		public Action<CallbackData> OnMessage;

		public ClientSettings Settings;
		public bool WebhookReady { get; private set; }

		private string _viberAuthToken = "";
		private static ViberBotClient _instance;
		private ICollection<EventType> _eventTypes;
		public static ViberBotClient GetInstance()
		{
			if (_instance == null)
				throw new NullReferenceException();

			return _instance;
		}
		public static ViberBotClient Init(string token, ClientSettings settings)
		{
			_instance = new ViberBotClient(token);
			_instance.Settings = settings;
			return _instance;
		}
		private ViberBotClient(string token)
		{
			_viberAuthToken = token;

			OnCallback += (data) => ProcessCallback(data);
			OnMessage += async (data) => await ProcessMessageCallback(data);
		}
		public async Task<ICollection<EventType>> SetWebhookAsync(string url, ICollection<EventType> eventTypes = null)
		{
			_eventTypes = eventTypes;

			var requestData = new ViberBotLib.Models.Request.SetWebhook { Url = url, EventTypes = eventTypes, SendName = true, SendPhoto = true };

			var responseString = await SendRequest(_setWevhookUrl, Newtonsoft.Json.JsonConvert.SerializeObject(requestData));
			var responseData = Newtonsoft.Json.JsonConvert.DeserializeObject<ViberBotLib.Models.Response.WebhookResponse>(responseString);

			_instance.WebhookReady = responseData.EventTypes != null;

			return responseData.EventTypes;
		}
		public bool ValidateWebhookHash(string signatureHeader, string jsonMessage)
		{
			throw new NotImplementedException();
		}
		public async Task<string> SendRequest(string url, string jsonData)
		{
			string responseData;

			using (WebClient wc = new WebClient())
			{
				wc.Headers.Add("X-Viber-Auth-Token", _viberAuthToken); // добвить токен из настроек
				responseData = await wc.UploadStringTaskAsync(new Uri(url), jsonData);
			}

			return responseData;
		}
		private void ProcessCallback(CallbackData data)
		{

			switch (data.Event)
			{
				case "message":
					OnMessage(data);
					break;
				case "conversation_started":
					OnConversationStarted(data);
					break;
				//case MessageType.Picture:
				//	type = typeof(PictureMessage);
				//	break;
				//case MessageType.Video:
				//	type = typeof(VideoMessage);
				//	break;
				//case MessageType.File:
				//	type = typeof(FileMessage);
				//	break;
				//case MessageType.Location:
				//	type = typeof(LocationMessage);
				//	break;
				//case MessageType.Contact:
				//	type = typeof(ContactMessage);
				//	break;
				//case MessageType.Sticker:
				//	type = typeof(StickerMessage);
				//	break;
				//case MessageType.CarouselContent:
				//	throw new NotImplementedException();
				//case MessageType.Url:
				//	type = typeof(UrlMessage);
				//	break;
				default:
					//throw new ArgumentOutOfRangeException();
					break;
			}
		}
		private async Task ProcessMessageCallback(CallbackData data)
		{
			if (data.Event == "message")
			{
				var messageText = ((TextMessage)data.Message).Text;

				if (messageText.ToLower() == "start")
				{
					data.Message.TrackingData = ViberBotTrackingData.Empty();
					await MenuAction.Execute(data);
					return;
				}
				if (messageText.ToLower() == "cancel" || data.Message.TrackingData == null)
				{
					data.Message.TrackingData = ViberBotTrackingData.Empty();
					await DefaultAction.Execute(data);
					return;
				}
				if (messageText.ToLower() == "help")
				{
					data.Message.TrackingData = ViberBotTrackingData.Empty();
					await HelpAction.Execute(data);
					return;
				}
				try
				{
					var trackingData = JsonConvert.DeserializeObject<ViberBotTrackingData>(data.Message.TrackingData);

					if (messageText.Contains("Action"))
					{
						trackingData.NextAction = messageText;
					}

					if (trackingData != null && !string.IsNullOrEmpty(trackingData.NextAction))
					{
						Type magicType = Type.GetType($"ViberBot.Client.Actions.{trackingData.NextAction}");
						if (magicType != null)
						{
							MethodInfo magicMethod = magicType.GetMethod("Execute");
							magicMethod.Invoke(null, new object[] { data });
						}
					}
					else
					{
						await DefaultAction.Execute(data);
					}

				}
				catch
				{
					await SendTextMessage(data.Sender.Id, BotError.SomethingWentWrong);
				}
			}
		}
		private async Task OnConversationStarted(CallbackData data)
		{
			if (data.Event == "conversation_started")
			{
				await GreetingsAction.Execute(data);
			}
		}

		public async Task<string> SendTextMessage(string receiver, string text)
		{
			var message = new KeyboardMessage
			{
				Receiver = receiver,
				Sender = BotUserData,
				Text = text,
				TrackingData = ViberBotTrackingData.Empty(),
				Keyboard = KeyboardHandler.MainMenu
			};
			return await SendMessage(receiver, message);
		}
		public async Task<string> SendMessage(string receiver, MessageBase message)
		{
			var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(message);
			var responseData = "";
			var url = _sendMessageUrl;
			using (WebClient wc = new WebClient())
			{
				wc.Encoding = System.Text.Encoding.UTF8;
				wc.Headers.Add("X-Viber-Auth-Token", _viberAuthToken); // добвить токен из настроек
				responseData = await wc.UploadStringTaskAsync(new Uri(url), jsonData);
			}
			return responseData;
		}
	}
}
