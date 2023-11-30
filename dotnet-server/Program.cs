using Dotnet.Server.Hubs;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton(sp =>
{
    var youtubeAPIServiceInitializer = new BaseClientService.Initializer
    {
        ApiKey = configuration["YouTubeApi:ApiKey"],
        ApplicationName = "ReactApplication"
    };
    return new YouTubeAPIService(youtubeAPIServiceInitializer);
});
builder.Services.AddScoped<PlaylistManager>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => 
{
    options.AddPolicy("AllowReactApplication",
        builder => builder.WithOrigins("*")
            .AllowAnyHeader()
            .AllowAnyMethod()   
    );
}); 

builder.Services.AddControllers()
    .AddJsonOptions(options => 
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = new PascalCaseNamingPolicy();
    });
builder.Services.AddSignalR(); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHub<RoomHub>("/Hub/Room");
app.UseCors("AllowReactApplication");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
