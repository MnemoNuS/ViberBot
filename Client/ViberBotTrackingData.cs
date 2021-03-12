using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ViberBot.Client
{
	public class ViberBotTrackingData
	{

		public string PrevAction;
		public string NextAction;
		public Dictionary<string, string> Data;

		private ViberBotTrackingData()
		{
			PrevAction = "DefaultAction";
			NextAction = "DefaultAction";
			Data = new Dictionary<string, string>();
		}
		public static string Build(string prevAction, string nextAction, Dictionary<string, string> data = null)
		{
			var td = new ViberBotTrackingData();
			td.PrevAction = prevAction;
			td.NextAction = nextAction;
			if (data != null)
				td.Data = data;

			return JsonConvert.SerializeObject(td);
		}

		public static string Empty()
		{
			var td = new ViberBotTrackingData();
			return JsonConvert.SerializeObject(td);
		}

		public static ViberBotTrackingData Get(string trackingData)
		{
			return JsonConvert.DeserializeObject<ViberBotTrackingData>(trackingData);
		}

		public void Add(string key, string value)
		{
			if(Data.TryGetValue(key, out var v))
			{
				Data[key] = value;
			}
			else
			{
				Data.Add(key, value);
			}
		}
	}
}
