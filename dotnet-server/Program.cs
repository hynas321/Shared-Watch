using Dotnet.Server.Hubs;
using dotnet_server.Infrastructure;
using Google.Apis.Services;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddSingleton<IYouTubeAPIService, YouTubeAPIService>(sp =>
    {
        var youtubeAPIServiceInitializer = new BaseClientService.Initializer
        {
            ApiKey = configuration["YouTubeApi:ApiKey"],
            ApplicationName = "ReactApplication"
        };
        return new YouTubeAPIService(youtubeAPIServiceInitializer);
    }
);

builder.Services.AddSingleton<AppData>();
builder.Services.AddScoped<IPlaylistService, PlaylistService>();
builder.Services.AddSingleton<IRoomRepository, RoomRepository>();
builder.Services.AddSingleton<IChatRepository, ChatRepository>();
builder.Services.AddSingleton<IPlaylistRepository, PlaylistRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options => 
    {
        options.AddPolicy("AllowReactApplication",
            builder => builder.WithOrigins("*")
                .AllowAnyHeader()
                .AllowAnyMethod()   
        );
    }
); 

builder.Services.AddControllers(options =>
{
    //options.Filters.Add<SignalRConnectionIdFilter>();
    //options.Filters.Add<AuthorizationTokenFilter>();
})

.AddJsonOptions(options => 
{
    options.JsonSerializerOptions.PropertyNamingPolicy = new PascalCaseNamingPolicy();
});

builder.Services.AddSignalR(); 

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHub<AppHub>("/Hub/Room");
app.UseCors("AllowReactApplication");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
