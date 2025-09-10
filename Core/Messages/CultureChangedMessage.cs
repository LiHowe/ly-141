using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Messages
{
	public class CultureChangedMessage
	{
		public string CultureCode { get; }

		public CultureChangedMessage(string cultureCode)
		{
			CultureCode = cultureCode;
		}
	}
}
