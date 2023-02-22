using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tgs.Promotion.OfflineEvent.Deposit;
public record EventRange(DateTime Start, DateTime End)
{
	public bool InTheRange(DateTime time)
		=> time >= Start && time <= End;

}
