using Spect.Net.SpectrumEmu.Abstraction.Providers;
using Spect.Net.SpectrumEmu.Devices.Keyboard;
using Spect.Net.SpectrumEmu.Devices.Tape;
using System;
using System.IO;

namespace Spect.Net.Dap.Providers;

public class DapKeyboardProvider : VmComponentProviderBase, IKeyboardProvider
{
    public void SetKeyStatusHandler(Action<SpectrumKeyCode, bool> statusHandler) { }
    public void Scan(bool allowPhysicalKeyboard) { }
    public bool EmulateKeyStroke() => false;
    public void QueueKeyPress(EmulatedKeyStroke keypress) { }
}

public class DapBeeperProvider : VmComponentProviderBase, IBeeperProvider
{
    public void AddSoundFrame(float[] samples) { }
    public void PlaySound() { }
    public void PauseSound() { }
    public void KillSound() { }
}

public class DapTapeProvider : VmComponentProviderBase, ITapeProvider
{
    public string TapeSetName { get; set; } = string.Empty;
    public BinaryReader GetTapeContent() => null;
    public void CreateTapeFile() { }
    public void SetName(string name) { }
    public void SaveTapeBlock(ITapeDataSerialization block) { }
    public void FinalizeTapeFile() { }
}

public class DapKempstonProvider : VmComponentProviderBase, IKempstonProvider
{
    public bool IsPresent => false;
    public bool LeftPressed => false;
    public bool RightPressed => false;
    public bool UpPressed => false;
    public bool DownPressed => false;
    public bool FirePressed => false;
}
