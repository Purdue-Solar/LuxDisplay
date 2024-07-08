using Lux.DriverInterface.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using BlazorBootstrap;
using Lux.DriverInterface.Shared;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazorBootstrap();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<WaveSculptor>();
builder.Services.AddSingleton<Telemetry>();
builder.Services.AddSingleton<Encoder>();
builder.Services.AddSingleton<MpptCollection>();
builder.Services.AddSingleton<PeripheralCollection>();
builder.Services.AddSingleton<SteeringWheel>();
builder.Services.AddSingleton<Distribution>();
builder.Services.AddSingleton<Blinkers>();
builder.Services.AddSingleton<BackgroundDataService>();

await builder.Build().RunAsync();
