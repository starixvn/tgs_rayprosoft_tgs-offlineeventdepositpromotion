using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.Promotion.Clients;
using Tgs.Promotion.OfflineEvent.Deposit;
using Tgs.SerialNumberGenerator;

namespace OfflineEventDepositPromotion.Core.UnitTests;

public class BrandDepositEventHandlerTests
{
	[Fact]
	public async Task 儲值事件發生時將會把事件派發給Brand底下有效時間的所有活動()
	{
		// Arrange
		var brandId = Guid.NewGuid();

		var promotionId = Guid.NewGuid();
		var playerCode = "test1";
		var depositFormId = Guid.NewGuid();
		var settings = new DepositPromotionSettings(
			new HashSet<string> { "test1" },
			10M,
			new(new[] { new DepositPhase(100M, 20M) }));

		var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>(Array.Empty<DepositPhase>());
		var fakeTgsRepository = Substitute.For<ITgsRepository>(Array.Empty<DepositPhase>());
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>(Array.Empty<DepositPhase>());
		var fakeStatisticsService = Substitute.For<IStatisticsService>(Array.Empty<DepositPhase>());

		var handlers = new[]
		{
			new DepositEventHandler(
				promotionId,
				brandId,
				new EventRange(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1)),
				settings,
				fakePromotionServiceClient,
				fakeTgsRepository,
				fakeSerialNumberGenerator,
				fakeStatisticsService,
				NullLogger<DepositEventHandler>.Instance),
			new DepositEventHandler(
				promotionId,
				brandId,
				new EventRange(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1)),
				settings,
				fakePromotionServiceClient,
				fakeTgsRepository,
				fakeSerialNumberGenerator,
				fakeStatisticsService,
				NullLogger < DepositEventHandler >.Instance)
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
		await sut.OnDepositedAsync(playerCode, depositFormId, DateTime.Now, 1000M);

		// Assert
		_ = fakePromotionServiceClient.Received(2)
			.DistributePromotionBonusAsync(
				Arg.Is(planId),
				Arg.Is(playerCode),
				Arg.Is(20M),
				Arg.Is(200M),
				Arg.Is(default(string)),
				Arg.Is(eventId));
	}

	[Fact]
	public async Task 儲值事件發生時只會把事件派發給在有效時間內的活動()
	{
		// Arrange
		var brandId = Guid.NewGuid();

		var promotionId = Guid.NewGuid();
		var playerCode = "test1";
		var depositFormId = Guid.NewGuid();
		var settings = new DepositPromotionSettings(
			new HashSet<string> { "test1" },
			10M,
			new(new[] { new DepositPhase(100M, 20M) }));

		var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>(Array.Empty<DepositPhase>());
		var fakeTgsRepository = Substitute.For<ITgsRepository>(Array.Empty<DepositPhase>());
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>(Array.Empty<DepositPhase>());
		var fakeStatisticsService = Substitute.For<IStatisticsService>(Array.Empty<DepositPhase>());

		var handlers = new[]
		{
			new DepositEventHandler(
				promotionId,
				brandId,
				new EventRange(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1)),
				settings,
				fakePromotionServiceClient,
				fakeTgsRepository,
				fakeSerialNumberGenerator,
				fakeStatisticsService,
				NullLogger < DepositEventHandler >.Instance),
			new DepositEventHandler(
				promotionId,
				brandId,
				new EventRange(DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1)),
				settings,
				fakePromotionServiceClient,
				fakeTgsRepository,
				fakeSerialNumberGenerator,
				fakeStatisticsService,
				NullLogger<DepositEventHandler>.Instance)
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
		await sut.OnDepositedAsync(playerCode, depositFormId, DateTime.Now, 1000M);

		// Assert
		_ = fakePromotionServiceClient.Received(1)
			.DistributePromotionBonusAsync(
				Arg.Is(planId),
				Arg.Is(playerCode),
				Arg.Is(20M),
				Arg.Is(200M),
				Arg.Is(default(string)),
				Arg.Is(eventId));
	}
}
