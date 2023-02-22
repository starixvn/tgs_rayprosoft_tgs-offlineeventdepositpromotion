using System.Text.Json;
using Google.Protobuf;
using Tgs.OfflineEventDepositPromotion.MessageQueue;
using Tgs.Promotion.Clients.Protos;
using Tgs.Promotion.OfflineEvent.Deposit;

namespace Tgs.OfflineEventDepositPromotion.UnitTests;

public class OfflineDepositSettingProcessorTests
{
	[Fact]
	public async Task 解讀線下存款的設定值()
	{
		// Arrange
		var sut = new OfflineDepositSettingProcessor();

		var settings = new DepositPromotionSettings(
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

		var request = new CampaignDetailRequest
		{
			PromotionId = Guid.NewGuid(),
			Settings = JsonSerializer.Serialize(settings, OfflineDepositUtility.SerializerOptions)
		};

		// Act
		var response = CampaignDetailResponse.Parser.ParseFrom((await sut.HandleAsync(request.ToByteArray())).Span);
		var actual = JsonSerializer.Deserialize<DepositPromotionSettings>(
			response.CampaignDetail,
			OfflineDepositUtility.SerializerOptions);

		// Assert
		Assert.Equivalent(settings.Players, actual?.Players);
		Assert.Equal(settings.BucketRate, actual?.BucketRate);
		Assert.Equal(settings.Hierarchy.Phases, actual?.Hierarchy.Phases!);
	}
}
