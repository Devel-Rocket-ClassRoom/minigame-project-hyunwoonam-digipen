using System.Collections.Generic;

/// <summary>
/// 다국어 텍스트 테이블. CSV(key, ko, en, ...).
/// </summary>
public sealed class LanguageData
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
        if (Table != null && Table.TryGetValue(key, out Dictionary<string, string> row))
        {
            if (!string.IsNullOrEmpty(lang) && row.TryGetValue(lang, out string localized))
            {
                return localized;
            }

            if (Languages != null && Languages.Count > 0 && row.TryGetValue(Languages[0], out string fallback))
            {
                return fallback;
            }
        }

        return key;
    }

    public static LanguageData FromRows(IList<IDictionary<string, string>> rows)
    {
        var data = new LanguageData
        {
            Languages = new List<string>(),
            Table = new Dictionary<string, Dictionary<string, string>>(),
        };

        if (rows == null)
        {
            return data;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            IDictionary<string, string> row = rows[i];
            string key = CsvParser.GetString(row, "Key");
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            var values = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> pair in row)
            {
                if (pair.Key == "Key")
                {
                    continue;
                }

                if (!data.Languages.Contains(pair.Key))
                {
                    data.Languages.Add(pair.Key);
                }

                values[pair.Key] = pair.Value;
            }

            data.Table[key] = values;
        }

        return data;
    }
}

