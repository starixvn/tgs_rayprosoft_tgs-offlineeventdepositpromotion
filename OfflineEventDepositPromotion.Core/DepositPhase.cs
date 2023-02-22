using System.Text.Json.Serialization;

namespace Tgs.Promotion.OfflineEvent.Deposit;
public record DepositPhase(
	[property: JsonPropertyName("condition")] decimal DepositCondition,
	decimal Bonus);
