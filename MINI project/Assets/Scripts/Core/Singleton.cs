using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// MonoBehaviour 싱글톤 베이스. GameSystemManager 등 최상위 시스템에서만 사용.
    /// </summary>
    /// <typeparam name="T">싱글톤으로 노출할 자기 자신 타입.</typeparam>
    public abstract class Singleton<T> : MonoBehaviour
        where T : Singleton<T>
    {
        /// <summary>
        /// 활성 싱글톤 인스턴스. 씬 종료 중에는 null 반환.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (TryGetInstance(out T instance))
                {
                    return instance;
                }

                if (IsQuitting)
                {
                    return null;
                }

                var go = new GameObject(typeof(T).Name);
                cachedInstance = go.AddComponent<T>();
                return cachedInstance;
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
        /// 이 MonoBehaviour 인스턴스가 활성 싱글톤 인스턴스인지 여부.
        /// </summary>
        protected bool IsSingletonInstance { get; private set; }

        /// <summary>
        /// 이미 존재하는 싱글톤 인스턴스를 가져온다. 없으면 새로 만들지 않는다.
        /// </summary>
        public static bool TryGetInstance(out T instance)
        {
            instance = null;
            if (IsQuitting)
            {
                return false;
            }

            if (cachedInstance == null)
            {
                cachedInstance = UnityEngine.Object.FindFirstObjectByType<T>();
            }

            instance = cachedInstance;
            return instance != null;
        }

        /// <summary>
        /// 첫 등장 시 캐시에 자기 자신을 저장하고, 중복 인스턴스면 자기 자신을 파괴한다.
        /// </summary>
        protected virtual void Awake()
        {
            if (cachedInstance == null)
            {
                cachedInstance = (T)this;
                IsSingletonInstance = true;
                DontDestroyOnLoad(gameObject);
                return;
            }

            if (cachedInstance == this)
            {
                IsSingletonInstance = true;
                DontDestroyOnLoad(gameObject);
                return;
            }

            IsSingletonInstance = false;
            Destroy(gameObject);
        }

        /// <summary>
        /// 정적 캐시를 정리한다. 어플리케이션 종료 시 호출.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            IsQuitting = true;
            cachedInstance = null;
            IsSingletonInstance = false;
        }

        /// <summary>
        /// 파괴 시 캐시가 자기 자신이면 정리한다.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (cachedInstance == this)
            {
                cachedInstance = null;
            }

            IsSingletonInstance = false;
        }
    }
}

