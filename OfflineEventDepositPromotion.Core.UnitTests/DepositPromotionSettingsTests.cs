using System.Text.Json;
using Tgs.Promotion.OfflineEvent.Deposit;

namespace OfflineEventDepositPromotion.Core.UnitTests;

public class DepositPromotionSettingsTests
{
	[Fact]
	public void DepositPromotionSettings序列化()
	{
		// Arrange
		var sut = new DepositPromotionSettings(
			new HashSet<string>
			{
				"test1",
				"test2",
				"test3"
			},
			10M,
			new DepositHierarchy(new[] {
				new DepositPhase(100M, 20M),
				new DepositPhase(500M, 50M),
				new DepositPhase(1000M, 120M)
			}));

		var expected = """{"players":["test1","test2","test3"],"bucketRate":10,"hierarchy":{"phases":[{"condition":100,"bonus":20},{"condition":500,"bonus":50},{"condition":1000,"bonus":120}]}}""";

		// Act
		var actual = JsonSerializer.Serialize(sut, OfflineDepositUtility.SerializerOptions);

		// Assert
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void DepositPromotionSettings反序列化()
	{
		// Arrange
		var sut = """{"players":["test1","test2","test3"],"bucketRate":10,"hierarchy":{"phases":[{"condition":100,"bonus":20},{"condition":500,"bonus":50},{"condition":1000,"bonus":120}]}}""";

		var expected = new DepositPromotionSettings(
			new HashSet<string>
			{
				"test1",
				"test2",
				"test3"
			},
			10M,
			new DepositHierarchy(new[] {
				new DepositPhase(100M, 20M),
				new DepositPhase(500M, 50M),
				new DepositPhase(1000M, 120M)
			}));

		// Act
		var actual = JsonSerializer.Deserialize<DepositPromotionSettings>(sut, OfflineDepositUtility.SerializerOptions);

		// Assert
		Assert.Equivalent(expected.Players, actual?.Players);
		Assert.Equal(expected.BucketRate, actual?.BucketRate);
		Assert.Equal(expected.Hierarchy.Phases, actual?.Hierarchy.Phases!);
	}
}
