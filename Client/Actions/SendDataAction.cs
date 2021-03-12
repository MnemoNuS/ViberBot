using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ViberBot.Client.Enums;
using ViberBot.Client.Handlers;
using ViberBotLib.Enums;
using ViberBotLib.Models;
using ViberBotLib.Models.Response;

namespace ViberBot.Client.Actions
{
	public class StartSendDataAction : ActionBase
	{
		public static async Task Execute(CallbackData data)
		{
			//отправить подтверждение
			var receiver = data.Sender.Id;

			if (client.Settings.DataReadingPeriod != null)
			{
				//проверка даты
				var day = DateTime.Now.Date.Day;
				if (day < client.Settings.DataReadingPeriod.From || day > client.Settings.DataReadingPeriod.To)
				{
					await client.SendTextMessage(receiver, BotError.Period(client.Settings.DataReadingPeriod.From.ToString(), client.Settings.DataReadingPeriod.To.ToString()));
					return;
				}
			}
			//достать данные из таблицы
			var flats = Handlers.GoogleSheetsHandler.CheckFlatData(data.Sender.Id);

			if (flats.Count == 0)
			{
				var message = new KeyboardMessage
				{
					Receiver = receiver,
					Sender = client.BotUserData,
					Text = $"У вас не привязана ни одна квартира.",
					TrackingData = ViberBotTrackingData.Empty(),
					Keyboard = KeyboardHandler.MainMenu
				};

				await client.SendMessage(receiver, message);
			}
			else if (flats.Count == 1)
			{
				var flatNumber = flats.FirstOrDefault().ToString();
				var lastData = Handlers.GoogleSheetsHandler.GetFlatData(flatNumber);

				var trackingData = JsonConvert.DeserializeObject<ViberBotTrackingData>(data.Message.TrackingData);
				trackingData.Add(DataField.FlatNumber.ToString(), flatNumber);

				var message = new KeyboardMessage
				{
					Receiver = receiver,
					Sender = client.BotUserData,
					Text = $"Квартира - { flatNumber}. Предыдущие показания счетчика - {lastData}. Введите текущие показания.",
					TrackingData = ViberBotTrackingData.Build("EnterSendDataAction", "СonfirmSendDataAction", trackingData.Data),
					Keyboard = KeyboardHandler.Cancel
				};

				await client.SendMessage(receiver, message);
			}
			else if (flats.Count > 1)
			{
				var flatButtonsData = new List<string[]>();
				foreach (var flat in flats)
				{
					flatButtonsData.Add(new string[] { flat.ToString(), flat.ToString() });
				}
				flatButtonsData.Add(new string[] { "Отмена", "Cancel" });

				var message = new KeyboardMessage
				{
					Receiver = receiver,
					Sender = client.BotUserData,
					Text = $"У вас указано несколько квартир. Выберите нужную.",
					TrackingData = ViberBotTrackingData.Build("StartSendDataAction", "EnterSendDataAction"),
					Keyboard = KeyboardHandler.CustomButtons(flatButtonsData.ToArray())
				};

				await client.SendMessage(receiver, message);
			}
		}
	}
	public class EnterSendDataAction : ActionBase
	{
		public static async Task Execute(CallbackData data)
		{
			//достать данные из таблицы

			var flatNumber = ((TextMessage)data.Message).Text;

			var lastData = Handlers.GoogleSheetsHandler.GetFlatData(flatNumber);

			var trackingData = ViberBotTrackingData.Get(data.Message.TrackingData);
			trackingData.Data.Add(DataField.FlatNumber.ToString(), flatNumber);

			//отправить подтверждение
			var receiver = data.Sender.Id;

			var message = new KeyboardMessage
			{
				Receiver = receiver,
				Sender = client.BotUserData,
				Text = $"Квартира - { flatNumber}. Предыдущие показания счетчика - {lastData}. Введите текущие показания.",
				TrackingData = ViberBotTrackingData.Build("EnterSendDataAction", "СonfirmSendDataAction", trackingData.Data),
				Keyboard = KeyboardHandler.Cancel
			};
			await client.SendMessage(receiver, message);
		}
	}
	public class СonfirmSendDataAction : ActionBase
	{
		public static async Task Execute(CallbackData data)
		{
			var receiver = data.Sender.Id;

			if (int.TryParse(((TextMessage)data.Message).Text, out int userValue))
			{

				//учесть данные в поле трекинга

				var trackingData = ViberBotTrackingData.Get(data.Message.TrackingData);

				var flatNumber = trackingData.Data[DataField.FlatNumber.ToString()];

				trackingData.Add(DataField.FlatValue.ToString(), userValue.ToString());

				//запросить  подтверждение
				var message = new KeyboardMessage
				{
					Receiver = receiver,
					Sender = client.BotUserData,
					Text = $"Для квартиры - {flatNumber}. Вы указали - {userValue}. Данные верны?",
					TrackingData = ViberBotTrackingData.Build("СonfirmSendDataAction", "SendDataAction", trackingData.Data),
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
	public class SendDataAction : ActionBase
	{
		public static async Task Execute(CallbackData data)
		{

			var text = ((TextMessage)data.Message).Text;
			var receiver = data.Sender.Id;
			var trackingData = ViberBotTrackingData.Get(data.Message.TrackingData);

			if (text == "no")
			{
				await StartSendDataAction.Execute(data);
				return;
			}
			else if (text == "yes")
			{

				var flatNumber = trackingData.Data[DataField.FlatNumber.ToString()];
				var flatValue = trackingData.Data[DataField.FlatValue.ToString()];

				//сохранить в таблицу
				Handlers.GoogleSheetsHandler.UpdateFlatData(flatNumber, flatValue, data.Sender);

				await client.SendTextMessage(receiver, "Данные успешно сохранены.");

			}else
			{
				var message = new KeyboardMessage
				{
					Receiver = receiver,
					Sender = client.BotUserData,
					Text = $"Выберите действи из списка.",
					TrackingData = ViberBotTrackingData.Build("SendDataAction", "SendDataAction", trackingData.Data),
					Keyboard = KeyboardHandler.YesNoCancel()
				};

				await client.SendMessage(receiver, message);
			}
		}
	}
}
