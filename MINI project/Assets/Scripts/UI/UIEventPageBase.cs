using UnityEngine;

/// <summary>
/// 런 이벤트(GameSystemManager.Events)를 구독하는 UI 패널 공통 베이스.
/// OnEnable 에서 구독 + 1회 갱신, OnDisable 에서 구독 해제하는
/// 표준 골격만 제공한다. 어떤 이벤트를 구독하는지는 파생 클래스가 결정한다.
///
/// 직렬화 영향 없음(직렬화 필드 0). 파생 MonoBehaviour 의 스크립트 GUID·
/// [SerializeField] 와이어링은 불변이다.
/// </summary>
public abstract class UIEventPageBase : MonoBehaviour
{
    /// <summary>이벤트 구독(멱등: -= 후 += 패턴 권장).</summary>
    protected abstract void SubscribeEvents();

    /// <summary>이벤트 구독 해제.</summary>
    protected abstract void UnsubscribeEvents();

    /// <summary>패널 내용 갱신. 활성화 시 1회 호출된다.</summary>
    public abstract void Refresh();

    protected virtual void OnEnable()
    {
        SubscribeEvents();
        Refresh();
    }

    protected virtual void OnDisable()
    {
        UnsubscribeEvents();
    }
}
