using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ViberBotLib.Enums;

namespace ViberBot.Services
{
	public class WebhookService : IWebhookService
	{
		public WebhookService()	{}
		public async Task<ICollection<EventType>> SetWebhookAsync(string url)
		{
			return await ViberBotClient.GetInstance().SetWebhookAsync($"{url}/api/Webhook");
		}
	}
}
