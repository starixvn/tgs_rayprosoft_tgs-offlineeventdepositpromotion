namespace Tgs.Promotion.OfflineEvent.Deposit;
public record DepositPromotionSettings(
	HashSet<string> Players,
	decimal BucketRate,
	DepositHierarchy Hierarchy);
