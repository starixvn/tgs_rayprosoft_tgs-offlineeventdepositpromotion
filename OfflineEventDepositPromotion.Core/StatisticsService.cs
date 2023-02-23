namespace Tgs.Promotion.OfflineEvent.Deposit;

public class StatisticsService : IStatisticsService
{
	private readonly ITgsRepository _tgsRepository;

	public StatisticsService(ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository;
	}

	public ValueTask<bool> AchievePromotionAsync(
		Guid promotionId,
		string playerCode,
		CancellationToken cancellationToken = default)
		=> _tgsRepository.AchievePromotionAsync(
			promotionId,
			playerCode,
			cancellationToken);
}
