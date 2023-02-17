using Tgs.MessageQueue;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;

namespace Tgs.OfflineEventDepositPromotion.MessageQueue;

public class DepositFormStatusChangedHandler : IMessageHandler
{
	private readonly IPaymentServiceClient _paymentServiceClient;

	public DepositFormStatusChangedHandler(IPaymentServiceClient paymentServiceClient)
	{
		_paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
	}

	public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var formEvent = FormEventEx.Parser.ParseFrom(data.Span);
		var previousStatus = Enum.Parse<PaymentFlowState>(formEvent.Status);

		var depositForm = await _paymentServiceClient.GetDepositFormAsync(
			formEvent.FormId,
			cancellationToken).ConfigureAwait(false);

		if (depositForm is null)
			return;

		if (previousStatus != depositForm.Status
			&& depositForm.Status == PaymentFlowState.Approved)
		{
			// TODO: 觸發儲值獎勵判斷
		}
	}
}
