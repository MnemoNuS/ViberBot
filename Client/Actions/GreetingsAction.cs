using System.Threading.Tasks;
using ViberBot.Client.Handlers;
using ViberBotLib.Models;
using ViberBotLib.Models.Response;

namespace ViberBot.Client.Actions
{
	public class GreetingsAction : ActionBase
	{
		public static async Task Execute(CallbackData data)
		{
			//достать данные из таблицы
			Handlers.GoogleSheetsHandler.AddUser(data.User);

			//отправить подтверждение
			var receiver = data.User.Id;

			//добавить пользователя

			var message = new KeyboardMessage
			{
				Receiver = receiver,
				Sender = client.BotUserData,
				Text = $"Добрый день я бот который помогает учитывать данные счетчика электроэнерги. Пожалуйста зарегистрируйте квартиру и контактные данные.",
				TrackingData = ViberBotTrackingData.Empty(),
				Keyboard = KeyboardHandler.Start
			};
			await client.SendMessage(receiver, message);
		}
	}
}
