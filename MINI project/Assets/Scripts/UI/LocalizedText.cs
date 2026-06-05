using TMPro;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// TMP_Text 컴포넌트가 있는 GameObject에 부착한다.
    /// key 에 대응하는 현재 언어 문자열을 표시하며, 언어 변경 시 자동 갱신된다.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class LocalizedText : MonoBehaviour
    {
        [SerializeField]
        private string key;

        private TMP_Text label;

        private void Awake()
        {
            label = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                gsm.Events.OnLanguageChanged -= OnLanguageChanged;
                gsm.Events.OnLanguageChanged += OnLanguageChanged;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                gsm.Events.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        private void OnLanguageChanged(string _)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (label != null && !string.IsNullOrEmpty(key))
            {
                label.text = Loc.Get(key);
            }
        }
    }
}
