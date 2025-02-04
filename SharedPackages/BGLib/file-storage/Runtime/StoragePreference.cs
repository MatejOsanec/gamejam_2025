/// <summary>
/// Storage Locations! These work different on various platforms, and choice is not guaranteed.<br />
/// <br/>
/// <b>Quest</b><br/>
/// Uses <a href="https://developer.oculus.com/documentation/unity/ps-cloud-backup/">Cloud Backups</a> for PreferCloudSynced.
/// These are not meant as a sync, but rather as a backup, in case you have uninstalled the app.
/// They can sync between different device categories, but only for the first time.
/// <br/><br/>
/// <b>Steam</b><br/>
/// ?
/// <br/><br/>
/// <b>Rift</b><br/>
/// ?
/// <br/><br/>
/// <b>Playstation</b><br/>
/// ?
/// </summary>
public enum StoragePreference {
    Cloud,
    Local
}
