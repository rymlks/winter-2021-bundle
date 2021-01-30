using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MutesTheMusic : MonoBehaviour
{
    [SerializeField] private bool muteIsActive = false;
    private GameObject AudioSourceObj;
    private AudioSource AudioSource;
    public void Awake() {
        AudioSourceObj = GameObject.FindGameObjectWithTag("Music");
        if (AudioSourceObj.GetComponent<AudioSource>()) {
            AudioSource = AudioSourceObj.GetComponent<AudioSource>();
        }
    }
    public void ToggleMuteBoolean() {
        muteIsActive = !muteIsActive;
        AudioSource.mute = muteIsActive;
    }
}
