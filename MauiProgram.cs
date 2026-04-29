using Microsoft.AspNetCore.Components.WebView.Maui;
using EtiquetadoAuto.Data;

namespace EtiquetadoAuto;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});
		builder.Services.AddSingleton<EtiquetadoAuto.Services.StockService>();
		builder.Services.AddSingleton<EtiquetadoAuto.Services.PrinterService>();
		builder.Services.AddSingleton<EtiquetadoAuto.Services.IAService>();
		builder.Services.AddMauiBlazorWebView();
#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
#endif



		return builder.Build();
	}
}
