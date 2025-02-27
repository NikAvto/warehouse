using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace warehouse
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<Database>(provider =>
            {
                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "storage.db");
                return new Database(dbPath);
            });
            return builder.Build();
        }
    }
}
