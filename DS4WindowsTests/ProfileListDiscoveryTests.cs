using DS4Windows;
using DS4WinWPF;

namespace DS4WindowsTests;

[TestClass]
[DoNotParallelize]
public sealed class ProfileListDiscoveryTests
{
    private string directory;
    private string profilesDirectory;
    private string previousRoot;

    [TestInitialize]
    public void Initialize()
    {
        directory = Directory.CreateTempSubdirectory("ds4w-profile-discovery-").FullName;
        profilesDirectory = Directory.CreateDirectory(Path.Combine(directory, "Profiles")).FullName;
        previousRoot = Global.appdatapath;
        Global.appdatapath = directory;
    }

    [TestCleanup]
    public void Cleanup()
    {
        Global.appdatapath = previousRoot;
        foreach (string path in Directory.GetFiles(profilesDirectory)) File.Delete(path);
        Directory.Delete(profilesDirectory);
        foreach (string path in Directory.GetFiles(directory)) File.Delete(path);
        Directory.Delete(directory);
    }

    [DataTestMethod]
    [DataRow(".xml")]
    [DataRow(".XML")]
    [DataRow(".Xml")]
    public void ImportedProfileRemainsAvailableAfterRefresh(string extension)
    {
        string filename = "Bluetooth DS4" + extension;
        string source = Path.Combine(directory, filename);
        File.WriteAllText(source, "<DS4Windows />");
        File.Copy(source, Path.Combine(profilesDirectory, filename));
        var profiles = new ProfileList();
        profiles.AddProfileSort(Path.GetFileNameWithoutExtension(source));

        profiles.Refresh();

        Assert.AreEqual(1, profiles.ProfileListCol.Count);
        Assert.AreEqual("Bluetooth DS4", profiles.ProfileListCol[0].Name);
        Assert.IsTrue(File.Exists(Path.Combine(profilesDirectory, filename)));
    }

    [TestMethod]
    public void RefreshDoesNotIncludeBackupsOrOtherFileTypes()
    {
        foreach (string filename in new[] { "Game.XML", "Game.xml.bak", "Notes.txt", "Game.XML.backup" })
            File.WriteAllText(Path.Combine(profilesDirectory, filename), "<DS4Windows />");
        var profiles = new ProfileList();

        profiles.Refresh();

        Assert.AreEqual(1, profiles.ProfileListCol.Count);
        Assert.AreEqual("Game", profiles.ProfileListCol[0].Name);
    }
}
