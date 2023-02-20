using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tgs.Promotion.OfflineEvent.Deposit.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.Promotion.OfflineEvent.Deposit;

internal class EventBus : IEventSender
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<EventBus> _logger;

	private readonly Channel<DepositedEvent> _channel = Channel.CreateUnbounded<DepositedEvent>(new UnboundedChannelOptions
	{
		SingleReader = true
	});

	public EventBus(IServiceProvider serviceProvider, ILogger<EventBus> logger)
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public ValueTask PushEventAsync(DepositedEvent depositedEvent, CancellationToken cancellationToken = default)
		=> _channel.Writer.WriteAsync(depositedEvent, cancellationToken);

	public async Task HandleEventLoopAsync(CancellationToken cancellationToken = default)
	{
		var reader = _channel.Reader;
		while (await reader
			.WaitToReadAsync(cancellationToken)
			.ConfigureAwait(false))
		{
			if (reader.TryRead(out var evt))
				try
				{
					using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

					await using var scope = _serviceProvider.CreateAsyncScope();

					var svc = scope.ServiceProvider.GetRequiredService<ICommandService<HandleDepositedEventCommand>>();

					await HandleEventAsync(
						evt,
						svc,
						cts.Token).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Process command occur errors.");
				}
		}
	}

	private static ValueTask HandleEventAsync(
		DepositedEvent evt,
		ICommandService<HandleDepositedEventCommand> handleService,
		CancellationToken cancellationToken)
	{
		var command = new HandleDepositedEventCommand(evt);

		return handleService.ExecuteAsync(command, cancellationToken);
	}
}
