using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 씬에 이미 배치된 텍스트와 런타임에 갱신되는 텍스트를 공통 로컬라이즈/폰트 정책으로 보정한다.
/// </summary>
public sealed class RuntimeTextLocalizer : MonoBehaviour
{
    private const string FontResourcePath = "Fonts/Pretendard-Bold SDF";
    private const float RefreshIntervalSec = 0.25f;

    private static readonly Dictionary<string, string> LiteralKeys = new Dictionary<string, string>
    {
        ["NEW GAME"] = "menu_new_game",
        ["새 게임"] = "menu_new_game",
        ["CONTINUE"] = "menu_continue",
        ["이어하기"] = "menu_continue",
        ["OPTIONS"] = "menu_options",
        ["옵션"] = "menu_options",
        ["EXIT"] = "menu_exit",
        ["종료"] = "menu_exit",
        ["CLOSE"] = "ui_close",
        ["닫기"] = "ui_close",
        ["CONFIRM"] = "ui_confirm",
        ["확인"] = "ui_confirm",
        ["CANCEL"] = "ui_cancel",
        ["취소"] = "ui_cancel",
        ["EMPTY"] = "ui_empty",
        ["비어있음"] = "ui_empty",
        ["MISSING"] = "ui_missing",
        ["누락"] = "ui_missing",
        ["LOCKED"] = "rune_locked",
        ["잠김"] = "rune_locked",
        ["ENHANCE"] = "forge_enhance",
        ["강화"] = "forge_enhance",
        ["RECRUIT"] = "tavern_recruit",
        ["고용"] = "tavern_recruit",
        ["PURIFY"] = "sanctuary_purify",
        ["정화"] = "sanctuary_purify",
        ["OPEN"] = "tavern_storage_open",
        ["열림"] = "tavern_storage_open",
        ["MAX"] = "ui_max",
        ["최대"] = "ui_max",
        ["TODAY"] = "ui_today",
        ["오늘"] = "ui_today",
        ["Owned"] = "ui_owned",
        ["보유"] = "ui_owned",
        ["Not owned"] = "ui_not_owned",
        ["미보유"] = "ui_not_owned",
        ["Player"] = "ui_player",
        ["플레이어"] = "ui_player",
        ["Gold"] = "GOLD",
        ["Daily Gold"] = "DAILY GOLD",
        ["(Empty)"] = "ui_empty_parenthesized",
        ["(비어있음)"] = "ui_empty_parenthesized",
        ["Select action"] = "combat_select_action",
        ["행동 선택"] = "combat_select_action",
        ["Select ally"] = "combat_select_ally",
        ["아군 선택"] = "combat_select_ally",
        ["Select enemy"] = "combat_select_enemy",
        ["적 선택"] = "combat_select_enemy",
    };

    private TMP_FontAsset fontAsset;
    private float refreshTimer;

    private void Awake()
    {
        fontAsset = Resources.Load<TMP_FontAsset>(FontResourcePath);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnEnable()
    {
        if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
        {
            gsm.Events.OnLanguageChanged -= OnLanguageChanged;
            gsm.Events.OnLanguageChanged += OnLanguageChanged;
        }

        RefreshAll();
    }

    private void OnDisable()
    {
        if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
        {
            gsm.Events.OnLanguageChanged -= OnLanguageChanged;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        refreshTimer -= Time.unscaledDeltaTime;
        if (refreshTimer > 0f)
        {
            return;
        }

        refreshTimer = RefreshIntervalSec;
        RefreshAll();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshAll();
    }

    private void OnLanguageChanged(string _)
    {
        RefreshAll();
    }

    private void RefreshAll()
    {
        TMP_Text[] tmpTexts = FindObjectsByType<TMP_Text>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        foreach (TMP_Text text in tmpTexts)
        {
            if (text == null)
            {
                continue;
            }

            if (fontAsset != null && text.font != fontAsset)
            {
                text.font = fontAsset;
            }

            text.text = ResolveLocalizedText(text.gameObject, text.text);
        }

        Text[] legacyTexts = FindObjectsByType<Text>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        foreach (Text text in legacyTexts)
        {
            if (text == null)
            {
                continue;
            }

            text.text = ResolveLocalizedText(text.gameObject, text.text);
        }
    }

    private static string ResolveLocalizedText(GameObject owner, string currentText)
    {
        RuntimeTextSourceCache cache = owner.GetComponent<RuntimeTextSourceCache>();
        if (cache == null)
        {
            cache = owner.AddComponent<RuntimeTextSourceCache>();
        }

        if (string.IsNullOrEmpty(cache.SourceText))
        {
            cache.SourceText = currentText;
        }
        else if (currentText != cache.LastLocalizedText)
        {
            cache.SourceText = currentText;
        }

        cache.LastLocalizedText = LocalizeDynamicText(cache.SourceText);
        return cache.LastLocalizedText;
    }

    private static string LocalizeDynamicText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        string exact = ResolveSingleToken(text);
        if (exact != text)
        {
            return exact;
        }

        string[] lines = text.Split('\n');
        bool changed = false;
        for (int i = 0; i < lines.Length; i++)
        {
            string localized = LocalizeLine(lines[i]);
            changed |= localized != lines[i];
            lines[i] = localized;
        }

        return changed ? string.Join("\n", lines) : text;
    }

    private static string LocalizeLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return line;
        }

        string trimmed = line.Trim();
        string exact = ResolveSingleToken(trimmed);
        if (exact != trimmed)
        {
            return line.Replace(trimmed, exact);
        }

        int plusIndex = trimmed.LastIndexOf(" +", System.StringComparison.Ordinal);
        if (plusIndex > 0)
        {
            string keyPart = trimmed.Substring(0, plusIndex);
            string suffix = trimmed.Substring(plusIndex);
            string localized = ResolveSingleToken(keyPart);
            if (localized != keyPart)
            {
                return line.Replace(trimmed, localized + suffix);
            }
        }

        return line;
    }

    private static string ResolveSingleToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        if (LiteralKeys.TryGetValue(value, out string mappedKey))
        {
            return Loc.Get(mappedKey);
        }

        string localized = Loc.Get(value);
        if (localized != value)
        {
            return localized;
        }

        string key = ResolveLocalizedValueToKey(value);
        return key != value ? Loc.Get(key) : value;
    }

    private static string ResolveLocalizedValueToKey(string value)
    {
        if (
            string.IsNullOrWhiteSpace(value)
            || !GameSystemManager.TryGetInstance(out GameSystemManager gsm)
            || gsm.Data?.Language?.Table == null
        )
        {
            return value;
        }

        foreach (KeyValuePair<string, Dictionary<string, string>> row in gsm.Data.Language.Table)
        {
            if (row.Value == null)
            {
                continue;
            }

            foreach (string localized in row.Value.Values)
            {
                if (value == localized)
                {
                    return row.Key;
                }
            }
        }

        return value;
    }
}

public sealed class RuntimeTextSourceCache : MonoBehaviour
{
    public string SourceText;
    public string LastLocalizedText;
}
