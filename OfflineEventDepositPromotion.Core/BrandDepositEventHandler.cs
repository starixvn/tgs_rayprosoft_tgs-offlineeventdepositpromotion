using Microsoft.Extensions.Logging;

namespace Tgs.Promotion.OfflineEvent.Deposit;

internal class BrandDepositEventHandler
{
	private readonly IEnumerable<DepositEventHandler> _eventHandlers;
	private readonly ILogger<BrandDepositEventHandler> _logger;

	public Guid BrandId { get; }

	public BrandDepositEventHandler(Guid brandId, IEnumerable<DepositEventHandler> eventHandlers, ILogger<BrandDepositEventHandler> logger)
	{
		BrandId = brandId;
		_eventHandlers = eventHandlers ?? throw new ArgumentNullException(nameof(eventHandlers));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public virtual async ValueTask OnDepositedAsync(
		string playerCode,
		Guid formId,
		DateTime happenTime,
		CancellationToken cancellationToken = default)
	{
		foreach (var handler in _eventHandlers)
			try
			{
				if (handler.EventRange.InTheRange(happenTime))
					await handler.OnDepositedAsync(
						playerCode,
						formId,
						cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Brand({brandId}) Handle event occur error.", BrandId);
			}
	}
}
