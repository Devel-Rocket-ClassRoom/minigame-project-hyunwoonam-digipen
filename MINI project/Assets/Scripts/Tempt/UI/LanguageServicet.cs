using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 언어 변경 + 키 → 문자열 조회. LanguageDatat에서 데이터를 받는다.
    /// </summary>
    public sealed class LanguageServicet
    {
        /// <summary>현재 언어 코드.</summary>
        public string Current { get; private set; } = "ko";

        /// <summary>참조 데이터.</summary>
        public LanguageDatat Data;

        /// <summary>이벤트 버스 참조(언어 변경 발행).</summary>
        public EventBust Events;

        /// <summary>
        /// 언어 변경.
        /// </summary>
        public void Change(string lang)
        {
            // 동작 요약:
            // - Data.Languages에 포함되는지 검사.
            // - Current = lang.
            // - Events.RaiseLanguageChanged(lang).
        }

        /// <summary>
        /// 키 조회.
        /// </summary>
        public string Get(string key)
        {
            // 동작 요약: Data.Get(key, Current) 반환.
            return string.Empty;
        }

        /// <summary>
        /// 키 + 인자.
        /// </summary>
        public string Get(string key, params object[] args)
        {
            // 동작 요약: string.Format(Data.Get(key, Current), args).
            return string.Empty;
        }
    }
}
