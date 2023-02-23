using NSubstitute;
using Tgs.Promotion.OfflineEvent.Deposit;

namespace OfflineEventDepositPromotion.Core.UnitTests;

public class StatisticsServiceTests
{
	[Fact]
	public async Task 確認是否發放過獎勵判斷會查詢資料庫是否已經有記錄()
	{
		// Arrange
		var fakeTgsRepository = Substitute.For<ITgsRepository>();

		var sut = new StatisticsService(fakeTgsRepository);

		var promotionId = Guid.NewGuid();
		var playerCode = "test1";

		// Act
		_ = await sut.AchievePromotionAsync(
			promotionId,
			playerCode,
			DateTime.UtcNow.AddDays(1D));

		// Assert
		_ = fakeTgsRepository.Received(1)
			.AchievePromotionAsync(Arg.Is(promotionId), Arg.Is(playerCode));
	}
}
