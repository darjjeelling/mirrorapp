using Android.App;
using Android.OS;
using Android.Preferences;

namespace mirror.Droid
{
	[Activity(Label = "preferences")]
	public class SettingsActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			FragmentManager.BeginTransaction().Replace(Android.Resource.Id.Content, new SettingsFragment()).Commit();
		}

		public class SettingsFragment : PreferenceFragment
		{
			public override void OnCreate(Bundle savedInstanceState)
			{
				base.OnCreate(savedInstanceState);
				AddPreferencesFromResource(Resource.Xml.preferences);
			}
		}
	}
}