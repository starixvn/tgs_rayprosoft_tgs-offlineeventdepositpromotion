using NSubstitute;
using StackExchange.Redis;
using Tgs.Promotion.OfflineEvent.Deposit;
using Tgs.Promotion.OfflineEvent.Deposit.Repositories;

namespace OfflineEventDepositPromotion.Repositories.UnitTests;

public class RedisCacheStatisticsServiceTests
{
	[Fact]
	public async Task 檢查是否已發放獎勵會先嘗試在Redis中檢查如果未發放會再檢查DB()
	{
		// Arrange
		var fakeSourceService = Substitute.For<IStatisticsService>();
		var fakeRedisDatabase = Substitute.For<IDatabase>();

		var sut = new RedisCacheStatisticsService(fakeSourceService, fakeRedisDatabase);

		var promotionId = Guid.NewGuid();
		var playerCode = "test1";
		var promotionEndTime = DateTime.UtcNow.AddDays(1D);

		_ = fakeRedisDatabase.StringSetAsync(
			Arg.Is(new RedisKey($"Tgs:Promotion:OfflineDeposit:{promotionId}:{playerCode}")),
			Arg.Any<RedisValue>(),
			expiry: Arg.Any<TimeSpan?>(),
			when: Arg.Is(When.NotExists))
			.Returns(true);

		// Act
		_ = await sut.AchievePromotionAsync(promotionId, playerCode, promotionEndTime);

		// Assert
		_ = fakeSourceService.Received(1)
			.AchievePromotionAsync(
				Arg.Is(promotionId),
				Arg.Is(playerCode),
				Arg.Is(promotionEndTime));
	}

	[Fact]
	public async Task 檢查是否已發放獎勵如果Redis回傳已發放會直接回傳結果不再往DB檢查()
	{
		// Arrange
		var fakeSourceService = Substitute.For<IStatisticsService>();
		var fakeRedisDatabase = Substitute.For<IDatabase>();

		var sut = new RedisCacheStatisticsService(fakeSourceService, fakeRedisDatabase);

		var promotionId = Guid.NewGuid();
		var playerCode = "test1";

		_ = fakeRedisDatabase.StringSetAsync(
			Arg.Is(new RedisKey($"Tgs:Promotion:OfflineDeposit:{promotionId}:{playerCode}")),
			Arg.Any<RedisValue>(),
			when: Arg.Is(When.NotExists))
			.Returns(false);

		// Act
		var actual = await sut.AchievePromotionAsync(promotionId, playerCode, DateTime.UtcNow.AddDays(1D));

		// Assert
		Assert.True(actual);
		_ = fakeSourceService.DidNotReceive()
			.AchievePromotionAsync(
				Arg.Is(promotionId),
				Arg.Is(playerCode),
				Arg.Any<DateTime>());
	}
}
