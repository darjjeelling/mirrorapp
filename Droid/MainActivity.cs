using Android.App;
using Android.OS;

namespace mirror.Droid
{
	[Activity(Label = "@string/app_name",
	          MainLauncher = true,
	          Icon = "@mipmap/ic_launcher",
	          ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)
	]
	public class CameraActivity : Activity
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			ActionBar.Hide();
			SetContentView(Resource.Layout.activity_camera);

			if (bundle == null)
			{
				FragmentManager.BeginTransaction().Replace(Resource.Id.container, MirrorFragment.NewInstance()).Commit();
			}
		}
	}
}
