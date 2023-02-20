using Microsoft.Extensions.Hosting;

namespace Tgs.Promotion.OfflineEvent.Deposit;

internal class DepositEventBackground : BackgroundService
{
	private readonly EventBus _eventBus;

	public DepositEventBackground(EventBus eventBus)
	{
		_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
	}

	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		return _eventBus.HandleEventLoopAsync(stoppingToken);
	}
}
