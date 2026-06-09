using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class CsvParser
{
    public static IList<T> Parse<T>(string resourcePath, Func<IDictionary<string, string>, T> rowFactory)
    {
        var result = new List<T>();
        if (rowFactory == null)
        {
            GameLog.LogError("[CsvParser] rowFactory is null: " + resourcePath);
            return result;
        }

        string normalizedPath = NormalizeResourcePath(resourcePath);
        TextAsset asset = Resources.Load<TextAsset>(normalizedPath);
        if (asset == null)
        {
            GameLog.LogError("[CsvParser] CSV file missing: Resources/" + normalizedPath + ".csv");
            return result;
        }

        List<List<string>> rows = ParseRaw(asset.text);
        if (rows.Count == 0)
        {
            return result;
        }

        List<string> header = rows[0];
        for (int i = 1; i < rows.Count; i++)
        {
            var row = new Dictionary<string, string>(StringComparer.Ordinal);
            List<string> cells = rows[i];
            for (int column = 0; column < header.Count; column++)
            {
                string key = header[column];
                row[key] = column < cells.Count ? cells[column] : string.Empty;
            }

            T value = rowFactory(row);
            if (value != null)
            {
                result.Add(value);
            }
        }

        return result;
    }

    public static string GetString(IDictionary<string, string> row, string key, string defaultValue = "")
    {
        if (row == null || !row.TryGetValue(key, out string value))
        {
            return defaultValue;
        }

        return value ?? defaultValue;
    }

    public static int GetInt(IDictionary<string, string> row, string key, int defaultValue = 0)
    {
        string value = GetString(row, key, string.Empty);
        return TryParseInt(value, out int parsed) ? parsed : defaultValue;
    }

    public static float GetFloat(IDictionary<string, string> row, string key, float defaultValue = 0f)
    {
        string value = GetString(row, key, string.Empty);
        return TryParseFloat(value, out float parsed) ? parsed : defaultValue;
    }

    public static bool GetBool(IDictionary<string, string> row, string key, bool defaultValue = false)
    {
        string value = GetString(row, key, string.Empty);
        return TryParseBool(value, out bool parsed) ? parsed : defaultValue;
    }

    public static T GetEnum<T>(IDictionary<string, string> row, string key, T defaultValue) where T : struct
    {
        string value = GetString(row, key, string.Empty);
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        if (Enum.TryParse(value, true, out T parsed))
        {
            return parsed;
        }

        GameLog.LogError("[CsvParser] enum parse failed: " + typeof(T).Name + "." + value + " (" + key + ")");
        return defaultValue;
    }

    public static List<int> GetIntList(IDictionary<string, string> row, string key)
    {
        var result = new List<int>();
        string value = GetString(row, key, string.Empty);
        if (string.IsNullOrEmpty(value))
        {
            return result;
        }

        string[] parts = value.Split(';');
        for (int i = 0; i < parts.Length; i++)
        {
            if (TryParseInt(parts[i], out int parsed))
            {
                result.Add(parsed);
            }
        }

        return result;
    }

    public static List<string> GetStringList(IDictionary<string, string> row, string key)
    {
        var result = new List<string>();
        string value = GetString(row, key, string.Empty);
        if (string.IsNullOrEmpty(value))
        {
            return result;
        }

        string[] parts = value.Split(';');
        for (int i = 0; i < parts.Length; i++)
        {
            string item = parts[i].Trim();
            if (!string.IsNullOrEmpty(item))
            {
                result.Add(item);
            }
        }

        return result;
    }

    public static bool HasColumns(IDictionary<string, string> row, string typeName, params string[] columns)
    {
        bool ok = true;
        for (int i = 0; i < columns.Length; i++)
        {
            if (row == null || !row.ContainsKey(columns[i]))
            {
                GameLog.LogError("[Data] " + typeName + " 필수 컬럼 누락: " + columns[i]);
                ok = false;
            }
        }

        return ok;
    }

    public static bool TryParseInt(string value, out int result)
    {
        return int.TryParse((value ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }

    public static bool TryParseFloat(string value, out float result)
    {
        return float.TryParse((value ?? string.Empty).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out result);
    }

    public static bool TryParseBool(string value, out bool result)
    {
        string normalized = (value ?? string.Empty).Trim();
        if (string.Equals(normalized, "1", StringComparison.OrdinalIgnoreCase))
        {
            result = true;
            return true;
        }

        if (string.Equals(normalized, "0", StringComparison.OrdinalIgnoreCase))
        {
            result = false;
            return true;
        }

        return bool.TryParse(normalized, out result);
    }

    private static string NormalizeResourcePath(string resourcePath)
    {
        string value = (resourcePath ?? string.Empty).Replace('\\', '/');
        if (value.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            value = value.Substring(0, value.Length - 4);
        }

        return value;
    }

    private static List<List<string>> ParseRaw(string text)
    {
        var rows = new List<List<string>>();
        var currentRow = new List<string>();
        var currentCell = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < text.Length && text[i + 1] == '"')
                {
                    currentCell.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
                continue;
            }

            if (!inQuotes && c == ',')
            {
                currentRow.Add(currentCell.ToString().Trim());
                currentCell.Length = 0;
                continue;
            }

            if (!inQuotes && (c == '\n' || c == '\r'))
            {
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                }

                FinishRow(rows, currentRow, currentCell);
                currentRow = new List<string>();
                currentCell.Length = 0;
                continue;
            }

            currentCell.Append(c);
        }

        FinishRow(rows, currentRow, currentCell);
        return rows;
    }

    private static void FinishRow(List<List<string>> rows, List<string> currentRow, System.Text.StringBuilder currentCell)
    {
        currentRow.Add(currentCell.ToString().Trim());
        if (currentRow.Count == 1 && string.IsNullOrEmpty(currentRow[0]))
        {
            return;
        }

        if (currentRow.Count > 0 && currentRow[0].StartsWith("#", StringComparison.Ordinal))
        {
            return;
        }

        rows.Add(currentRow);
    }
}
