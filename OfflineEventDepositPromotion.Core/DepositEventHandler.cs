using Tgs.Promotion.Clients;

namespace Tgs.Promotion.OfflineEvent.Deposit;

internal class DepositEventHandler
{
	private readonly Guid _promotionId;
	private readonly DepositPromotionSettings _settings;
	private readonly IPromotionServiceClient _promotionServiceClient;

	public DepositEventHandler(Guid promotionId, DepositPromotionSettings settings, IPromotionServiceClient promotionServiceClient)
	{
		_promotionId = promotionId;
		_settings = settings ?? throw new ArgumentNullException(nameof(settings));
		_promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));
	}

	public async ValueTask OnDepositedAsync(string playerCode, Guid formId, CancellationToken cancellationToken = default)
	{
		if (!_settings.Players.Contains(playerCode))
			return;

		var distPlan = await _promotionServiceClient.GetPromotionDistributePlanAsync(
			_promotionId,
			0L,
			cancellationToken).ConfigureAwait(false);

		_ = await _promotionServiceClient.DistributePromotionBonusAsync(
			distPlan.Id,
			playerCode,
			_settings.BonusAmount,
			_settings.BucketAmount,
			null,
			null,
			cancellationToken).ConfigureAwait(false);
	}
}
