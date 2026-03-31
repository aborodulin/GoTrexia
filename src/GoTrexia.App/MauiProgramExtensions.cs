using GoTrexia.Application;
using GoTrexia.Core.Engine;
using Microsoft.Extensions.Logging;

namespace GoTrexia
{
    public static class MauiProgramExtensions
    {
        public static MauiAppBuilder UseSharedMauiApp(this MauiAppBuilder builder)
        {
            builder
                .UseMauiApp<App>()
                .UseMauiMaps()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<GameSession>();
            builder.Services.AddSingleton<DistanceCalculator>();
            builder.Services.AddSingleton<StageCompletionEngine>();
            builder.Services.AddSingleton<CompletedSoundPlayer>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder;
        }
    }
}
