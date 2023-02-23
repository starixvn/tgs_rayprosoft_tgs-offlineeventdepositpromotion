using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Tgs.Promotion.OfflineEvent.Deposit.Repositories.Entities;

namespace Tgs.Promotion.OfflineEvent.Deposit.Repositories.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddOfflineDepsoitPromotionRepositories(
		this IServiceCollection services,
		Action<IServiceProvider, DbContextOptionsBuilder> dbConfigure)
		=> services
			.AddTransient<ITgsRepository, TgsRepository>()
			.AddDbContext<TgsContext>(dbConfigure);

	public static IServiceCollection DecorateOfflineDepsoitPromotionForRedis(
		this IServiceCollection services,
		Func<IServiceProvider, IDatabase> redisDatabaseFactory)
		=> services
			.Decorate<IStatisticsService>((source, sp) => new RedisCacheStatisticsService(
				source,
				redisDatabaseFactory(sp)));
}
