using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 통일된 로그 유틸. 카테고리/태그를 붙여서 필터 가능.
    /// </summary>
    public static class DebugLoggert
    {
        /// <summary>일반 로그.</summary>
        public static void Log(string tag, string msg)
        {
            // 동작 요약: Debug.Log($"[{tag}] {msg}").
            //TODO: Debug.Log($"[{tag}] {msg}");
        }

        /// <summary>경고.</summary>
        public static void Warn(string tag, string msg)
        {
            // 동작 요약: Debug.LogWarning($"[{tag}] {msg}").
            //TODO: Debug.LogWarning($"[{tag}] {msg}");
        }

        /// <summary>에러.</summary>
        public static void Error(string tag, string msg)
        {
            // 동작 요약: Debug.LogError($"[{tag}] {msg}").
            //TODO: Debug.LogError($"[{tag}] {msg}");
        }
    }
}
