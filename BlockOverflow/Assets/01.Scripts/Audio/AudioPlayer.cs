using UnityEngine;
using UnityEngine.Pool;
using System;
using System.Collections.Generic;
using System.Linq;

public class AudioPlayer : Singleton<AudioPlayer> {
    private IObjectPool<AudioSourceTracker> _audioTrackerPool;

    [SerializeField]
    private AudioSourceTracker _trackerPrefab;

    [Serializable]
    public class AudioDatas {
        public string name;
        public AudioData audioData;
    }
    [SerializeField]
    private AudioDatas[] _audioDatas;
    private Dictionary<string, AudioDatas> _audioDatasDict;
    
    private AudioSourceTracker bgmTracker;

    protected override void Awake() {
        base.Awake();
        if (this == null) return;

        _audioTrackerPool = new ObjectPool<AudioSourceTracker>(
            createFunc: () => {
                var tracker = Instantiate(_trackerPrefab, transform);
                tracker.SetPool(_audioTrackerPool);

                return tracker;
            },
            actionOnGet: tracker => tracker.gameObject.SetActive(true),
            actionOnRelease: tracker => tracker.gameObject.SetActive(false),
            collectionCheck: true
        );
        
        ToDict();
    }
    
    void ToDict()
    {
        _audioDatasDict = new Dictionary<string, AudioDatas>();
        foreach (var data in _audioDatas)
        {
            if (!_audioDatasDict.ContainsKey(data.name))
                _audioDatasDict.Add(data.name, data);
        }
    }

    public void Play(AudioData audioData) 
    {
        if (_trackerPrefab == null) return;
        var tracker = _audioTrackerPool.Get();

        tracker.Initialize(audioData);
        tracker.Play();
    }
    
    public void Play(string audioKey)
    {
        var audioData = _audioDatasDict.GetValueOrDefault(audioKey)?.audioData;
        if (audioData == null) return;

        Play(audioData);
    }

    public void PlayBGM(AudioData audioData)
    {
        if (_trackerPrefab == null) return;
        if (bgmTracker == null) {
            bgmTracker = _audioTrackerPool.Get();
        } else {
            bgmTracker.Release();
            bgmTracker = _audioTrackerPool.Get();
        }

        bgmTracker.Initialize(audioData);
        bgmTracker.Play();
    }
    
    public void FadeInBGM(float duration)
    {
        if (bgmTracker == null) return;
        bgmTracker.FadeIn(duration, true);
    }
    
    public void FadeOutBGM(float duration)
    {
        if (bgmTracker == null) return;
        bgmTracker.FadeOut(duration, false);
    }
}