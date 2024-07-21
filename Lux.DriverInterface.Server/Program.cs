using Lux.DataRadio;
using Microsoft.AspNetCore.ResponseCompression;
using Lux.DriverInterface.Shared;
using System.Device.Gpio;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
	Args = args,
	ContentRootPath = Environment.OSVersion.Platform == PlatformID.Unix ? "/app" : Directory.GetCurrentDirectory(),
	WebRootPath = Environment.OSVersion.Platform == PlatformID.Unix ? "/app/wwwroot" : "wwwroot"
});
builder.WebHost.UseUrls("http://*:61248");
builder.WebHost.UseSetting("http_port", "61248");
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<WaveSculptor>();
builder.Services.AddSingleton<Telemetry>();
builder.Services.AddSingleton<Encoder>();
builder.Services.AddSingleton<Battery>();
builder.Services.AddSingleton<MpptCollection>();
builder.Services.AddSingleton<PeripheralCollection>();
builder.Services.AddSingleton<SteeringWheel>();
builder.Services.AddSingleton<Distribution>();
builder.Services.AddSingleton<CanDecoder>();

//builder.Services.AddHostedService<TestingDataIncrementService>();

builder.Services.AddSingleton<IPacketQueue, PacketQueue>();
builder.Services.AddSingleton<RadioService>();

if (Environment.OSVersion.Platform == PlatformID.Unix)
{
	builder.Services.AddSingleton<ICanServiceBase, UnixCanServiceBase>();
}
else
{
	builder.Services.AddHostedService<PacketGeneratorService>();
	builder.Services.AddSingleton<ICanServiceBase, DummyCanServiceBase>();
}

builder.Services.AddSingleton<CanSendService>();
builder.Services.AddHostedService<CanReceiveService>();

builder.Services.AddSingleton<GpioWrapper>();
builder.Services.AddHostedService<PedalService>();
builder.Services.AddHostedService<CanAutoSender>();

builder.Services.AddBlazorBootstrap(); // Add this line
builder.Services.AddRazorPages();

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.NumberHandling |= System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals);
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

await app.RunAsync();
