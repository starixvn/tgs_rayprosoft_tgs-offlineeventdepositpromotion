using System.Text.Json;
using ExpectedObjects;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.Promotion.Clients;
using Tgs.Promotion.OfflineEvent.Deposit;
using Tgs.SerialNumberGenerator;

namespace OfflineEventDepositPromotion.Core.UnitTests;

public class DepositEventHandlerTests
{
	[Fact]
	public async Task 儲值事件發生時PlayerCode不在名單內_什麼事都不會發生()
	{
		// Arrange
		var promotionId = Guid.NewGuid();
		var brandId = Guid.NewGuid();
		var playerCode = "test1";
		var depositFormId = Guid.NewGuid();
		var settings = new DepositPromotionSettings(
			new HashSet<string>(),
			10M,
			new(Array.Empty<DepositPhase>()));

		var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
		var fakeTgsRepository = Substitute.For<ITgsRepository>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
		var fakeStatisticsService = Substitute.For<IStatisticsService>();

		var sut = new DepositEventHandler(
			promotionId,
			brandId,
			new EventRange(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1)),
			settings,
			fakePromotionServiceClient,
			fakeTgsRepository,
			fakeSerialNumberGenerator,
			fakeStatisticsService,
			NullLogger<DepositEventHandler>.Instance);

		// Act
		await sut.OnDepositedAsync(playerCode, depositFormId, 1000M);

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
	public async Task 儲值事件發生時玩家已經達到過就不處理()
	{
		// Arrange
		var promotionId = Guid.NewGuid();
		var brandId = Guid.NewGuid();
		var playerCode = "test1";
		var depositFormId = Guid.NewGuid();
		var settings = new DepositPromotionSettings(
			new HashSet<string> { "test1" },
			10M,
			new(Array.Empty<DepositPhase>()));

		var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
		var fakeTgsRepository = Substitute.For<ITgsRepository>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
		var fakeStatisticsService = Substitute.For<IStatisticsService>();

		var sut = new DepositEventHandler(
			promotionId,
			brandId,
			new EventRange(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1)),
			settings,
			fakePromotionServiceClient,
			fakeTgsRepository,
			fakeSerialNumberGenerator,
			fakeStatisticsService,
			NullLogger<DepositEventHandler>.Instance);

		_ = fakeStatisticsService.AchievePromotionAsync(
			Arg.Is(promotionId),
			Arg.Is(playerCode),
			Arg.Any<DateTime>())
			.Returns(true);

		// Act
		await sut.OnDepositedAsync(playerCode, depositFormId, 1000M);

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
		var brandId = Guid.NewGuid();
		var playerCode = "test1";
		var depositFormId = Guid.NewGuid();
		var settings = new DepositPromotionSettings(
			new HashSet<string> { "test1" },
			10M,
			new(new[] { new DepositPhase(100M, 20M) }));

		var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
		var fakeTgsRepository = Substitute.For<ITgsRepository>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
		var fakeStatisticsService = Substitute.For<IStatisticsService>();

		var sut = new DepositEventHandler(
			promotionId,
			brandId,
			new EventRange(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1)),
			settings,
			fakePromotionServiceClient,
			fakeTgsRepository,
			fakeSerialNumberGenerator,
			fakeStatisticsService,
			NullLogger<DepositEventHandler>.Instance);

		_ = fakeStatisticsService.AchievePromotionAsync(
			Arg.Is(promotionId),
			Arg.Is(playerCode),
			Arg.Any<DateTime>())
			.Returns(false);

		var eventId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(eventId);

		var planId = Guid.NewGuid();
		_ = fakePromotionServiceClient.GetPromotionDistributePlanAsync(Arg.Is(promotionId), Arg.Is(0L))
			.Returns(new PromotionDistributePlanInfo(planId, Guid.NewGuid(), promotionId, 0L));

		var expectedEventData = new OfflineDepositEvent(
			eventId,
			playerCode,
			brandId,
			20M,
			200M,
			depositFormId,
			promotionId,
			JsonSerializer.SerializeToDocument(settings, OfflineDepositUtility.SerializerOptions),
			DateTime.UtcNow);

		// Act
		await sut.OnDepositedAsync(playerCode, depositFormId, 1000M);

		// Assert
		var match = expectedEventData.ToExpectedObject(ctx =>
		{
			_ = ctx.IgnoreEqualsOverride();
			_ = ctx
				.Ignore(evt => evt.Settings)
				.Ignore(evt => evt.CreatedOn);
		});
		_ = fakeTgsRepository.Received(1)
			.AddOfflineDepositEventAsync(Arg.Is<OfflineDepositEvent>(p => match.Matches(p)));

		_ = fakePromotionServiceClient.Received(1)
			.DistributePromotionBonusAsync(
				Arg.Is(planId),
				Arg.Is(playerCode),
				Arg.Is(20M),
				Arg.Is(200M),
				Arg.Is(default(string)),
				Arg.Is(eventId));
	}

	[Fact]
	public async Task 如果儲值金沒有符合最低資格不會發放獎勵()
	{
		// Arrange
		var promotionId = Guid.NewGuid();
		var brandId = Guid.NewGuid();
		var playerCode = "test1";
		var depositFormId = Guid.NewGuid();
		var settings = new DepositPromotionSettings(
			new HashSet<string> { "test1" },
			10M,
			new(new[] { new DepositPhase(100M, 20M) }));

		var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
		var fakeTgsRepository = Substitute.For<ITgsRepository>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
		var fakeStatisticsService = Substitute.For<IStatisticsService>();

		var sut = new DepositEventHandler(
			promotionId,
			brandId,
			new EventRange(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1)),
			settings,
			fakePromotionServiceClient,
			fakeTgsRepository,
			fakeSerialNumberGenerator,
			fakeStatisticsService,
			NullLogger<DepositEventHandler>.Instance);

		_ = fakeStatisticsService.AchievePromotionAsync(
			Arg.Is(promotionId),
			Arg.Is(playerCode),
			Arg.Any<DateTime>())
			.Returns(false);

		// Act
		await sut.OnDepositedAsync(playerCode, depositFormId, 50M);

		// Assert

		_ = fakePromotionServiceClient.DidNotReceiveWithAnyArgs()
			.DistributePromotionBonusAsync(
				Arg.Any<Guid>(),
				Arg.Is(playerCode),
				Arg.Any<decimal>(),
				Arg.Any<decimal>(),
				Arg.Is(default(string)),
				Arg.Any<Guid>());
	}
}
