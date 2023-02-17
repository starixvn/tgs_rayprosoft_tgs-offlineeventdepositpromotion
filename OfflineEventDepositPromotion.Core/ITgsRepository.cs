namespace Tgs.Promotion.OfflineEvent.Deposit;

public interface ITgsRepository
{
	ValueTask AddOfflineDepositEventAsync(OfflineDepositEvent eventData, CancellationToken cancellationToken = default);
}
