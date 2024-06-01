using Lux.DataRadio;
using Microsoft.AspNetCore.ResponseCompression;
using Lux.DriverInterface.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://*:61248");
builder.WebHost.UseSetting("http_port", "61248");
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<Header>();
builder.Services.AddSingleton<EMU>();
builder.Services.AddSingleton<WaveSculptor>();
builder.Services.AddSingleton<Telemetry>();
builder.Services.AddSingleton<Peripheral>(); //Not needed after the steering wheel gets implemented
builder.Services.AddSingleton<Encoder>(); // Only necessary for testing
builder.Services.AddHostedService<TestingDataIncrementService>();
//builder.Services.AddHostedService<CANSendService>();


builder.Services.AddHostedService<EncoderService>();

builder.Services.AddBlazorBootstrap(); // Add this line
builder.Services.AddRazorPages();

builder.Services.AddControllers();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
