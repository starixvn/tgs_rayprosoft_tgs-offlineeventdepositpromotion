using Tgs.Promotion.OfflineEvent.Deposit;

namespace OfflineEventDepositPromotion.Core.UnitTests;

public class DepositHierarchyTests
{
	[Fact]
	public void 依照儲值金額取得獎勵金額會找出能符合的最高獎勵()
	{
		// Arrange
		var sut = new DepositHierarchy(
			new[] {
				new DepositPhase(100M, 20M),
				new DepositPhase(500M, 50M),
				new DepositPhase(1000M, 120M)
			});

		// Act
		var actual = sut.GetBonus(600M);

		// Assert
		Assert.Equal(50M, actual);
	}

	[Fact]
	public void 如果儲值金額沒有符合資格_獎勵金會是0()
	{
		// Arrange
		var sut = new DepositHierarchy(
			new[] {
				new DepositPhase(100M, 20M),
				new DepositPhase(500M, 50M),
				new DepositPhase(1000M, 120M)
			});

		// Act
		var actual = sut.GetBonus(50M);

		// Assert
		Assert.Equal(0M, actual);
	}
}
