using NSubstitute;
using Tgs.Promotion.Clients;
using Tgs.Promotion.OfflineEvent.Deposit;

namespace OfflineEventDepositPromotion.Core.UnitTests;

public class DepositEventHandlerTests
{
	[Fact]
	public async Task 儲值事件發生時PlayerCode不在名單內_什麼事都不會發生()
	{
		// Arrange
		var promotionId = Guid.NewGuid();
		var playerCode = "test1";
		var depositFormId = Guid.NewGuid();
		var settings = new DepositPromotionSettings(new HashSet<string>(), 100M, 1000M);

		var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

		var sut = new DepositEventHandler(promotionId, settings, fakePromotionServiceClient);

		// Act
		await sut.OnDepositedAsync(playerCode, depositFormId);

		// Assert
		_ = fakePromotionServiceClient.DidNotReceiveWithAnyArgs()
			.DistributePromotionBonusAsync(
				Arg.Any<Guid>(),
				Arg.Any<string>(),
				Arg.Any<decimal>(),
				Arg.Any<decimal>(),
				Arg.Any<string>(),
				Arg.Any<Guid?>());
	}

	[Fact]
	public async Task 儲值事件發生時PlayerCode在名單內就發放獎勵()
	{
		// Arrange
		var promotionId = Guid.NewGuid();
		var playerCode = "test1";
		var depositFormId = Guid.NewGuid();
		var settings = new DepositPromotionSettings(new HashSet<string> { "test1" }, 100M, 1000M);

		var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

		var sut = new DepositEventHandler(promotionId, settings, fakePromotionServiceClient);

		var planId = Guid.NewGuid();
		_ = fakePromotionServiceClient.GetPromotionDistributePlanAsync(Arg.Is(promotionId), Arg.Is(0L))
			.Returns(new PromotionDistributePlanInfo(planId, Guid.NewGuid(), promotionId, 0L));

		// Act
		await sut.OnDepositedAsync(playerCode, depositFormId);

		// Assert
		_ = fakePromotionServiceClient.Received(1)
			.DistributePromotionBonusAsync(
				Arg.Is(planId),
				Arg.Is(playerCode),
				Arg.Is(settings.BonusAmount),
				Arg.Is(settings.BucketAmount),
				Arg.Is(default(string)),
				Arg.Is(default(Guid?)));
	}
}
