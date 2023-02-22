namespace Tgs.Promotion.OfflineEvent.Deposit;

public class DepositHierarchy
{
	public IEnumerable<DepositPhase> Phases { get; }

	public DepositHierarchy(IEnumerable<DepositPhase> phases)
	{
		Phases = phases ?? throw new ArgumentNullException(nameof(phases));
	}

	public decimal GetBonus(decimal depositAmount)
		=> Phases.Reverse()
			.Where(phase => phase.DepositCondition <= depositAmount)
			.Select(phase => phase.Bonus)
			.FirstOrDefault();
}
