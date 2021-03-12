using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ViberBot.Client.Enums;
using ViberBot.Client.Handlers;
using ViberBotLib.Enums;
using ViberBotLib.Models;
using ViberBotLib.Models.Response;

namespace ViberBot.Client.Actions
{
	public class StartSendContactsAction : ActionBase
	{
		public static async Task Execute(CallbackData data)
		{
			// проверить есть ли запись о пользователе вообще
			//или создавать ее во время приветствия

			//отправить подтверждение
			var receiver = data.Sender.Id;

			//достать данные из таблицы
			var contactsData = Handlers.GoogleSheetsHandler.GetContactsData(data.Sender.Id);
			var trackingData = JsonConvert.DeserializeObject<ViberBotTrackingData>(data.Message.TrackingData);

			trackingData.Add(DataField.Contacts.ToString(), contactsData);

			var message = new KeyboardMessage
			{
				Receiver = receiver,
				Sender = client.BotUserData,
				Text = $"Указанные контактные данные - {contactsData}. Введите номер телефона по которому можно было бы с вами связаться. Данные вводите в числовом формате без пробелов. Пример: +370123456789",
				TrackingData = ViberBotTrackingData.Build("StartSendContactsAction", "СonfirmSendContactsAction", trackingData.Data),
				Keyboard = KeyboardHandler.Cancel
			};

			await client.SendMessage(receiver, message);
		}
	}
	public class СonfirmSendContactsAction : ActionBase
	{
		public static async Task Execute(CallbackData data)
		{
			var receiver = data.Sender.Id;

			var userValue = ((TextMessage)data.Message).Text;

			//TODO возможно сделать проверку номера
			Regex r = new Regex(@"^\+?\d{0,2}\-?\d{4,5}\-?\d{5,6}");

			if (r.IsMatch(userValue))
			{
				//учесть данные в поле трекинга

				var trackingData = ViberBotTrackingData.Get(data.Message.TrackingData);

				var contactsData = trackingData.Data[DataField.Contacts.ToString()];

				trackingData.Add(DataField.Contacts.ToString(), userValue);

				//запросить  подтверждение
				var message = new KeyboardMessage
				{
					Receiver = receiver,
					Sender = client.BotUserData,
					Text = $"Старые контактные данные - {contactsData}. Вы указали - {userValue}. Данные верны?",
					TrackingData = ViberBotTrackingData.Build("СonfirmSendContactsAction", "SendContactsAction", trackingData.Data),
					Keyboard = KeyboardHandler.YesNoCancel()
				};
				await client.SendMessage(receiver, message);
			}
			else
			{
				//неверное значение
				await client.SendTextMessage(receiver, BotError.BadValue);
			}
		}
	}
	public class SendContactsAction : ActionBase
	{
		public static async Task Execute(CallbackData data)
		{
			var text = ((TextMessage)data.Message).Text;
			var receiver = data.Sender.Id;
			var trackingData = ViberBotTrackingData.Get(data.Message.TrackingData);

			if (text == "no")
			{
				await StartSendContactsAction.Execute(data);
				return;
			}else if (text == "yes")
			{
				var contacts = trackingData.Data[DataField.Contacts.ToString()];
			//сохранить в таблицу
				Handlers.GoogleSheetsHandler.UpdateContactsData(contacts, data.Sender);

				await client.SendTextMessage(receiver, "Данные успешно сохранены.");
			}
			else
			{
				var message = new KeyboardMessage
				{
					Receiver = receiver,
					Sender = client.BotUserData,
					Text = $"Выберите действи из списка.",
					TrackingData = ViberBotTrackingData.Build("SendContactsAction", "SendContactsAction", trackingData.Data),
					Keyboard = KeyboardHandler.YesNoCancel()
				};
				await client.SendMessage(receiver, message);
			}
		}
	}
}
