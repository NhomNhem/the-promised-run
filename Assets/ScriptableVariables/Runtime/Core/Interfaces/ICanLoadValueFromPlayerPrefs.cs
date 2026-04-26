namespace OpenUtility.Data
{
    /// <summary>
    /// Implement this interface on your custom scriptable object if it loads its value from
    /// player preferences. This player preference will automatically be deleted if an asset
    /// is deleted from the project.
    /// </summary>
    public interface ICanLoadValueFromPlayerPrefs
    {
        Optional<string> PlayerPref { get; }
    }
}
