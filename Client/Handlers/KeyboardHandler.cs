using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ViberBotLib.Enums;
using ViberBotLib.Models;

namespace ViberBot.Client.Handlers
{
	public class KeyboardHandler
	{
		public static Keyboard MainMenu => new Keyboard
		{
			DefaultHeight = true,
			Buttons = new KeyboardButton[] {
				new KeyboardButton { ActionType = KeyboardActionType.Reply, ActionBody = "StartSetFlatNumberAction", Text = "Указать номер квартиры", TextSize = TextSize.Regular },
				new KeyboardButton { ActionType = KeyboardActionType.Reply, ActionBody = "StartSendContactsAction", Text = "Указать контактные данные", TextSize = TextSize.Regular },
				new KeyboardButton { ActionType = KeyboardActionType.Reply, ActionBody = "StartSendDataAction", Text = "Указать данные счетчика", TextSize = TextSize.Regular },
				new KeyboardButton { ActionType = KeyboardActionType.Reply, ActionBody = "HelpAction", Text = "Инструкция", TextSize = TextSize.Regular }
			}
		};
		public static Keyboard Cancel => new Keyboard
		{
			Buttons = new KeyboardButton[] { new KeyboardButton { ActionType = KeyboardActionType.Reply, ActionBody = "Cancel", Text = "Отмена", TextSize = TextSize.Regular }
			}
		};
		public static Keyboard Start => new Keyboard
		{
			Buttons = new KeyboardButton[] { new KeyboardButton { ActionType = KeyboardActionType.Reply, ActionBody = "Start", Text = "Начать", TextSize = TextSize.Regular }
			}
		};
		public static Keyboard YesNo(string yesActionBody, string noActionBody) => new Keyboard
		{
			DefaultHeight = true,
			Buttons = new KeyboardButton[]
			{
				new KeyboardButton { ActionType = KeyboardActionType.Reply, ActionBody = yesActionBody, Text = "Да", TextSize = TextSize.Regular },
				new KeyboardButton { ActionType = KeyboardActionType.Reply, ActionBody = noActionBody, Text = "Нет", TextSize = TextSize.Regular },
			}
		};
		public static Keyboard YesNoCancel() => new Keyboard
		{
			DefaultHeight = true,
			Buttons = new KeyboardButton[]
			{
				new KeyboardButton { ActionType = KeyboardActionType.Reply, ActionBody = "yes", Text = "Да", TextSize = TextSize.Regular },
				new KeyboardButton { ActionType = KeyboardActionType.Reply, ActionBody = "no", Text = "Нет", TextSize = TextSize.Regular },
				new KeyboardButton { ActionType = KeyboardActionType.Reply, ActionBody = "Cancel", Text = "Отмена", TextSize = TextSize.Regular }
			}
		};
		public static Keyboard CustomButtons(params string[][] buttonsData) => new Keyboard
		{
			DefaultHeight = true,
			Buttons = ParamsToButtons(buttonsData)
		};

		private static KeyboardButton[] ParamsToButtons(params string[][] buttonsData)
		{
			var buttons = new List<KeyboardButton>();
			foreach (var data in buttonsData)
			{
				buttons.Add(new KeyboardButton { ActionType = KeyboardActionType.Reply, ActionBody = data[1], Text = data[0], TextSize = TextSize.Regular });
			}
			return buttons.ToArray();
		}

	}
}