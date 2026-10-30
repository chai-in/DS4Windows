using DS4Windows;
using DS4WinWPF;
using DS4WinWPF.DS4Forms.ViewModels.SpecialActions;

namespace DS4WindowsTests;

[TestClass]
public sealed class ProfileListImportTests
{
    [DataTestMethod]
    [DataRow("Bluetooth DS4")]
    [DataRow("bluetooth ds4")]
    [DataRow("BLUETOOTH DS4")]
    public void ReimportPreservesOneProfileAndItsExistingSelection(string importedName)
    {
        var profiles = new ProfileList();
        profiles.AddProfileSort("Zebra");
        profiles.AddProfileSort("Bluetooth DS4");
        profiles.AddProfileSort("Alpha");
        ProfileEntity selected = profiles.ProfileListCol[1];
        int collectionChanges = 0;
        profiles.ProfileListCol.CollectionChanged += (_, _) => collectionChanges++;

        profiles.AddProfileSort(importedName);

        CollectionAssert.AreEqual(new[] { "Alpha", "Bluetooth DS4", "Zebra" },
            profiles.ProfileListCol.Select(profile => profile.Name).ToArray());
        Assert.AreSame(selected, profiles.ProfileListCol[1]);
        Assert.AreEqual(0, collectionChanges);

        var actionEditor = new LoadProfileViewModel(profiles);
        actionEditor.LoadAction(new SpecialAction("Switch to DS4", "PS", "Profile", selected.Name));
        Assert.AreEqual(2, actionEditor.ProfileIndex);
        Assert.IsTrue(actionEditor.UnloadEnabled);
    }

    [TestMethod]
    public void RepeatedImportStillAllowsTheProfileToBeRemovedOnce()
    {
        var profiles = new ProfileList();
        profiles.AddProfileSort("Bluetooth DS4");
        profiles.AddProfileSort("Bluetooth DS4");

        profiles.RemoveProfile("Bluetooth DS4");

        Assert.AreEqual(0, profiles.ProfileListCol.Count);
    }
}
