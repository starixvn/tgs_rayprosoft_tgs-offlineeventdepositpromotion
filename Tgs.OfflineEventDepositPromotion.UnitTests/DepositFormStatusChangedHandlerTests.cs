using ExpectedObjects;
using Google.Protobuf;
using NSubstitute;
using Tgs.OfflineEventDepositPromotion.MessageQueue;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;
using Tgs.Promotion.OfflineEvent.Deposit;

namespace Tgs.OfflineEventDepositPromotion.UnitTests;

public class DepositFormStatusChangedHandlerTests
{
	[Fact]
	public async Task 儲值事件發生時會進行活動優惠判斷處理()
	{
		// Arrange
		var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
		var fakeEventSender = Substitute.For<IEventSender>();

		var sut = new DepositFormStatusChangedHandler(fakePaymentServiceClient, fakeEventSender);

		var eventData = new FormEventEx
		{
			BrandId = Guid.NewGuid(),
			FormId = Guid.NewGuid(),
			Status = nameof(PaymentFlowState.WaitApprove)
		};

		_ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(eventData.FormId.ToGuid()))
			.Returns(new DepositFormInfo
			{
				Id = eventData.FormId.ToGuid(),
				BrandId = eventData.BrandId,
				PlayerCode = "test1",
				ActualAmount = 200M,
				Status = PaymentFlowState.Approved
			});

		// Act
		await sut.HandleAsync(eventData.ToByteArray());

		// Assert
		_ = fakeEventSender.Received(1)
			.PushEventAsync(Arg.Is<DepositedEvent>(evt => new
			{
				PlayerCode = "test1",
				BrandId = eventData.BrandId.ToGuid(),
				FormId = eventData.FormId.ToGuid(),
				Amount = 200M
			}.ToExpectedObject().Matches(evt)));
	}
}
