/// <summary>
/// 현재 언어로 로컬라이즈된 문자열을 반환하는 정적 헬퍼.
/// GSM이 없거나 키가 없으면 key 자체를 반환한다.
/// </summary>
public static class Loc
{
    /// <summary>key 에 대응하는 현재 언어 문자열 반환.</summary>
    public static string Get(string key)
    {
        if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm))
        {
            return key;
        }

        string lang = gsm.Options?.CurrentLanguage ?? "ko";
        string localized = gsm.Data?.Language?.Get(key, lang) ?? key;
        return localized.Replace("\\n", "\n");
    }

    /// <summary>key 를 가져와 string.Format 으로 인자를 삽입한다.</summary>
    public static string Format(string key, params object[] args)
    {
        string template = Get(key);
        if (args == null || args.Length == 0)
        {
            return template;
        }

        try
        {
            return string.Format(template, args);
        }
        catch
        {
            return template;
        }
    }
}
