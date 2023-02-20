using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.Promotion.Clients;
using Tgs.Promotion.OfflineEvent.Deposit;
using Tgs.SerialNumberGenerator;

namespace OfflineEventDepositPromotion.Core.UnitTests;

public class BrandDepositEventHandlerTests
{
	[Fact]
	public async Task 儲值事件發生時將會把事件派發給Brand底下的所有活動()
	{
		// Arrange
		var brandId = Guid.NewGuid();

		var promotionId = Guid.NewGuid();
		var playerCode = "test1";
		var depositFormId = Guid.NewGuid();
		var settings = new DepositPromotionSettings(new HashSet<string> { "test1" }, 100M, 1000M);

		var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
		var fakeTgsRepository = Substitute.For<ITgsRepository>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
		var fakeStatisticsService = Substitute.For<IStatisticsService>();

		var handlers = new[]
		{
			new DepositEventHandler(
				promotionId,
				brandId,
				settings,
				fakePromotionServiceClient,
				fakeTgsRepository,
				fakeSerialNumberGenerator,
				fakeStatisticsService),
			new DepositEventHandler(
				promotionId,
				brandId,
				settings,
				fakePromotionServiceClient,
				fakeTgsRepository,
				fakeSerialNumberGenerator,
				fakeStatisticsService)
		};

		var sut = new BrandDepositEventHandler(
			brandId,
			handlers,
			NullLogger<BrandDepositEventHandler>.Instance);

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var planId = Guid.NewGuid();
		_ = fakePromotionServiceClient.GetPromotionDistributePlanAsync(Arg.Is(promotionId), Arg.Is(0L))
			.Returns(new PromotionDistributePlanInfo(planId, Guid.NewGuid(), promotionId, 0L));

		// Act
		await sut.OnDepositedAsync(playerCode, depositFormId);

		// Assert
		_ = fakePromotionServiceClient.Received(2)
			.DistributePromotionBonusAsync(
				Arg.Is(planId),
				Arg.Is(playerCode),
				Arg.Is(settings.BonusAmount),
				Arg.Is(settings.BucketAmount),
				Arg.Is(default(string)),
				Arg.Is(eventId));
	}
}
