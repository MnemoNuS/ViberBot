using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ViberBot.Client.Handlers;
using ViberBotLib.Models;
using ViberBotLib.Models.Response;

namespace ViberBot.Client.Actions
{
	public class DefaultAction:ActionBase
	{
		public static async Task Execute(CallbackData data)
		{
			//достать данные из таблицы

			//отправить подтверждение
			var receiver = data.Sender.Id;

			var message = new KeyboardMessage
			{
				Receiver = receiver,
				Sender = client.BotUserData,
				Text = $"Выберите действие из списка.",
				TrackingData = ViberBotTrackingData.Empty(),
				Keyboard = KeyboardHandler.MainMenu
			};
			await client.SendMessage(receiver, message);
		}

	}
}
