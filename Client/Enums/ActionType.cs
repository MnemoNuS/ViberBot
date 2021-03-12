using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


namespace ViberBot.Client.Enums
{
	/// <summary>
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum ActionType
	{
		[EnumMember(Value = "send_data")]
		SendData = 1,
	}
}
