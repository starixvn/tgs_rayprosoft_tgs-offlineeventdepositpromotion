using StackExchange.Redis;

namespace Tgs.Promotion.OfflineEvent.Deposit.Repositories;

internal class RedisCacheStatisticsService : IStatisticsService
{
	private readonly IStatisticsService _source;
	private readonly IDatabase _redisDatabase;

	public RedisCacheStatisticsService(IStatisticsService source, IDatabase redisDatabase)
	{
		_source = source ?? throw new ArgumentNullException(nameof(source));
		_redisDatabase = redisDatabase ?? throw new ArgumentNullException(nameof(redisDatabase));
	}

	public async ValueTask<bool> AchievePromotionAsync(
		Guid promotionId,
		string playerCode,
		DateTime promotionEndTime,
		CancellationToken cancellationToken = default)
	{
		var key = $"Tgs:Promotion:OfflineDeposit:{promotionId}:{playerCode}";
		var expiry = promotionEndTime.ToUniversalTime().AddDays(1D) - DateTime.UtcNow;

		var isSet = await _redisDatabase.StringSetAsync(
			key,
			Environment.MachineName,
			expiry: expiry,
			when: When.NotExists).ConfigureAwait(false);

		return !isSet
			? !isSet
			: await _source.AchievePromotionAsync(
				promotionId,
				playerCode,
				promotionEndTime,
				cancellationToken).ConfigureAwait(false);
	}
}
