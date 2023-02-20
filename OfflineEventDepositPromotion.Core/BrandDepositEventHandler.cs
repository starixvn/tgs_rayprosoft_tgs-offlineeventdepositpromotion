using Microsoft.Extensions.Logging;

namespace Tgs.Promotion.OfflineEvent.Deposit;

internal class BrandDepositEventHandler
{
	private readonly Guid _brandId;
	private readonly IEnumerable<DepositEventHandler> _eventHandlers;
	private readonly ILogger<BrandDepositEventHandler> _logger;

	public BrandDepositEventHandler(Guid brandId, IEnumerable<DepositEventHandler> eventHandlers, ILogger<BrandDepositEventHandler> logger)
	{
		_brandId = brandId;
		_eventHandlers = eventHandlers ?? throw new ArgumentNullException(nameof(eventHandlers));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async ValueTask OnDepositedAsync(string playerCode, Guid formId, CancellationToken cancellationToken = default)
	{
		foreach (var handler in _eventHandlers)
			try
			{
				await handler.OnDepositedAsync(
					playerCode,
					formId,
					cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Brand({brandId}) Handle event occur error.", _brandId);
			}
	}
}
