namespace Tgs.Promotion.OfflineEvent.Deposit;
public record DepositedEvent(
	string PlayerCode,
	Guid BrandId,
	Guid FormId,
	DateTime HappenTime);
