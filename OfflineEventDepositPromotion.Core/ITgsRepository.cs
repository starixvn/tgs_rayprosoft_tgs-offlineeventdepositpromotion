namespace Tgs.Promotion.OfflineEvent.Deposit;

public interface ITgsRepository
{
	ValueTask AddOfflineDepositEventAsync(OfflineDepositEvent eventData, CancellationToken cancellationToken = default);

	ValueTask<bool> AchievePromotionAsync(Guid promotionId, string playerCode, CancellationToken cancellationToken = default);
}
