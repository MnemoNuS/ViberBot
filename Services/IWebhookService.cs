using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ViberBotLib.Enums;

namespace ViberBot.Services
{
	public interface IWebhookService
	{
		Task<ICollection<EventType>> SetWebhookAsync(string url);
	}
}
