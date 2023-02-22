using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.Promotion.OfflineEvent.Deposit;

public static class OfflineDepositUtility
{
	public static readonly JsonSerializerOptions SerializerOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
		Converters = {
			new JsonStringEnumConverter()
		}
	};
}
