using GoogleSheetsLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ViberBotLib.Models;

namespace ViberBot.Client.Handlers
{
	public class GoogleSheetsHandler
	{
		private static string _dateFormat = "m";
		private static string contactsDataSheet = "Контактные данные";
		private static string idColName = "Идентификатор";
		private static string flatColName = "Квартира";
		private static string dateColName = "Дата изменения";
		private static string whoColName = "Имя";
		private static string contactsColName = "Контактный номер";

		public static GoogleSheetsClient InitGoogleSheetsClient(string appName, string spresdsheetId, string secrets_file)
		{
			//настроить загрузку через конфиг
			string defaultSheet = DateTime.Now.Year.ToString();

			var googleSheetsClient = GoogleSheetsClient.Init(appName, spresdsheetId, defaultSheet, secrets_file);

			googleSheetsClient.BeforeAction += CheckDefaultSheet;

			return googleSheetsClient;
		}

		public static void CheckDefaultSheet()
		{
			var googleSheetsClient = GoogleSheetsClient.GetInstanse();
			if (!googleSheetsClient.IsSheetExist(GoogleSheetsClient.DefaultSheet))
			{
				googleSheetsClient.CreateSheet(GoogleSheetsClient.DefaultSheet);
				FillWithDefaultData(GoogleSheetsClient.DefaultSheet);
			}
		}

		public static void FillWithDefaultData(string name)
		{
			var client = ViberBotClient.GetInstance();
			var googleSheetsClient = GoogleSheetsClient.GetInstanse();

			if(!googleSheetsClient.IsSheetExist(name))
				return;

			var firstRow = new List<object>();

			firstRow.Add(flatColName);
			firstRow.AddRange(GetMonthNames());
			firstRow.Add(dateColName);
			firstRow.Add(idColName);
			firstRow.Add(whoColName);

			googleSheetsClient.UpdateData( 1,  1, firstRow, name);

			var flatNumbersColumn = new List<object>();
			flatNumbersColumn.AddRange(Enumerable.Range(1, client.Settings.FlatsNumber).Select(x => x.ToString()));

			googleSheetsClient.UpdateColData(2, 1, flatNumbersColumn, name);
		}
		public static List<string> GetMonthNames()
		{
			CultureInfo culInf = new CultureInfo("ru-RU");
			System.Threading.Thread.CurrentThread.CurrentCulture = culInf;
			System.Threading.Thread.CurrentThread.CurrentUICulture = culInf;
			var result = DateTimeFormatInfo.CurrentInfo.MonthNames.ToList();
			result.Remove("");
			return result;
		}

		public static List<string> CheckFlatData(string userId)
		{
			var googleSheetsClient = GoogleSheetsClient.GetInstanse();
			var dataSheet = contactsDataSheet;
			var data = googleSheetsClient.ReadData(dataSheet);
			var idColIndex = data[0].ToList().IndexOf(idColName);
			var flatColIndex = data[0].ToList().IndexOf(flatColName);
			var flats = data.FirstOrDefault(r => r.Count > 1 && r[idColIndex].ToString() == userId)[flatColIndex].ToString();
			var result = new List<string>();
			if(! string.IsNullOrEmpty(flats))
				result.AddRange( flats.Split(',').ToList());
			return result;
		}

		public static bool CheckContactsData(string userId)
		{
			var contacts = GetContactsData(userId);
			return string.IsNullOrEmpty(contacts);
		}

		public static string GetContactsData(string userId)
		{
			var googleSheetsClient = GoogleSheetsClient.GetInstanse();

			var dataSheet = contactsDataSheet;

			var data = googleSheetsClient.ReadData(dataSheet);

			var idColIndex = data[0].ToList().IndexOf(idColName);
			var userRow = data.FirstOrDefault(r => r.Count > idColIndex && r[idColIndex].ToString() == userId);
			var userRowIndex = data.ToList().IndexOf(userRow);
			var contactsColIndex = data[0].ToList().IndexOf(contactsColName);

			var contactsData = data[userRowIndex][contactsColIndex].ToString();
			return string.IsNullOrWhiteSpace(contactsData)? "отсутствуют": contactsData;
		}
		public static void UpdateContactsData( string value, User user)
		{
			var googleSheetsClient = GoogleSheetsClient.GetInstanse();
			var dataSheet = contactsDataSheet;
			var data = googleSheetsClient.ReadData(dataSheet);

			var idColIndex = data[0].ToList().IndexOf(idColName);
			var userRow = data.FirstOrDefault(r => r.Count > idColIndex && r[idColIndex].ToString() == user.Id);

			if (userRow != null)
			{
				var userRowIndex = data.ToList().IndexOf(userRow);
				var contactsColIndex = data[0].ToList().IndexOf(contactsColName);

				var objectList = new List<object>() { value };
				googleSheetsClient.UpdateData(userRowIndex + 1, contactsColIndex+1, objectList, dataSheet);

				var dateColIndex = data[0].ToList().IndexOf(dateColName);

				objectList = new List<object>() { DateTime.Now.ToString(_dateFormat)};
				googleSheetsClient.UpdateData(userRowIndex + 1, dateColIndex + 1, objectList, dataSheet);
			}
		}
		public static string GetFlatData(string flatNumber)
		{
			var googleSheetsClient = GoogleSheetsClient.GetInstanse();
			var data = googleSheetsClient.ReadData();
			var flatRowIndex = data.Select(o => o[0].ToString()).ToList().IndexOf(flatNumber);

			var value =  GetFlatValueFromData(flatNumber, data);
			if (!string.IsNullOrEmpty(value))
				return value;

			//проверить предыдущий год
			var previouseYear = (DateTime.Now.Year - 1).ToString();
			if (googleSheetsClient.IsSheetExist(previouseYear))
			{
				data = googleSheetsClient.ReadData(previouseYear);
				value = GetFlatValueFromData(flatNumber, data);
				if (!string.IsNullOrEmpty(value))
					return value;
			}
			return "отсутствуют";
		}

