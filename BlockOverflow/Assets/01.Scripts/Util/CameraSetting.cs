using UnityEngine;
using System;
using Sirenix.OdinInspector;

// 카메라의 위치/회전을 하나의 프리셋으로 묶은 데이터 컨테이너
[Serializable]
public class CameraSetting {
    // 카메라 위치
    public Vector3 position;
    // 카메라 회전(Euler 각)
    public Vector3 rotation;
    // 카메라 시야각
    public float projectionSize = 5f;

    // 에디터에서 버튼으로 현재 메인 카메라에 이 세팅을 즉시 적용
    [Button]
    public void Set()
    {
        Camera.main.transform.position = position;
        Camera.main.transform.rotation = Quaternion.Euler(rotation);
        Camera.main.orthographicSize = projectionSize;
    }
}