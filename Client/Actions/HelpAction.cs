using System.Threading.Tasks;
using ViberBot.Client.Handlers;
using ViberBotLib.Models;
using ViberBotLib.Models.Response;

namespace ViberBot.Client.Actions
{
	public class HelpAction : ActionBase
	{
		public static async Task Execute(CallbackData data)
		{

			//отправить подтверждение
			var receiver = data.Sender.Id;

			var helpFile = client.Settings.HelpFile;

			var message = new KeyboardMessage
			{
				Receiver = receiver,
				Sender = client.BotUserData,
				Text = $"Добрый день я бот который помогает учитывать данные счетчика электроэнерги. Инструкцию по использованию можно найти по адресу {helpFile.Url}.",
				TrackingData = ViberBotTrackingData.Empty(),
				Keyboard = KeyboardHandler.MainMenu
			};
			await client.SendMessage(receiver, message);

			var helpLink = new LinkMessage
			{
				Receiver = receiver,
				Sender = client.BotUserData,
				Media = helpFile.Url,
			};

			await client.SendMessage(receiver, helpLink);

			var filemessage = new FileMessage
			{
				Receiver = receiver,
				Sender = client.BotUserData,
				Media = helpFile.Url,
				Size = helpFile.Size,
				FileName = helpFile.FileName
			};

			await client.SendMessage(receiver, filemessage);

			await DefaultAction.Execute(data);
		}
	}
}
