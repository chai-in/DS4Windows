using System.Runtime.CompilerServices;
using DS4Windows;
using DS4WinWPF.DS4Control;
using DS4WinWPF.DS4Forms.ViewModels;

namespace DS4WindowsTests;

[TestClass]
[DoNotParallelize]
public sealed class WiredDualShock4PresetTests
{
    [TestMethod]
    public void PresetPickerOffersNativeDS4WithoutAnXboxOutputChoice()
    {
        var picker = new PresetOptionViewModel();
        picker.PresetIndex = picker.PresetsList.FindIndex(preset => preset is WiredDualShock4Preset);

        Assert.IsTrue(picker.PresetIndex >= 0);
        Assert.IsFalse(picker.PresetDisplayOutputCont);
        Assert.AreEqual(PresetOption.OutputContChoice.DualShock4,
            picker.PresetsList[picker.PresetIndex].OutputCont);
        Assert.IsFalse(string.IsNullOrWhiteSpace(picker.PresetDescription));
        Assert.IsInstanceOfType(picker.PresetsList[0], typeof(GamepadPreset));
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void SavedNativeProfileRetainsTouchMotionAndUnmodifiedAxes(bool legacy)
    {
        string directory = Directory.CreateTempSubdirectory("ds4w-native-preset-").FullName;
        string previousRoot = Global.appdatapath;
        int editorSlot = Global.MAX_DS4_CONTROLLER_COUNT;
        try
        {
            Global.appdatapath = directory;
            Directory.CreateDirectory(Path.Combine(directory, "Profiles"));
            var store = new BackingStore();
            store.LoadBlankDS4Profile(editorSlot, false, null, xinputChange: false,
                postLoad: false, nativePassthrough: true);
            Assert.IsTrue(store.SaveProfileNew(editorSlot, "Native DS4"));

            var reloaded = new BackingStore();
            var service = (ControlService)RuntimeHelpers.GetUninitializedObject(typeof(ControlService));
            string profilePath = Path.Combine(directory, "Profiles", "Native DS4.xml");
            Assert.IsTrue(legacy
                ? reloaded.LoadProfile(editorSlot, false, service, profilePath,
                    xinputChange: false, postLoad: false)
                : reloaded.LoadProfileNew(editorSlot, false, service, profilePath,
                    xinputChange: false, postLoad: false));

            Assert.AreEqual(OutContType.ViiperDS4, reloaded.outputDevType[editorSlot]);
            Assert.AreEqual(TouchpadOutMode.Passthru, reloaded.touchOutMode[editorSlot]);
            Assert.IsTrue(reloaded.touchClickPassthru[editorSlot]);
            Assert.AreEqual(GyroOutMode.Passthru, reloaded.gyroOutMode[editorSlot]);
            Assert.AreEqual(0, reloaded.lsModInfo[editorSlot].deadZone);
            Assert.AreEqual(0, reloaded.rsModInfo[editorSlot].deadZone);
            Assert.AreEqual(0, reloaded.lsModInfo[editorSlot].antiDeadZone);
            Assert.AreEqual(0, reloaded.rsModInfo[editorSlot].antiDeadZone);
            Assert.AreEqual((byte)0, reloaded.l2ModInfo[editorSlot].deadZone);
            Assert.AreEqual((byte)0, reloaded.r2ModInfo[editorSlot].deadZone);
        }
        finally
        {
            Global.appdatapath = previousRoot;
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void StandardDS4PresetStillResetsToItsExistingMouseBehavior()
    {
        int editorSlot = Global.MAX_DS4_CONTROLLER_COUNT;
        var store = new BackingStore();
        store.LoadBlankDS4Profile(editorSlot, false, null, xinputChange: false,
            postLoad: false, nativePassthrough: true);

        store.LoadBlankDS4Profile(editorSlot, false, null,
            xinputChange: false, postLoad: false);

        Assert.AreEqual(OutContType.ViiperDS4, store.outputDevType[editorSlot]);
        Assert.AreEqual(TouchpadOutMode.Mouse, store.touchOutMode[editorSlot]);
        Assert.IsFalse(store.touchClickPassthru[editorSlot]);
        Assert.AreEqual(GyroOutMode.Controls, store.gyroOutMode[editorSlot]);
    }
}
