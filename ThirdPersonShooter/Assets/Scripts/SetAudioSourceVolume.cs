using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAudioSourceVolume : MonoBehaviour
{
    public bool useMasterVolume = true;
    public bool useMusicVolume = true;
    private float lastVolumeCheckTime = 0f;
    private AudioSource aS;
    void Start()
    {
        aS = GetComponent<AudioSource>();
        aS.volume = 1 * (useMasterVolume ? SettingsManager.instance.settingsFile.masterVolume : 1f) * (useMusicVolume ? SettingsManager.instance.settingsFile.musicVolume : 1f);
    }
    private void FixedUpdate()
    {
        lastVolumeCheckTime += Time.fixedDeltaTime;
        if (lastVolumeCheckTime > 1f)
        {
            lastVolumeCheckTime = 0f;
            aS.volume = 1 * (useMasterVolume?SettingsManager.instance.settingsFile.masterVolume:1f) * (useMusicVolume?SettingsManager.instance.settingsFile.musicVolume:1f);
        }
    }
}
