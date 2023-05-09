using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[TypeIndex(94)]
public class RunProgress : SObj {
    public SInt ActiveScene { get => Get<SInt>(3); set => Set(3, value); }
    public SInt Seed { get => Get<SInt>(9); set => Set(9, value); }
    public SList<SInt> Rerolls { get => Get<SList<SInt>>(11); set => Set(11, value); }
}

[ProtectStatics]
[TypeIndex(95)]
public class SaveState : SObj {
    public static SaveState Singleton { get; protected set; }
    public RunProgress CurrentRun { get => Get<RunProgress>(0); set => Set(0, value); }
    public Options Options { get => Get<Options>(1); set => Set(1, value); }
    public string CurrentSave => Convert.ToBase64String(this.ToStream().data);
    protected static string Path = Application.persistentDataPath + "/save" + FileSerialization.Extension;
    public void Save() {
        FileSerialization.Write(this, Path);
    }
    public static void Initialize() {
        if (!FileSerialization.Exists(Path)) {
            Singleton = new SaveState();
        }
        else {
            Singleton = FileSerialization.Read<SaveState>(Path);
        }
        if (SNull.IsNull(Singleton.Options)) {
            // Options first, since creating a new run depends on RNG, which is seeded by options.
            Singleton.Options = new Options();
        }
        if (SNull.IsNull(Singleton.CurrentRun)) {
            Singleton.CurrentRun = new RunProgress();
        }
    }
    public void LoadRun() {
        SceneController.LoadScene(Stream.AvailableAssemblyTypes[(ushort)CurrentRun.ActiveScene.Value]);
    }
    public void ClearSave() {
        CurrentRun = new RunProgress();
        Save();
    }
    public bool CanLoad {
        get {
            return !SNull.IsNull(Singleton.CurrentRun);
        }
    }
}
