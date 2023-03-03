using Tgs.ServiceUtility;

namespace Tgs.Promotion.OfflineEvent.Deposit.ApplicationServices;

internal class HandleDepositedEventCommandService : ICommandService<HandleDepositedEventCommand>
{
	private readonly PromotionDataService _promotionDataService;

	public HandleDepositedEventCommandService(
		PromotionDataService promotionDataService)
	{
		_promotionDataService = promotionDataService ?? throw new ArgumentNullException(nameof(promotionDataService));
	}

	public async ValueTask ExecuteAsync(HandleDepositedEventCommand command, CancellationToken cancellationToken = default)
	{
		var brandDict = await _promotionDataService
			.GetHandlersAsync(cancellationToken)
			.ConfigureAwait(false);

		if (brandDict.TryGetValue(command.Event.BrandId, out var brandHandler))
			await brandHandler.OnDepositedAsync(
				command.Event.PlayerCode,
				command.Event.FormId,
				command.Event.HappenTime,
				command.Event.Amount,
				cancellationToken).ConfigureAwait(false);
	}
}
