using System.Diagnostics;
using Object = UnityEngine.Object;

/// <summary>
/// 로그 출력 래퍼. 출시 빌드 노이즈 제거용.
/// Log / LogWarning 은 에디터·개발빌드에서만 컴파일된다([Conditional] 가
/// 호출 자체를 스트립하므로 인자 평가 비용도 릴리스에서 사라진다).
/// LogError 는 항상 컴파일된다(오류는 출시에서도 노출돼야 함).
/// GameDevelopSetting.md:130 의 주요 체크포인트 로그 요구를 보존하면서
/// 릴리스 노이즈만 제거한다.
/// </summary>
public static class GameLog
{
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Log(object message) => UnityEngine.Debug.Log(message);

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Log(object message, Object context) => UnityEngine.Debug.Log(message, context);

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void LogWarning(object message) => UnityEngine.Debug.LogWarning(message);

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void LogWarning(object message, Object context) => UnityEngine.Debug.LogWarning(message, context);

    public static void LogError(object message) => UnityEngine.Debug.LogError(message);

    public static void LogError(object message, Object context) => UnityEngine.Debug.LogError(message, context);
}
