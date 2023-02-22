using System.Text.Json;
using Tgs.Promotion.Clients;
using Tgs.SerialNumberGenerator;

namespace Tgs.Promotion.OfflineEvent.Deposit;

internal class DepositEventHandler
{
	private readonly Guid _promotionId;
	private readonly DepositPromotionSettings _settings;
	private readonly IPromotionServiceClient _promotionServiceClient;
	private readonly ITgsRepository _tgsRepository;
	private readonly ISerialNumberGenerator _serialNumberGenerator;
	private readonly IStatisticsService _statisticsService;

	public Guid BrandId { get; }

	public EventRange EventRange { get; }

	public DepositEventHandler(
		Guid promotionId,
		Guid brandId,
		EventRange eventRange,
		DepositPromotionSettings settings,
		IPromotionServiceClient promotionServiceClient,
		ITgsRepository tgsRepository,
		ISerialNumberGenerator serialNumberGenerator,
		IStatisticsService statisticsService)
	{
		_promotionId = promotionId;
		BrandId = brandId;
		EventRange = eventRange ?? throw new ArgumentNullException(nameof(eventRange));
		_settings = settings ?? throw new ArgumentNullException(nameof(settings));
		_promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
		_serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));
		_statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
	}

	public async ValueTask OnDepositedAsync(string playerCode, Guid formId, CancellationToken cancellationToken = default)
	{
		if (!_settings.Players.Contains(playerCode))
			return;

		var isAchieve = await _statisticsService.AchievePromotionAsync(
			_promotionId,
			playerCode,
			cancellationToken).ConfigureAwait(false);

		if (isAchieve)
			return;

		var eventId = await _serialNumberGenerator
			.GenerateAsync(cancellationToken)
			.ConfigureAwait(false);

		var eventData = new OfflineDepositEvent(
			eventId,
			playerCode,
			BrandId,
			_settings.BonusAmount,
			_settings.BucketAmount,
			formId,
			_promotionId,
			JsonSerializer.SerializeToDocument(_settings, OfflineDepositUtility.SerializerOptions),
			DateTime.UtcNow);

		await _tgsRepository.AddOfflineDepositEventAsync(
			eventData,
			cancellationToken).ConfigureAwait(false);

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
			eventId,
			cancellationToken).ConfigureAwait(false);
	}
}