		public static string GetFlatValueFromData(string flatNumber, IList<IList<object>> data)
		{
			var flatRowIndex = data.Select(o => o[0].ToString()).ToList().IndexOf(flatNumber);

			if (flatRowIndex > 0 && data[flatRowIndex].Count > 1)
			{
				for (int i = 12; i > 1; i--)
				{
					if (!string.IsNullOrEmpty(data[flatRowIndex][i].ToString()))
						return data[flatRowIndex][i].ToString();
				}
			}
			return "";
		}
		public static void UpdateFlatData(string flatNumber, string value, User user)
		{
			var googleSheetsClient = GoogleSheetsClient.GetInstanse();
			var data = googleSheetsClient.ReadData();
			var flatRowIndex = data.Select(o => o[0].ToString()).ToList().IndexOf(flatNumber);

			if (flatRowIndex > 0)
			{
				var objectList = new List<object>() {value};
				googleSheetsClient.UpdateData(flatRowIndex + 1, DateTime.Now.Month+1, objectList);

				var colIndex = data[0].ToList().IndexOf(dateColName);

				objectList = new List<object>() { DateTime.Now.ToString(_dateFormat), user.Id, user.Name };
				googleSheetsClient.UpdateData(flatRowIndex + 1, colIndex+1, objectList);
			}
		}

		public static void AddUser(User user)
		{
			var googleSheetsClient = GoogleSheetsClient.GetInstanse();

			var dataSheet = contactsDataSheet;

			var data = googleSheetsClient.ReadData(dataSheet);

			var idColIndex = data[0].ToList().IndexOf(idColName);

			var flatRow = data.FirstOrDefault(r => r.Count > idColIndex && r[idColIndex].ToString() == user.Id);
			var flatRowIndex = data.ToList().IndexOf(flatRow);

			if (flatRowIndex < 0)
			{
				var objectList = new List<object>() { user.Id, "", user.Name, "", DateTime.Now.ToString(_dateFormat) };
				googleSheetsClient.CreateData(objectList, dataSheet);
			}

		}

		public static void AddUserFlatNumber(string flatNumber, User user)
		{
			var googleSheetsClient = GoogleSheetsClient.GetInstanse();

			var dataSheet = contactsDataSheet;

			var data = googleSheetsClient.ReadData(dataSheet);

			var idColIndex = data[0].ToList().IndexOf(idColName);

			var flatRow = data.FirstOrDefault(r => r.Count > idColIndex && r[idColIndex].ToString() == user.Id);
			var flatRowIndex = data.ToList().IndexOf(flatRow);

			if (flatRowIndex > 0)
			{
				var flatColIndex = data[0].ToList().IndexOf(flatColName);

				var flatNumbers = flatRow[flatColIndex].ToString();
				if (flatNumbers == "")
					flatNumbers = flatNumber.ToString();
				else if (!flatNumbers.Contains(flatNumber.ToString()))
				{
					var temp = flatNumbers.Split(',').ToList();
					temp.Add(flatNumber.ToString());
					flatNumbers = string.Join(",", temp);
				}

				var objectList = new List<object>() { flatNumbers };
				googleSheetsClient.UpdateData(flatRowIndex + 1, flatColIndex + 1, objectList, dataSheet);
				
				var dataColIndex = data[0].ToList().IndexOf(dateColName);
				objectList = new List<object>() { DateTime.Now.ToString(_dateFormat) };
				googleSheetsClient.UpdateData(flatRowIndex + 1, dataColIndex + 1, objectList, dataSheet);
			}
			else
			{
				var objectList = new List<object>() { user.Id, flatNumber, user.Name,"", DateTime.Now.ToString(_dateFormat) };
				googleSheetsClient.CreateData(objectList, dataSheet);
			}

		}
		public static void DeleteUserFlatNumber(string flatNumber, User user)
		{
			var googleSheetsClient = GoogleSheetsClient.GetInstanse();

			var dataSheet = contactsDataSheet;

			var data = googleSheetsClient.ReadData(dataSheet);

			var idColIndex = data[0].ToList().IndexOf(idColName);

			var flatRow = data.FirstOrDefault(r => r.Count > idColIndex && r[idColIndex].ToString() == user.Id);
			var flatRowIndex = data.ToList().IndexOf(flatRow);


			if (flatRowIndex > 0)
			{
				var flatColIndex = data[0].ToList().IndexOf(flatColName);
				var dataColIndex = data[0].ToList().IndexOf(dateColName);

				var flatNumbers = flatRow[flatColIndex].ToString().Split(',').ToList();
				flatNumbers.Remove(flatNumber.ToString());
				var flatNumbersResult = string.Join(",", flatNumbers);

				var objectList = new List<object>() { flatNumbersResult};
				googleSheetsClient.UpdateData(flatRowIndex + 1, flatColIndex +1, objectList, dataSheet);

				objectList = new List<object>() { DateTime.Now.ToString(_dateFormat) };
				googleSheetsClient.UpdateData(flatRowIndex + 1, dataColIndex + 1, objectList, dataSheet);

				//googleSheetsClient.DeleteDataRow(flatRowIndex+1, dataSheet);
			}
		}
	}
}
