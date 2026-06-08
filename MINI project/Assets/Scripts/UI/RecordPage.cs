using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    /// <summary>
    /// 메인 메뉴 Record(비석) 패널. 저장된 최종 승리 기록을 나열하고, 선택 시 그 시점 상태로 플레이한다.
    /// 직렬화 참조만 사용한다(런타임 hierarchy 경로 검색 없음).
    /// 행은 rowTemplate 을 복제해 만든다. rowTemplate 은 container 하위의 비활성 Button 이어야 한다.
    /// </summary>
    public sealed class RecordPage : MonoBehaviour
    {
        [SerializeField]
        private GameObject panelRoot;

        /// <summary>기록 행이 추가될 부모(스크롤 Content 등).</summary>
        [SerializeField]
        private Transform container;

        /// <summary>복제될 기록 행 템플릿(비활성 Button + 하위 Text/TMP_Text).</summary>
        [SerializeField]
        private Button rowTemplate;

        /// <summary>기록이 없을 때 표시할 안내 오브젝트(선택).</summary>
        [SerializeField]
        private GameObject emptyLabel;

        [SerializeField]
        private Button closeButton;

        private readonly List<GameObject> spawnedRows = new List<GameObject>();

        private void Awake()
        {
            if (panelRoot == null)
            {
                panelRoot = gameObject;
            }

            if (rowTemplate != null)
            {
                rowTemplate.gameObject.SetActive(false);
            }

            panelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
                closeButton.onClick.AddListener(Close);
            }
        }

        private void OnDisable()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
            }
        }

        /// <summary>패널을 열고 승리 기록 목록을 채운다.</summary>
        public void Open()
        {
            if (panelRoot != null)
            {
                panelRoot.transform.SetAsLastSibling();
                panelRoot.SetActive(true);
            }

            Populate();
        }

        /// <summary>패널을 닫는다.</summary>
        public void Close()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void Populate()
        {
            ClearRows();

            List<ClearRecord> clears = null;
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Save?.Records != null)
            {
                clears = gsm.Save.Records.Clears;
            }

            int playable = 0;
            if (clears != null && container != null && rowTemplate != null)
            {
                // 최신 기록이 위로 오도록 역순.
                for (int i = clears.Count - 1; i >= 0; i--)
                {
                    ClearRecord rec = clears[i];
                    if (rec == null || !rec.IsPlayable)
                    {
                        continue;
                    }

                    SpawnRow(rec);
                    playable++;
                }
            }

            if (emptyLabel != null)
            {
                emptyLabel.SetActive(playable == 0);
            }
        }

        private void SpawnRow(ClearRecord rec)
        {
            GameObject rowGo = Instantiate(rowTemplate.gameObject, container);
            rowGo.SetActive(true);
            spawnedRows.Add(rowGo);

            SetRowLabel(rowGo, BuildLabel(rec));

            var btn = rowGo.GetComponent<Button>();
            if (btn != null)
            {
                SaveSnapshot snapshot = rec.Snapshot;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnClickRecordRow(snapshot));
            }
        }

        private void OnClickRecordRow(SaveSnapshot snapshot)
        {
            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                return;
            }

            Close();
            gsm.PlayRecord(snapshot);
        }

        private static string BuildLabel(ClearRecord rec)
        {
            string name = string.IsNullOrEmpty(rec.Name) ? "Unknown" : rec.Name;
            string when = rec.TimestampIso;
            if (System.DateTime.TryParse(
                    rec.TimestampIso,
                    null,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out System.DateTime parsed))
            {
                when = parsed.ToString("yyyy-MM-dd HH:mm");
            }

            return string.IsNullOrEmpty(when) ? name : name + "  ·  " + when;
        }

        private static void SetRowLabel(GameObject rowGo, string text)
        {
            var tmp = rowGo.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null)
            {
                tmp.text = text;
                return;
            }

            var uiText = rowGo.GetComponentInChildren<Text>(true);
            if (uiText != null)
            {
                uiText.text = text;
            }
        }

        private void ClearRows()
        {
            for (int i = 0; i < spawnedRows.Count; i++)
            {
                if (spawnedRows[i] != null)
                {
                    Destroy(spawnedRows[i]);
                }
            }

            spawnedRows.Clear();
        }
    }
}
