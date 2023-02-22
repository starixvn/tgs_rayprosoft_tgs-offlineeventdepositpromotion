using Tgs.ServiceUtility;

namespace Tgs.Promotion.OfflineEvent.Deposit.ApplicationServices;

internal class HandleDepositedEventCommandService : ICommandService<HandleDepositedEventCommand>
{
	private readonly IGetService<ListBrandDepositEventHandlersQuery, IReadOnlyDictionary<Guid, BrandDepositEventHandler>> _listBrandHandlersService;

	public HandleDepositedEventCommandService(
		IGetService<ListBrandDepositEventHandlersQuery, IReadOnlyDictionary<Guid, BrandDepositEventHandler>> listBrandHandlersService)
	{
		_listBrandHandlersService = listBrandHandlersService ?? throw new ArgumentNullException(nameof(listBrandHandlersService));
	}

	public async ValueTask ExecuteAsync(HandleDepositedEventCommand command, CancellationToken cancellationToken = default)
	{
		var brandDict = await _listBrandHandlersService.GetAsync(
			new(),
			cancellationToken).ConfigureAwait(false);

		if (brandDict.TryGetValue(command.Event.BrandId, out var brandHandler))
			await brandHandler.OnDepositedAsync(
				command.Event.PlayerCode,
				command.Event.FormId,
				command.Event.HappenTime,
				command.Event.Amount,
				cancellationToken).ConfigureAwait(false);
	}
}
