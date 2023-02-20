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
			.OnDepositedAsync(Arg.Is(command.Event.PlayerCode), Arg.Is(command.Event.FormId));
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
}
