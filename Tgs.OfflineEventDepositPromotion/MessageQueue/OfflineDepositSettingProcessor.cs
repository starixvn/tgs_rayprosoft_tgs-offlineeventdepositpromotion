using System.Text.Json;
using Google.Protobuf;
using Tgs.MessageQueue;
using Tgs.Promotion.Clients.Protos;
using Tgs.Promotion.OfflineEvent.Deposit;

namespace Tgs.OfflineEventDepositPromotion.MessageQueue;

public class OfflineDepositSettingProcessor : IMessageProcessor
{
	public ValueTask<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var request = CampaignDetailRequest.Parser.ParseFrom(data.Span);

		var settings = JsonSerializer.Deserialize<DepositPromotionSettings>(
			request.Settings,
			OfflineDepositUtility.SerializerOptions);

		if (settings is null)
			throw new InvalidOperationException("Settings is null");

		var response = new CampaignDetailResponse
		{
			CampaignDetail = JsonSerializer.Serialize(settings, OfflineDepositUtility.SerializerOptions)
		};

		return ValueTask.FromResult((ReadOnlyMemory<byte>)response.ToByteArray());
	}
}
