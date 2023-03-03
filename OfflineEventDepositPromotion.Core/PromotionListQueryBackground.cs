using Microsoft.Extensions.Hosting;
using Tgs.Promotion.OfflineEvent.Deposit.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.Promotion.OfflineEvent.Deposit;

internal class PromotionListQueryBackground : BackgroundService
{
	private readonly IGetService<ListBrandDepositEventHandlersQuery, IReadOnlyDictionary<Guid, BrandDepositEventHandler>> _listService;
	private readonly PromotionDataService _promotionDataService;

	public PromotionListQueryBackground(
		IGetService<ListBrandDepositEventHandlersQuery, IReadOnlyDictionary<Guid, BrandDepositEventHandler>> listService,
		PromotionDataService promotionDataService)
	{
		_listService = listService ?? throw new ArgumentNullException(nameof(listService));
		_promotionDataService = promotionDataService ?? throw new ArgumentNullException(nameof(promotionDataService));
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var timer = new PeriodicTimer(TimeSpan.FromMinutes(5D));

		do
		{
			_promotionDataService.RenewHandlers(await _listService.GetAsync(
				ListBrandDepositEventHandlersQuery.Default,
				stoppingToken).ConfigureAwait(false));
		} while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
	}
}
