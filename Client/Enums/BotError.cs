using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ViberBot.Client.Enums
{    
	public class BotError
	{
		public const string BadValue = "Неверное значение";
		public const string SomethingWentWrong = "Что-то пошло не так...";
		public const string Date = "Данные можно подавать с 20 по 25 число месяца.";
		public static string Period (string from, string to) => $"Данные можно подавать с {from} по {to} число месяца.";

	}
}
