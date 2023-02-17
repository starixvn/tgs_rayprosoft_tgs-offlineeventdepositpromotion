using System.Text.Json;

namespace Tgs.Promotion.OfflineEvent.Deposit;
public record OfflineDepositEvent(
	Guid Id,
	string PlayerCode,
	Guid BrandId,
	decimal Bonus,
	decimal BucketAmount,
	Guid DepositFormId,
	Guid PromotionId,
	JsonDocument Settings,
	DateTime CreatedOn);
