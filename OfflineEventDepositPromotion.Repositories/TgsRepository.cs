using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Tgs.Promotion.OfflineEvent.Deposit.Repositories.Entities;

namespace Tgs.Promotion.OfflineEvent.Deposit.Repositories;

internal class TgsRepository : ITgsRepository
{
	private readonly TgsContext _dbContext;

	public TgsRepository(TgsContext dbContext)
	{
		_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
	}

	public async ValueTask AddOfflineDepositEventAsync(OfflineDepositEvent eventData, CancellationToken cancellationToken = default)
	{
		var row = new OfflineDepositEventRow
		{
			Id = eventData.Id,
			BrandId = eventData.BrandId,
			BonusAmount = eventData.Bonus,
			BucketAmount = eventData.BucketAmount,
			DepositFormId = eventData.DepositFormId,
			PlayerCode = eventData.PlayerCode,
			PromotionId = eventData.PromotionId,
			CreatedOn = eventData.CreatedOn,
			Settings = JsonSerializer.Serialize(eventData.Settings)
		};

		_ = await _dbContext.OfflineDepositEventRows.AddAsync(
			row,
			cancellationToken).ConfigureAwait(false);

		_ = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask<bool> AchievePromotionAsync(
		Guid promotionId,
		string playerCode,
		CancellationToken cancellationToken = default)
		=> await _dbContext.OfflineDepositEventRows
			.Where(row => row.PromotionId == promotionId
				&& row.PlayerCode == playerCode)
			.AnyAsync(cancellationToken: cancellationToken)
			.ConfigureAwait(false);
}
