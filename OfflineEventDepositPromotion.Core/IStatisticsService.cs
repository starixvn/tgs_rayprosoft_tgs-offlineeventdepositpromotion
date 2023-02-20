namespace Tgs.Promotion.OfflineEvent.Deposit;

public interface IStatisticsService
{
	ValueTask<bool> AchievePromotionAsync(Guid promotionId, string playerCode, CancellationToken cancellationToken = default);
}
