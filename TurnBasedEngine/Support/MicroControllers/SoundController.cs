using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ProtectStatics]
public class SoundController : MonoBehaviour {
    public static SoundController instance;

    public AudioSource MusicSource;
    public AudioSource SoundSource;

    public void Start() {
        instance = this;
        SetSoundEnabled(SaveState.Singleton.Options.SoundsEnabled);
        SetMusicEnabled(SaveState.Singleton.Options.MusicEnabled);
        SetVolume(SaveState.Singleton.Options.VolumeSlider);
    }

    public void SetVolume(SFloat volume) {
        if (SNull.IsNull(volume)) {
            return;
        }
        // Assuming user hardware varies between 30dB and 90dB relative, exponent is ln(1001)
        // To get 0 to line up without introducing linearity, I changed the 0 point: (e^(ln(1001)*v)-1)/1000
        // https://www.dr-lex.be/info-stuff/volumecontrols.html#ideal
        float lnThousand = 6.90875477932f; // = ln(1001)
        float vOut = (Mathf.Exp(lnThousand * volume) - 1f) / 1000f;
        MusicSource.volume = vOut;
        SoundSource.volume = vOut;
    }
    public void SetSoundEnabled(SBool enabled) {
        if (SNull.IsNull(enabled)) {
            return;
        }
        SoundSource.enabled = enabled;
    }

    public void SetMusicEnabled(SBool enabled) {
        if (SNull.IsNull(enabled)) {
            return;
        }
        MusicSource.enabled = enabled;
    }
}
