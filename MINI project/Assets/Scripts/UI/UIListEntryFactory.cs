using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Tempt
{
    public static class UIListEntryFactory
    {
        public static Button SpawnListEntry(Button prefab, Transform parent, string label, UnityAction onClick, string missingTextLog)
        {
            Button entry = Object.Instantiate(prefab, parent);
            entry.gameObject.SetActive(true);
            entry.onClick.RemoveAllListeners();
            entry.onClick.AddListener(onClick);

            if (TrySetLabel(entry, label))
            {
                return entry;
            }

            GameLog.LogError(missingTextLog);
            entry.interactable = false;
            return entry;
        }

        private static bool TrySetLabel(Button entry, string label)
        {
            TMP_Text tmp = entry.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null)
            {
                tmp.text = label;
                return true;
            }

            Text text = entry.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.text = label;
                return true;
            }

            return false;
        }
    }
}
