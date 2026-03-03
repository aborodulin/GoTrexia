namespace GoTrexia.Droid
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                //.UseMauiMaps()
                .UseSharedMauiApp();
                

            return builder.Build();
        }
    }
}
