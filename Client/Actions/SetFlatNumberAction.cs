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
	public class StartSetFlatNumberAction : ActionBase
	{
		public static async Task Execute(CallbackData data)
		{
			//достать данные из таблицы
			var flats = Handlers.GoogleSheetsHandler.CheckFlatData(data.Sender.Id);

			if (flats.Count > 0)
			{
				//отправить подтверждение
				var receiver = data.Sender.Id;

				var message = new KeyboardMessage
				{
					Receiver = receiver,
					Sender = client.BotUserData,
					Text = $"Вы указывали квартиры : {String.Join(",", flats)}. Выберите действие.",
					TrackingData = ViberBotTrackingData.Build("", "StartSetFlatNumberAction"),
					Keyboard = KeyboardHandler.CustomButtons(new string[][] { new string[] { "Добавить квартиру", "AddFlatNumberAction" }, new string[] { "Удалить квартиру", "ConfirmDeleteFlatNumberAction" }, new string[] { "Отмена", "Cancel" } })
				};

				await client.SendMessage(receiver, message);
			}
			else
			{
				await AddFlatNumberAction.Execute(data);
			}
		}
	}

	public class AddFlatNumberAction : ActionBase
	{
		public static async Task Execute(CallbackData data)
		{
			//отправить подтверждение
			var receiver = data.Sender.Id;

			var message = new KeyboardMessage
			{
				Receiver = receiver,
				Sender = client.BotUserData,
				Text = $"Укажите номер вашей квартиры.",
				TrackingData = ViberBotTrackingData.Build("AddFlatNumberAction", "СonfirmSetFlatNumberAction"),
				Keyboard = KeyboardHandler.Cancel
			};

			await client.SendMessage(receiver, message);
		}
	}

	public class СonfirmSetFlatNumberAction : ActionBase
	{
		public static async Task Execute(CallbackData data)
		{
			var receiver = data.Sender.Id;

			if (int.TryParse(((TextMessage)data.Message).Text, out int userValue))
			{
				//учесть данные в поле трекинга
				var flats = Handlers.GoogleSheetsHandler.CheckFlatData(data.Sender.Id);

				if (flats.Any(f => f.ToString() == userValue.ToString()))
				{
					await client.SendTextMessage(receiver, $"Вы указали - {userValue}. Уже в списке.");
				}
				else if (userValue <= 0 || userValue > ViberBotClient.GetInstance().Settings.FlatsNumber)
				{
					await client.SendTextMessage(receiver, $"Вы указали - {userValue}. Номер квартиры должен быть от 1 до {ViberBotClient.GetInstance().Settings.FlatsNumber}.");
				}
				else
				{
					var trackingData = ViberBotTrackingData.Get(data.Message.TrackingData);

					trackingData.Add(DataField.FlatNumber.ToString(), userValue.ToString());

					//запросить  подтверждение
					var message = new KeyboardMessage
					{
						Receiver = receiver,
						Sender = client.BotUserData,
						Text = $"Вы указали - {userValue}. Данные верны?",
						TrackingData = ViberBotTrackingData.Build("СonfirmSetFlatNumberAction", "SetFlatNumberAction", trackingData.Data),
						Keyboard = KeyboardHandler.YesNoCancel()
					};

					await client.SendMessage(receiver, message);
				}
			}
			else
			{
				//неверное значение
				await client.SendTextMessage(receiver, BotError.BadValue);
			}
		}
	}
	public class ConfirmDeleteFlatNumberAction : ActionBase
	{
		public static async Task Execute(CallbackData data)
		{
			//достать данные из таблицы
			var receiver = data.Sender.Id;
			var flats = Handlers.GoogleSheetsHandler.CheckFlatData(data.Sender.Id);

			if (flats.Any())
			{
				var flatButtonsData = new List<string[]>();

				foreach (var flat in flats)
				{
					flatButtonsData.Add(new string[] { flat.ToString(), flat.ToString() });
				}

				flatButtonsData.Add(new string[] { "Отмена", "Cancel" });
				//отправить подтверждение

				var message = new KeyboardMessage
				{
					Receiver = receiver,
					Sender = client.BotUserData,
					Text = $"Выберите номер квартиру для удаления.",
					TrackingData = ViberBotTrackingData.Build("ConfirmDeleteFlatNumberAction", "DeleteFlatNumberAction"),
					Keyboard = KeyboardHandler.CustomButtons(flatButtonsData.ToArray())
				};

				await client.SendMessage(receiver, message);
			}
			else
			{
				await client.SendTextMessage(receiver, "Нет привязаных квартир.");
			}
		}
	}


	public class DeleteFlatNumberAction : ActionBase
	{
		public static async Task Execute(CallbackData data)
		{
			var receiver = data.Sender.Id;
			var flatNumber = ((TextMessage)data.Message).Text;

			Handlers.GoogleSheetsHandler.DeleteUserFlatNumber(flatNumber, data.Sender);

			//отправить подтверждение
			await client.SendTextMessage(receiver, "Данные успешно удалены.");

		}
	}
	public class SetFlatNumberAction : ActionBase
	{
		public static async Task Execute(CallbackData data)
		{

			var text = ((TextMessage)data.Message).Text;
			var receiver = data.Sender.Id;
			var trackingData = ViberBotTrackingData.Get(data.Message.TrackingData);

			if (text == "no")
			{
				await StartSetFlatNumberAction.Execute(data);
				return;
			}
			else if (text == "yes")
			{
				var flatNumber = trackingData.Data[DataField.FlatNumber.ToString()];

				//сохранить в таблицу
				Handlers.GoogleSheetsHandler.AddUserFlatNumber(flatNumber, data.Sender);

				await client.SendTextMessage(receiver, "Данные успешно сохранены.");
			}
			else
			{
				var message = new KeyboardMessage
				{
					Receiver = receiver,
					Sender = client.BotUserData,
					Text = $"Выберите действи из списка.",
					TrackingData = ViberBotTrackingData.Build("SetFlatNumberAction", "SetFlatNumberAction", trackingData.Data),
					Keyboard = KeyboardHandler.YesNoCancel()
				};

				await client.SendMessage(receiver, message);
			}
		}
	}
}
