using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

// 관찰자 패턴에 의해 변동이 있을때 유니티 이벤트를 날려주는 클래스
// .Value로 접근
/// <typeparam name="T">The type of the value being observed.</typeparam>
[Serializable]
public class Observable<T> {
    [SerializeField] T value;
    [SerializeField] UnityEvent<T> onValueChanged;

    // 데이터 값
    public T Value {
        get => value;
        set => Set(value);
    }
    
    // Observable<T>를 일반 T처럼 사용할 수 있게 해줌
    // 예시)
    // Observable<int> myInt = new Observable<int>(1);
    // int number = myInt;
    public static implicit operator T(Observable<T> observable) => observable.value;
    
    // 생성자. 초기 값과 콜백 리스너 설정
    public Observable(T value, UnityAction<T> callback = null) {
        this.value = value;
        onValueChanged = new UnityEvent<T>();
        if (callback != null) onValueChanged.AddListener(callback);
    }

    // 값 설정
    public void Set(T value) {
        if (Equals(this.value, value)) return;
        this.value = value;
        Invoke();
    }

    // 이벤트 호출
    public void Invoke() {
        onValueChanged.Invoke(value);
    }

    // 관찰자 추가
    public void AddListener(UnityAction<T> callback) {
        if (callback == null) return;
        if (onValueChanged == null) onValueChanged = new UnityEvent<T>();

        onValueChanged.AddListener(callback);
    }
    
    // 관찰자 삭제
    public void RemoveListener(UnityAction<T> callback) {
        if (callback == null) return;
        if (onValueChanged == null) return;
        
        onValueChanged.RemoveListener(callback);
    }
    
    // 관찰자 전체 삭제
    public void RemoveAllListeners()
    {
        if (onValueChanged == null) return;
        onValueChanged.RemoveAllListeners();
    }
    
    public void Dispose() {
        RemoveAllListeners();
        onValueChanged = null;
        value = default;
    }
}