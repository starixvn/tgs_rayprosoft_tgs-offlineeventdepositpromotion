using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Tgs.Promotion.OfflineEvent.Deposit;

internal class PromotionDataService
{
	private readonly ReplaySubject<IReadOnlyDictionary<Guid, BrandDepositEventHandler>> _subject = new(1);

	public async ValueTask<IReadOnlyDictionary<Guid, BrandDepositEventHandler>> GetHandlersAsync(CancellationToken cancellationToken = default)
		=> await _subject.ToAsyncEnumerable()
			.FirstAsync(cancellationToken: cancellationToken)
			.ConfigureAwait(false);

	public void RenewHandlers(IReadOnlyDictionary<Guid, BrandDepositEventHandler> newHandlers)
		=> _subject.OnNext(newHandlers);
}
