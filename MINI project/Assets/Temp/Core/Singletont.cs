using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// MonoBehaviour 싱글톤 베이스. GameSystemManagert 등 최상위 시스템에서만 사용.
    /// </summary>
    /// <typeparam name="T">싱글톤으로 노출할 자기 자신 타입.</typeparam>
    public abstract class Singletont<T> : MonoBehaviour
        where T : Singletont<T>
    {
        /// <summary>
        /// 활성 싱글톤 인스턴스. 씬 종료 중에는 null 반환.
        /// </summary>
        public static T Instance
        {
            get
            {
                // 동작 요약:
                // - 정적 캐시가 살아 있으면 캐시 반환.
                // - 캐시가 없으면 FindFirstObjectByType<T>로 검색.
                // - 검색해도 없고 게임 종료 플래그가 false면 새 GameObject로 자동 생성.
                // - 종료 중이면 null 반환(파괴된 인스턴스 재생성 방지).
                //TODO: if (IsQuitting) return null;
                //TODO: if (cachedInstance != null) return cachedInstance;
                //TODO: cachedInstance = FindFirstObjectByType<T>();
                //TODO: if (cachedInstance == null && !IsQuitting)
                //TODO: {
                //TODO:     var go = new GameObject(typeof(T).Name);
                //TODO:     cachedInstance = go.AddComponent<T>();
                //TODO: }
                //TODO: return cachedInstance;
                return null;
            }
        }

        /// <summary>
        /// 씬 종료 또는 어플리케이션 종료가 진행 중이면 true. 싱글톤 자동 생성 방지용.
        /// </summary>
        protected static bool IsQuitting;

        /// <summary>
        /// 정적 인스턴스 캐시. Awake에서 셋업.
        /// </summary>
        private static T cachedInstance;

        /// <summary>
        /// 첫 등장 시 캐시에 자기 자신을 저장하고, 중복 인스턴스면 자기 자신을 파괴한다.
        /// </summary>
        protected virtual void Awake()
        {
            // 동작 요약:
            // - cachedInstance가 null이면 this를 등록.
            // - cachedInstance가 다른 인스턴스를 가리키면 Destroy(gameObject)로 자기 파괴.
            // - DontDestroyOnLoad가 필요하면 파생 클래스에서 override 후 호출.
            //TODO: if (cachedInstance == null) { cachedInstance = (T)this; DontDestroyOnLoad(gameObject); }
            //TODO: else if (cachedInstance != this) Destroy(gameObject);
        }

        /// <summary>
        /// 정적 캐시를 정리한다. 어플리케이션 종료 시 호출.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            // 동작 요약:
            // - IsQuitting = true 설정.
            // - cachedInstance를 null로 정리.
            //TODO: IsQuitting = true;
            //TODO: cachedInstance = null;
        }

        /// <summary>
        /// 파괴 시 캐시가 자기 자신이면 정리한다.
        /// </summary>
        protected virtual void OnDestroy()
        {
            // 동작 요약:
            // - cachedInstance == this인 경우만 null로 정리.
            // - 다른 인스턴스를 잘못 지우지 않게 동일성 검사.
            //TODO: if (cachedInstance == this) cachedInstance = null;
        }
    }
}
