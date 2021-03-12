using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ViberBot.Client
{
	public class ClientSettings
	{
		public string Domain { get; set; }
		public string Token { get; set; }
		public string ApplicationName { get; set; }
		public string SpreadsheetId { get; set; }
		public int FlatsNumber { get; set; }
		public DataReadingPeriod DataReadingPeriod { get; set; }
		public AppFile HelpFile { get; set; }
		public ClientSettings() { }
	}

	public class DataReadingPeriod
	{
		public int From { get; set; }
		public int To { get; set; }
		public DataReadingPeriod()
		{
			From = 1;
			To = 31;
		}

	}
	public class AppFile
	{
		public string FileName{ get; set; }
		public string Size { get; set; }
		public string Url { get; set; }

	}
}
