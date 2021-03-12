using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace ViberBot.Client.Enums
{

	[JsonConverter(typeof(StringEnumConverter))]
	public enum DataField
	{
		[EnumMember(Value = "flat_number")]
		FlatNumber,
		[EnumMember(Value = "flat_value")]
		FlatValue,
		[EnumMember(Value = "contacts")]
		Contacts
	}

}
