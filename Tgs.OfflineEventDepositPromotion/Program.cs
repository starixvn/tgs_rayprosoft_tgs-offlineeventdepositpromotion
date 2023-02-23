using FluentdForward.OpenTelemetry.MessagePack;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.OfflineEventDepositPromotion;
using Tgs.OfflineEventDepositPromotion.MessageQueue;
using Tgs.Promotion.OfflineEvent.Deposit.Repositories.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging
	.AddOpenTelemetry(options =>
	{
		options.IncludeFormattedMessage = true;
		options.ParseStateValues = true;
		_ = options.AddFluentdForwardExporter(fluentOptions =>
		{
			var nodeIp = builder.Configuration.GetValue<string>("NODE_IP")!;
			fluentOptions.Host = nodeIp;
			fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
			fluentOptions.UseMessagePack(LogRecordFormatterResolver.GetResolverInstanceWithExtendInfo(
				"MachineName",
				() => Environment.MachineName));
		});
	});

builder.Services
	.AddMessageQueue()
	.AddNatsMessageQueue(natsConfiguration => natsConfiguration
		.ConfigQueueOptions((options, sp) =>
		{
			var config = sp.GetRequiredService<IConfiguration>();

			options.Url = config.GetConnectionString("Nats")!;
		})
		.AddHandler<DepositFormStatusChangedHandler>(
			"tgs.payment.deposit.status_ex",
			"tgs.promotion.offline.deposit")
		.AddProcessor<OfflineDepositSettingProcessor>(
			"tgs.promotion.offlinedepositreward.setting",
			"tgs.promotion.offlinedepositreward.setting"))
	.AddNatsGlobPatternExchange("*");

builder.Services
	.AddSingleton(sp => new RedisConnectionFactory(
		builder.Configuration.GetConnectionString("Redis")!))
	.AddPromotionClientServices((options, _) =>
	{
		options.Url = builder.Configuration.GetValue<Uri>("OfflineDeposit:PromotionServiceUrl")!;
	})
	.AddPaymentServiceClient(httpBuilder => httpBuilder
		.ConfigureHttpClient(httpClient =>
		{
			httpClient.BaseAddress = builder.Configuration.GetValue<Uri>("OfflineDeposit:PaymentServiceUrl");
		}))
	.AddOfflineDepsoitPromotion()
	.AddOfflineDepsoitPromotionRepositories((_, dbBuilder) => dbBuilder
		.UseNpgsql("name=Tgs"))
	.DecorateOfflineDepsoitPromotionForRedis(sp => sp.GetRequiredService<RedisConnectionFactory>().Database);

builder.Services
	.AddOpenTelemetry()
	.ConfigureResource(resourceBuilder => resourceBuilder
		.AddEnvironmentVariableDetector())
	.WithTracing(tracerBuilder => tracerBuilder
		.AddSource("Tgs.*")
		.AddSource("TGS.*")
		.AddAspNetCoreInstrumentation()
		.AddHttpClientInstrumentation()
		.AddNpgsql()
		.AddProfileViewExporter()
		.AddOtlpExporter())
	.WithMetrics(meterBuilder => meterBuilder
		.AddRuntimeInstrumentation()
		.AddAspNetCoreInstrumentation()
		.AddHttpClientInstrumentation()
		.AddOtlpExporter())
	.Services
	.Configure<AspNetCoreInstrumentationOptions>(options => options.Filter =
		httpContext => !httpContext.Request.Path.HasValue
			|| !httpContext.Request.Path.StartsWithSegments("/swagger")
			&& !httpContext.Request.Path.StartsWithSegments("/healthz")
			&& !httpContext.Request.Path.StartsWithSegments("/metrics")
			&& !new[] { "html", "ico", "png", "jpg" }.Any(
				ext => httpContext.Request.Path.Value.EndsWith($".{ext}")));

builder.Services
	.AddHealthChecks();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapHealthChecks("/healthz");
app.MapProfileViewer();

app.Run();
