using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "New AudioData", menuName = "Audio/Audio Data")]
public class AudioData : ScriptableObject {
    [Title("Clip Settings")]
    public AudioClip clip;

    [ShowIf(nameof(clip)), DisableIf(nameof(loop))]
    [MinMaxSlider(0, nameof(GetClipLength), showFields: true)]
    public Vector2 trimming;
    private float GetClipLength() => clip != null ? clip.length : 0f;

    [Title("Playback Settings")]
    public AudioMixerGroup audioMixerGroup;

    [Range(0f, 1f)]
    public float volume = 1.0f;

    [Range(0.5f, 2f)]
    public float pitch = 1.0f;

    [InfoBox("If loop is true, trimming not be applied.", VisibleIf = nameof(loop))]
    public bool loop = false;

    void OnValidate() {
        if (clip == null) {
            trimming = Vector2.zero;
            return;
        }

        if (trimming == Vector2.zero) {
            trimming = new Vector2(0, clip.length);
        }
    }

    public void Play() {
        if (clip == null) return;
        if (AudioPlayer.Instance == null) return;

        AudioPlayer.Instance.Play(this);
    }
}