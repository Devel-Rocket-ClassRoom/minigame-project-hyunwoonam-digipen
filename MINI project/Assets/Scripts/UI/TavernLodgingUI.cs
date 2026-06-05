using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    /// <summary>Safe1 Content_LODGING 표시/입력 계층. 참조와 REST OnClick은 Inspector에서 연결한다.</summary>
    public sealed class TavernLodgingUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text costText;

        [SerializeField]
        private Button restButton;

        private void Awake()
        {
            if (costText == null || restButton == null)
            {
                Debug.LogError(
                    "[TavernLodgingUI] costText / restButton Inspector 참조가 누락되었습니다."
                );
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

        public void Refresh()
        {
            if (!enabled || !TryGetRun(out GameRunState run))
            {
                return;
            }

            int partySize = TavernLodging.GetPartySize(run);
            int totalCost = TavernLodging.GetRestCost(run);
            int costPerPerson = TavernLodging.GetCostPerPerson(GetBalance());
            string people = partySize == 1 ? "person" : "people";
            costText.text =
                "Cost: "
                + costPerPerson
                + " G per person | Party: "
                + partySize
                + " "
                + people
                + " | Total: "
                + totalCost
                + " G";
            restButton.interactable = TavernLodging.CanRest(run);
        }

        public void Rest()
        {
            if (TryGetRun(out GameRunState run) && TavernLodging.TryRest(run))
            {
                Refresh();
            }
        }

        private void SubscribeEvents()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnGoldChanged -= HandleGoldChanged;
                gsm.Events.OnRosterChanged -= HandleRosterChanged;
                gsm.Events.OnDayChanged -= HandleDayChanged;
                gsm.Events.OnGoldChanged += HandleGoldChanged;
                gsm.Events.OnRosterChanged += HandleRosterChanged;
                gsm.Events.OnDayChanged += HandleDayChanged;
            }
        }

        private void UnsubscribeEvents()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnGoldChanged -= HandleGoldChanged;
                gsm.Events.OnRosterChanged -= HandleRosterChanged;
                gsm.Events.OnDayChanged -= HandleDayChanged;
            }
        }

        private void HandleGoldChanged(int gold)
        {
            Refresh();
        }

        private void HandleRosterChanged(int companionId, bool joined)
        {
            Refresh();
        }

        private void HandleDayChanged(int day)
        {
            Refresh();
        }

        private static bool TryGetRun(out GameRunState run)
        {
            run = null;
            if (
                !GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                || gsm.CurrentRun?.Player == null
            )
            {
                return false;
            }

            run = gsm.CurrentRun;
            return true;
        }

        private static BalanceData GetBalance()
        {
            return GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                ? gsm.Data?.Balance
                : null;
        }
    }
}
