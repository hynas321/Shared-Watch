using DotnetServer.Application.HostedServices;
using DotnetServer.Application.Services;
using DotnetServer.Application.Services.Interfaces;
using DotnetServer.Api.Handlers;
using DotnetServer.Infrastructure.Repositories;
using DotnetServer.Shared.Helpers;
using DotnetServer.SignalR;
using Google.Apis.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddSingleton<IYouTubeAPIService, YouTubeAPIService>(sp =>
{
    var youtubeAPIServiceInitializer = new BaseClientService.Initializer
    {
        ApiKey = configuration["YouTubeApi:ApiKey"],
        ApplicationName = "ReactApplication"
    };
    return new YouTubeAPIService(youtubeAPIServiceInitializer);
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IPlaylistRepository, PlaylistRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IRoomControllerHandler, RoomControllerHandler>();
builder.Services.AddScoped<IPlaylistService, PlaylistService>();

builder.Services.AddHostedService<DatabaseCleanup>();

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
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

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
