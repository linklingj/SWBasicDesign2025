using UnityEngine;
using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;

public class RewardCameraConroller : MonoBehaviour
{
    [Serializable]
    public class StateCameraSetting {
        [SerializeReference]
        public State<UpgradeManager> state;
        public CameraSetting setting = new CameraSetting();
    }

    // 캐시된 Camera 컴포넌트 참조 (FOV 트윈용)
    private Camera cam;

    // 상태별 카메라 세팅 목록 (인스펙터에서 상태→세팅 등록)
    [TableList]
    [SerializeField]
    public List<StateCameraSetting> stateCameraSettings = new List<StateCameraSetting>();

    private State<UpgradeManager> curState;

    private void Awake() {
        cam = GetComponent<Camera>();
    }

    // 외부에서 상태 전환을 요청할 때 진입점
    public void SetCameraByState<T>(float duration, Action onFinish = null) where T : State<UpgradeManager>
    {
        if (stateCameraSettings == null) return;
        var setting = stateCameraSettings.Find(x=> x.state is T);
        if (setting != null) {
            ApplySetting(setting.setting, duration, onFinish);
        }
    }
    
    // 일반 상태 전환: 위치/회전/FOV를 주어진 시간 동안 트윈
    public void ApplySetting(CameraSetting setting, float duration, Action onFinish = null) {
        if (cam != null) {
            cam.DOOrthoSize(setting.projectionSize, duration).SetEase(Ease.OutSine);
        }
        transform.DOMove(setting.position, duration).SetEase(Ease.OutSine);
        transform.DORotate(setting.rotation, duration).SetEase(Ease.InSine)
            .OnComplete(() => onFinish?.Invoke());
    }
}

