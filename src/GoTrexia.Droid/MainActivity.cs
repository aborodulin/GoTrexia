using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Core.View;

namespace GoTrexia.Droid
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ApplyFullscreen();
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);

            if (hasFocus)
            {
                ApplyFullscreen();
            }
        }

        private sealed class ConsumeInsetsListener : Java.Lang.Object, AndroidX.Core.View.IOnApplyWindowInsetsListener
        {
            public WindowInsetsCompat OnApplyWindowInsets(Android.Views.View v, WindowInsetsCompat insets)
            {
                v.SetPadding(0, 0, 0, 0);
                return WindowInsetsCompat.Consumed;
            }
        }

        private void ApplyFullscreen()
        {
            if (Window is null)
            {
                return;
            }

            Window.SetFlags(WindowManagerFlags.LayoutNoLimits, WindowManagerFlags.LayoutNoLimits);
            Window.AddFlags(WindowManagerFlags.Fullscreen);
            WindowCompat.SetDecorFitsSystemWindows(Window, false);

            if (OperatingSystem.IsAndroidVersionAtLeast(28))
            {
                var attributes = Window.Attributes;
                attributes.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;
                Window.Attributes = attributes;
            }

            var controller = new WindowInsetsControllerCompat(Window, Window.DecorView);
            controller.Hide(WindowInsetsCompat.Type.StatusBars() | WindowInsetsCompat.Type.NavigationBars());
            controller.SystemBarsBehavior = WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe;

            var content = Window.DecorView.FindViewById(Android.Resource.Id.Content);
            if (content is not null)
            {
                ViewCompat.SetOnApplyWindowInsetsListener(content, new ConsumeInsetsListener());
                ViewCompat.RequestApplyInsets(content);
            }
        }
    }
}
