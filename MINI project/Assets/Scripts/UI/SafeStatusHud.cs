using TMPro;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// Safe 씬 상단 상태 표시. 씬에 직접 배치된 Gold/HP/MP TMP_Text 참조만 사용한다.
    /// </summary>
    public sealed class SafeStatusHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text mpText;

        private int lastGold = int.MinValue;
        private int lastCurrentHp = int.MinValue;
        private int lastMaxHp = int.MinValue;
        private int lastCurrentMp = int.MinValue;
        private int lastMaxMp = int.MinValue;

        private void Awake()
        {
            if (!ValidateReferences())
            {
                enabled = false;
            }
        }

        private void OnEnable()
        {
            SubscribeEvents();
            Refresh();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void Update()
        {
            RefreshIfChanged();
        }

        public void Refresh()
        {
            if (!TryGetRunState(out GameRunState run) || run.Player?.Stats == null)
            {
                Debug.LogError("[SafeStatusHud] GameSystemManager.CurrentRun.Player.Stats 참조가 없습니다.");
                return;
            }

            StatBlock stats = run.Player.Stats;
            ApplyValues(run.Gold, stats.CurrentHP, stats.MaxHP, stats.CurrentMP, stats.MaxMP);
        }

        internal static string FormatGold(int gold)
        {
            return Mathf.Max(0, gold) + " G";
        }

        internal static string FormatHp(int current, int max)
        {
            int safeMax = Mathf.Max(0, max);
            return "HP " + Mathf.Clamp(current, 0, safeMax) + " / " + safeMax;
        }

        internal static string FormatMp(int current, int max)
        {
            int safeMax = Mathf.Max(0, max);
            return "MP " + Mathf.Clamp(current, 0, safeMax) + " / " + safeMax;
        }

        private void RefreshGold(int gold)
        {
            lastGold = int.MinValue;
            RefreshIfChanged();
        }

        private void RefreshIfChanged()
        {
            if (!TryGetRunState(out GameRunState run) || run.Player?.Stats == null)
            {
                return;
            }

            StatBlock stats = run.Player.Stats;
            if (run.Gold == lastGold
                && stats.CurrentHP == lastCurrentHp
                && stats.MaxHP == lastMaxHp
                && stats.CurrentMP == lastCurrentMp
                && stats.MaxMP == lastMaxMp)
            {
                return;
            }

            ApplyValues(run.Gold, stats.CurrentHP, stats.MaxHP, stats.CurrentMP, stats.MaxMP);
        }

        private void ApplyValues(int gold, int currentHp, int maxHp, int currentMp, int maxMp)
        {
            goldText.text = FormatGold(gold);
            hpText.text = FormatHp(currentHp, maxHp);
            mpText.text = FormatMp(currentMp, maxMp);

            lastGold = gold;
            lastCurrentHp = currentHp;
            lastMaxHp = maxHp;
            lastCurrentMp = currentMp;
            lastMaxMp = maxMp;
        }

        private void SubscribeEvents()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnGoldChanged -= RefreshGold;
                gsm.Events.OnInventoryChanged -= Refresh;
                gsm.Events.OnEquipmentChanged -= Refresh;
                gsm.Events.OnPlayerLevelUp -= RefreshAfterLevelUp;
                gsm.Events.OnGoldChanged += RefreshGold;
                gsm.Events.OnInventoryChanged += Refresh;
                gsm.Events.OnEquipmentChanged += Refresh;
                gsm.Events.OnPlayerLevelUp += RefreshAfterLevelUp;
            }
        }

        private void UnsubscribeEvents()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnGoldChanged -= RefreshGold;
                gsm.Events.OnInventoryChanged -= Refresh;
                gsm.Events.OnEquipmentChanged -= Refresh;
                gsm.Events.OnPlayerLevelUp -= RefreshAfterLevelUp;
            }
        }

        private void RefreshAfterLevelUp(int level)
        {
            Refresh();
        }

        private static bool TryGetRunState(out GameRunState run)
        {
            run = null;
            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm) || gsm.CurrentRun == null)
            {
                return false;
            }

            run = gsm.CurrentRun;
            return true;
        }

        private bool ValidateReferences()
        {
            bool valid = goldText != null && hpText != null && mpText != null;
            if (!valid)
            {
                Debug.LogError("[SafeStatusHud] Gold/HP/MP 텍스트 참조가 씬에서 직접 할당되어 있지 않습니다.");
            }

            return valid;
        }
    }
}
