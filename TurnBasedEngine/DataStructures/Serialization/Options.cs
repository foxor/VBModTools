using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[TypeIndex(91)]
public class Options : Model {
    public SBool MusicEnabled { get => Get<SBool>(1); set => Set(1, value); }
    public SBool SoundsEnabled { get => Get<SBool>(2); set => Set(2, value); }
    public SString SeedText { get => Get<SString>(3); set => Set(3, value); }
    public SFloat VolumeSlider { get => Get<SFloat>(4); set => Set(4, value); }
    public SBool Windowed { get => Get<SBool>(5); set => Set(5, value); }

    public Options() {
        MusicEnabled = true;
        SoundsEnabled = true;
        SeedText = "";
        VolumeSlider = .5f;
        Windowed = true;
    }
    public void Copy(Options other) {
        // The reason we need to do this is because this has a model ID which is explicitly not copied and is out of date.
        if (SNull.IsNull(other)) {
            return;
        }
        MusicEnabled = other.MusicEnabled;
        SoundsEnabled = other.SoundsEnabled;
        SeedText = other.SeedText;
        VolumeSlider = other.VolumeSlider;
        Windowed = other.Windowed;
    }
}