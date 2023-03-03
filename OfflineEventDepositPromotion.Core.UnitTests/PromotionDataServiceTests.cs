using Microsoft.Extensions.Logging.Abstractions;
using Tgs.Promotion.OfflineEvent.Deposit;

namespace OfflineEventDepositPromotion.Core.UnitTests;

public class PromotionDataServiceTests
{
	[Fact]
	public async Task 如果沒有更新活動列表會等待取得資料()
	{
		// Arrange
		var sut = new PromotionDataService();

		// Act
		var actual = await Assert.ThrowsAsync<TimeoutException>(
			async () => await sut.GetHandlersAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(1D)));

		// Assert
		Assert.NotNull(actual);
	}

	[Fact]
	public async Task 取得活動列表會得到最後更新的結果()
	{
		// Arrange
		var sut = new PromotionDataService();

		sut.RenewHandlers(new Dictionary<Guid, BrandDepositEventHandler>().AsReadOnly());
		sut.RenewHandlers(new Dictionary<Guid, BrandDepositEventHandler>
		{
			[Guid.NewGuid()] = new BrandDepositEventHandler(
				Guid.NewGuid(),
				Array.Empty<DepositEventHandler>(),
				NullLogger<BrandDepositEventHandler>.Instance)
		}.AsReadOnly());

		// Act
		var actual = await sut.GetHandlersAsync();

		// Assert
		Assert.Equal(1, actual.Count);
	}
}
