using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace GMTK2024
{
	[Activity(Label = "GMTK2024"
		, MainLauncher = true
		, Icon = "@drawable/icon"
		, Theme = "@style/Theme.Splash"
		, AlwaysRetainTaskState = true
		, LaunchMode = LaunchMode.SingleInstance
		, ScreenOrientation = ScreenOrientation.FullSensor
		, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout | ConfigChanges.UiMode | ConfigChanges.SmallestScreenSize)]
	public class GMTK2024Activity : Microsoft.Xna.Framework.AndroidGameActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			var game = new GMTK2024Game();
			SetContentView((View) game.Services.GetService(typeof(View)));
			game.Run();
		}
	}
}

