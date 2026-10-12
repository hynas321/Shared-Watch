using WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSharedWatchServices(builder.Configuration, builder.Environment);

var app = builder.Build();
app.UseSharedWatchPipeline();
app.Run();
public partial class Program { }


