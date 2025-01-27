using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using JsonToCsharpPoco.Components;
using JsonToCsharpPoco.Converter;
using Blazored.LocalStorage;
using JsonToCsharpPoco.Shared;
using JsonToCsharpPoco.Components.Toast;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddLocalization();
builder.Services.AddSingleton<CSharpPocoBuilder>();
builder.Services.AddSingleton<JsonToCSharp>();
builder.Services.AddSingleton<ToastService>();
builder.Services.AddBlazoredLocalStorageAsSingleton();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var host = builder.Build();

//Get pereferred culture from local storage
var localStorageService = host.Services.GetRequiredService<ILocalStorageService>();
var preferredCulture = await localStorageService.GetItemAsStringAsync(Constants.PreferredCulture);
var cultureInfo = new System.Globalization.CultureInfo(preferredCulture ?? "en");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

await host.RunAsync();
