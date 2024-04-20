using Lux.DriverInterface.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorBootstrap;
using Lux.DriverInterface.Shared;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazorBootstrap();
builder.Services.AddSingleton<Header>();
builder.Services.AddSingleton<TimerService>();
builder.Services.AddSingleton<EMU>();
builder.Services.AddSingleton<WaveSculptor>();
builder.Services.AddSingleton<Telemetry>();
builder.Services.AddSingleton<Encoder>();
builder.Services.AddSingleton<Peripheral>(); //Not needed after the steering wheel gets implemented
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
