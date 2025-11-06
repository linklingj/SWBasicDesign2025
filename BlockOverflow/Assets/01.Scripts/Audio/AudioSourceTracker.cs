using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceTracker : PoolBehaviour<AudioSourceTracker> {
    private AudioSource audioSource;
    private float _duration = 0f;

    private float _baseVolume = 1f;
    private CancellationTokenSource _fadeCts;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    public void Initialize(AudioData audioData) {
        audioSource.clip = audioData.clip;
        audioSource.outputAudioMixerGroup = audioData.audioMixerGroup;

        audioSource.volume = audioData.volume;
        _baseVolume = audioData.volume;
        audioSource.pitch = audioData.pitch;
        audioSource.loop = audioData.loop;

        if (audioData.loop) {
            audioSource.time = 0f;
        } else {
            audioSource.time = audioData.trimming.x;

            _duration = audioData.trimming.y - audioData.trimming.x;
            _duration /= audioData.pitch;
        }
    }

    public void Play() {
        audioSource.Play();

        // 반복하지 않는 경우, 종료 시간 설정 및 Pool 반환
        if (!audioSource.loop) {
            audioSource.SetScheduledEndTime(AudioSettings.dspTime + _duration);

            OnAudioEnd().Forget();
        }
    }

    /// <summary>
    /// Fades the current AudioSource volume to a target value over duration.
    /// Any previous fade is cancelled. Returns when fade completes or is cancelled.
    /// </summary>
    private async UniTask FadeTo(float targetVolume, float duration, bool stopAtEnd = false, bool releaseAtEnd = false)
    {
        _fadeCts?.Cancel();
        _fadeCts?.Dispose();
        _fadeCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        var ct = _fadeCts.Token;

        float start = audioSource.volume;
        float t = 0f;
        // Guard against zero/negative duration
        if (duration <= 0f)
        {
            audioSource.volume = targetVolume;
        }
        else
        {
            while (t < duration)
            {
                ct.ThrowIfCancellationRequested();
                t += Time.unscaledDeltaTime; // unscaled so timescale doesn't affect the fade
                float k = Mathf.Clamp01(t / duration);
                audioSource.volume = Mathf.Lerp(start, targetVolume, k);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            audioSource.volume = targetVolume;
        }

        if (stopAtEnd)
        {
            audioSource.Stop();
        }
        if (releaseAtEnd)
        {
            Release();
        }
    }

    /// <summary>
    /// Fade in from current volume (or 0 if requested) to base volume over duration.
    /// </summary>
    public UniTask FadeIn(float duration, bool fromZero = false, float? toVolume = null)
    {
        if (fromZero) audioSource.volume = 0f;
        float target = toVolume.HasValue ? Mathf.Clamp01(toVolume.Value) : _baseVolume;
        return FadeTo(target, duration, stopAtEnd: false, releaseAtEnd: false);
    }

    /// <summary>
    /// Fade out to zero over duration. Optionally stop and/or release on completion.
    /// </summary>
    public UniTask FadeOut(float duration, bool stopOnComplete = true, bool releaseOnComplete = false)
    {
        return FadeTo(0f, duration, stopAtEnd: stopOnComplete, releaseAtEnd: releaseOnComplete);
    }

    private async UniTask OnAudioEnd() {
        await UniTask.WaitForSeconds(_duration, cancellationToken: this.GetCancellationTokenOnDestroy());

        Release();
    }

    private void OnDisable() {
        _fadeCts?.Cancel();
        _fadeCts?.Dispose();
        _fadeCts = null;

        audioSource.Stop();
        
        audioSource.clip = null;
        audioSource.outputAudioMixerGroup = null;
    }
}