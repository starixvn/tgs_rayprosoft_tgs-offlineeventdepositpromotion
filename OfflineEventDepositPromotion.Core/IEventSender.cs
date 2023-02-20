namespace Tgs.Promotion.OfflineEvent.Deposit;

public interface IEventSender
{
	ValueTask PushEventAsync(DepositedEvent depositedEvent, CancellationToken cancellationToken = default);
}
