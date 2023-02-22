using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.Promotion.Clients;
using Tgs.Promotion.OfflineEvent.Deposit;
using Tgs.Promotion.OfflineEvent.Deposit.ApplicationServices;
using Tgs.ServiceUtility;

namespace OfflineEventDepositPromotion.Core.UnitTests;

using ListBrandHandlersQueryService = IGetService<
	ListBrandDepositEventHandlersQuery,
	IReadOnlyDictionary<Guid, BrandDepositEventHandler>>;

public class DepositProcessTests
{
	[Fact]
	public async Task 處理存款事件會根據BrandId來取得Brand的處理器執行()
	{
		// Arrange
		var fakeListBrandHandlersQueryService = Substitute.For<ListBrandHandlersQueryService>();

		var sut = new HandleDepositedEventCommandService(fakeListBrandHandlersQueryService);

		var command = new HandleDepositedEventCommand(
			   new DepositedEvent("test1", Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow));

		var fakeBrandHandler = Substitute.For<BrandDepositEventHandler>(
			command.Event.BrandId,
			Array.Empty<DepositEventHandler>(),
			NullLogger<BrandDepositEventHandler>.Instance);

		_ = fakeListBrandHandlersQueryService.GetAsync(Arg.Is(ListBrandDepositEventHandlersQuery.Default))
			.Returns(new Dictionary<Guid, BrandDepositEventHandler>
			{
				[command.Event.BrandId] = fakeBrandHandler
			}.AsReadOnly());

		// Act
		await sut.ExecuteAsync(command);

		// Assert
		_ = fakeBrandHandler.Received(1)
			.OnDepositedAsync(
				Arg.Is(command.Event.PlayerCode),
				Arg.Is(command.Event.FormId),
				Arg.Is(command.Event.HappenTime));
	}

	[Fact]
	public async Task 取得系統中所有的線下存款活動處理器()
	{
		// Arrange
		var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
		var fakeServiceProvider = new ServiceCollection().BuildServiceProvider();

		var sut = new ListBrandDepositEventHandlersQueryService(
			fakePromotionServiceClient,
			fakeServiceProvider);

		// Act
		_ = await sut.GetAsync(ListBrandDepositEventHandlersQuery.Default);

		// Assert
		_ = fakePromotionServiceClient.Received(1)
			.ListPromotionsByPromotionTypeAsync(Arg.Is("OfflineDepositReward"));
	}

	[Fact]
	public async Task 取得系統中所有的線下存款活動處理器_會過濾掉開始時間前一個小時及結束時間後一個小時的資料()
	{
		// Arrange
		var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
		var fakeServiceProvider = new ServiceCollection().BuildServiceProvider();

		var sut = new ListBrandDepositEventHandlersQueryService(
			fakePromotionServiceClient,
			fakeServiceProvider);

		var brandId = Guid.NewGuid();

		var promotions = new[] {
			new PromotionMainInfo
			{
				Id = Guid.NewGuid(),
				BrandId = brandId,
				EventDuring = (DateTime.UtcNow.AddHours(1.1D), DateTime.UtcNow.AddDays(1D))
			},
			new PromotionMainInfo
			{
				Id = Guid.NewGuid(),
				BrandId = brandId,
				EventDuring = (DateTime.UtcNow, DateTime.UtcNow.AddDays(1D))
			},
			new PromotionMainInfo
			{
				Id = Guid.NewGuid(),
				BrandId = brandId,
				EventDuring = (DateTime.UtcNow, DateTime.UtcNow.AddHours(-1D))
			}
		};

		_ = fakePromotionServiceClient.ListPromotionsByPromotionTypeAsync(Arg.Is("OfflineDepositReward"))
			.Returns(promotions.ToAsyncEnumerable());

		// Act
		_ = await sut.GetAsync(ListBrandDepositEventHandlersQuery.Default);

		// Assert
		_ = fakePromotionServiceClient.Received(1)
			.GetPromotionRuleAsync<DepositPromotionSettings>(Arg.Any<Guid>());
		_ = fakePromotionServiceClient.Received(1)
			.GetPromotionRuleAsync<DepositPromotionSettings>(Arg.Is(promotions[1].Id));
	}
}
