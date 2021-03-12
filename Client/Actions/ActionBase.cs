using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ViberBot.Client.Actions
{
	public abstract class ActionBase
	{
		public static ViberBotClient client => ViberBotClient.GetInstance();
	}
}
