using Microsoft.Extensions.DependencyInjection;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Promotion.OfflineEvent.Deposit.ApplicationServices;

internal class ListBrandDepositEventHandlersQueryService : IGetService<ListBrandDepositEventHandlersQuery, IReadOnlyDictionary<Guid, BrandDepositEventHandler>>
{
	private readonly IPromotionServiceClient _promotionServiceClient;
	private readonly IServiceProvider _serviceProvider;

	public ListBrandDepositEventHandlersQueryService(IPromotionServiceClient promotionServiceClient, IServiceProvider serviceProvider)
	{
		_promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
	}

	public async ValueTask<IReadOnlyDictionary<Guid, BrandDepositEventHandler>> GetAsync(
		ListBrandDepositEventHandlersQuery query,
		CancellationToken cancellationToken = default)
		=> (await _promotionServiceClient.ListPromotionsByPromotionTypeAsync("OfflineDepositReward", cancellationToken)
			.Where(info => new EventRange(
				info.EventDuring.From.AddHours(-1D),
				info.EventDuring.To.AddHours(1D)).InTheRange(DateTime.UtcNow))
			.Select(info => _promotionServiceClient
				.GetPromotionRuleAsync<PromotionRule>(info.Id, cancellationToken)
				.AsTask()
				.ToAsyncEnumerable()
				.Where(rule => rule is not null)
				.Select(rule => ActivatorUtilities.CreateInstance<DepositEventHandler>(
					_serviceProvider,
					info.Id,
					info.BrandId,
					new EventRange(info.EventDuring.From, info.EventDuring.To),
					rule!.CompaignDetail)))
			.Merge()
			.GroupBy(handler => handler.BrandId)
			.Select(g => ActivatorUtilities.CreateInstance<BrandDepositEventHandler>(
				_serviceProvider,
				g.Key,
				g.ToEnumerable()))
			.ToDictionaryAsync(brandHandler => brandHandler.BrandId, cancellationToken: cancellationToken)
			.ConfigureAwait(false)).AsReadOnly();

	internal record PromotionRule(DepositPromotionSettings CompaignDetail);
}
