using Shipeng.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host
     .ConfigureLogging((context, logBuilder) =>
     {
         Log.Logger = new LoggerConfiguration()
          .Enrich.FromLogContext()
          .WriteTo.Console()// 日志输出到控制台
          .MinimumLevel.Information()
          .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
          .WriteTo.Async(c => c.File("Logs/logs.log", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:HH:mm} ||{Level} || {SourceContext:l} || {Message} || {Exception} |end {NewLine}"))
          .WriteTo.Async(c => c.Console())
          .CreateLogger();
         logBuilder.ClearProviders();
         logBuilder.AddSerilog(dispose: true);
     })
     .UseAutofac();


//配置请求体大小
builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Limits.MaxRequestBodySize = int.MaxValue;
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = int.MaxValue;
});
builder.Services.ReplaceConfiguration(builder.Configuration);//修正配置错误

Startup startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);
WebApplication app = builder.Build();
startup.Configure(app, app.Environment);
app.Run();

//builder.Services.AddApplication<HostingModule>();
//var app = builder.Build();
//app.InitializeApplication();
//app.MapGet("/", context => context.Response.WriteAsync("hello world!!!"));
//app.Run();
