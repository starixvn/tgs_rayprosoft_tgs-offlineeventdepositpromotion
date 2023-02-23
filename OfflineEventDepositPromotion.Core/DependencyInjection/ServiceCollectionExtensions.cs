using System.Reflection;
using Tgs.Promotion.OfflineEvent.Deposit;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddOfflineDepsoitPromotion(this IServiceCollection services)
		=> services
			.RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
			.AddHostedService<DepositEventBackground>()
			.AddSingleton<EventBus>()
			.AddTransient<IEventSender>(sp => sp.GetRequiredService<EventBus>())
			.AddTransient<IStatisticsService, StatisticsService>();
}
