using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Combat 씬의 PlayerUI/StatsPanel/HPBar를 실제 Player 체력에 연결합니다.
/// </summary>
public class PlayerStatsPanelUI : MonoBehaviour
{
    public const string PlayerUIObjectName = "PlayerUI";
    public const string StatsPanelPath = "StatsPanel";
    public const string HPBarPath = "StatsPanel/HPBar";

    [SerializeField]
    private Player player;

    [SerializeField]
    private Slider hpBar;

    public Player Player => player;
    public Slider HPBar => hpBar;

    public static PlayerStatsPanelUI EnsureForScene(Player targetPlayer)
    {
        if (targetPlayer == null)
        {
            return null;
        }

        GameObject playerUIObject = GameObject.Find(PlayerUIObjectName);
        if (playerUIObject == null)
        {
            Debug.LogWarning($"[PlayerStatsPanelUI] {PlayerUIObjectName} object is missing.");
            return null;
        }

        PlayerStatsPanelUI statsPanelUI = playerUIObject.GetComponent<PlayerStatsPanelUI>();
        if (statsPanelUI == null)
        {
            statsPanelUI = playerUIObject.AddComponent<PlayerStatsPanelUI>();
        }

        statsPanelUI.Bind(targetPlayer, FindHPBar(playerUIObject.transform));
        return statsPanelUI;
    }

    public void Bind(Player targetPlayer, Slider targetHPBar)
    {
        player = targetPlayer;
        hpBar = targetHPBar;
        Refresh();
    }

    public void Refresh()
    {
        if (player == null || hpBar == null)
        {
            return;
        }

        hpBar.interactable = false;
        hpBar.minValue = 0f;
        hpBar.maxValue = Mathf.Max(1, player.MaxHP);
        hpBar.value = Mathf.Clamp(player.CurrentHP, 0, player.MaxHP);
    }

    private void LateUpdate()
    {
        Refresh();
    }

    private static Slider FindHPBar(Transform playerUIRoot)
    {
        if (playerUIRoot == null)
        {
            return null;
        }

        Transform hpBarTransform = playerUIRoot.Find(HPBarPath);
        if (hpBarTransform != null && hpBarTransform.TryGetComponent(out Slider slider))
        {
            return slider;
        }

        Transform statsPanel = playerUIRoot.Find(StatsPanelPath);
        if (statsPanel == null)
        {
            return null;
        }

        Slider[] sliders = statsPanel.GetComponentsInChildren<Slider>(true);
        for (int i = 0; i < sliders.Length; i++)
        {
            if (sliders[i].name == "HPBar")
            {
                return sliders[i];
            }
        }

        return null;
    }
}
