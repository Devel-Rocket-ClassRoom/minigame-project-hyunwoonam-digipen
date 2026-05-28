using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 다국어 텍스트 테이블. CSV(key, ko, en, ...).
    /// </summary>
    public sealed class LanguageDatat
    {
        /// <summary>지원 언어 코드 목록.</summary>
        public List<string> Languages;

        /// <summary>키 → 언어 코드 → 문자열.</summary>
        public Dictionary<string, Dictionary<string, string>> Table;

        /// <summary>
        /// 키와 언어로 문자열을 조회.
        /// </summary>
        public string Get(string key, string lang)
        {
            // 동작 요약:
            // - Table[key][lang] 조회.
            // - 없으면 첫 번째 언어 fallback.
            // - 그것도 없으면 key 자체 반환(누락 경고용).
            //TODO: if (Table.TryGetValue(key, out var langMap))
            //TODO: {
            //TODO:     if (langMap.TryGetValue(lang, out var text)) return text;
            //TODO:     if (Languages.Count > 0 && langMap.TryGetValue(Languages[0], out text)) return text;
            //TODO: }
            //TODO: return key; // 누락 키 그대로 반환(디버그용)
            return string.Empty;
        }
    }
}
