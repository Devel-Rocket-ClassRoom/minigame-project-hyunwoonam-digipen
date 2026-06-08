using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 전투 중 사용 가능한 소모 4칸. 인벤토리에서 미리 지정(전투 외에서만 변경 가능).
    /// </summary>
    public sealed class ConsumableSlots
    {
        public const int SlotCount = 4;

        /// <summary>슬롯 4칸의 아이템 ID(0=비어있음).</summary>
        public int[] SlotItemIds = new int[SlotCount];

        private delegate void ConsumableEffectHandler(ItemData data, EntityBase user);

        private static readonly Dictionary<string, ConsumableEffectHandler> EffectHandlers = new Dictionary<string, ConsumableEffectHandler>
        {
            ["Escape"] = ApplyEscape,
            ["HealHP"] = ApplyHealHP,
            ["HP_Potion"] = ApplyHealHP,
            ["HealMP"] = ApplyHealMP,
            ["MP_Potion"] = ApplyHealMP,
        };

        /// <summary>
        /// 슬롯 설정. 전투 중에는 호출 금지(UI와 도메인 양쪽에서 차단).
        /// </summary>
        public bool TrySetSlot(int slotIndex, int itemId, InventoryState inv)
        {
            if (slotIndex < 0 || slotIndex >= SlotItemIds.Length)
            {
                return false;
            }

            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                GameLog.LogError("[ConsumableSlots.TrySetSlot] GameSystemManager 참조가 없습니다.");
                return false;
            }

            if (gsm.CombatContext != null || (gsm.Scenes != null && gsm.Scenes.CurrentSceneId == SceneId.Combat))
            {
                return false;
            }

            if (itemId == 0)
            {
                SlotItemIds[slotIndex] = 0;
                gsm.Events?.RaiseInventoryChanged();
                return true;
            }

            if (inv == null || inv.CountOf(itemId) <= 0)
            {
                return false;
            }

            if (gsm.Data?.Items == null || !gsm.Data.Items.TryGetValue(itemId, out ItemData data))
            {
                GameLog.LogError("[ConsumableSlots.TrySetSlot] 아이템 ID 없음: " + itemId);
                return false;
            }

            if (data.Category != ItemCategory.Consumable)
            {
                return false;
            }

            for (int i = 0; i < SlotItemIds.Length; i++)
            {
                if (i != slotIndex && SlotItemIds[i] == itemId)
                {
                    SlotItemIds[i] = 0;
                }
            }

            SlotItemIds[slotIndex] = itemId;
            gsm.Events?.RaiseInventoryChanged();
            return true;
        }

        /// <summary>
        /// 전투 중 슬롯 사용. 행동 비용 없음(설계변경.txt).
        /// </summary>
        public bool TryUse(int slotIndex, EntityBase user, InventoryState inv)
        {
            if (slotIndex < 0 || slotIndex >= SlotItemIds.Length || user == null || inv == null)
            {
                return false;
            }

            int itemId = SlotItemIds[slotIndex];
            if (itemId == 0 || inv.CountOf(itemId) <= 0)
            {
                return false;
            }

            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm) || !gsm.Data.Items.TryGetValue(itemId, out ItemData data))
            {
                return false;
            }

            if (!inv.Remove(itemId, 1))
            {
                return false;
            }

            ApplyConsumableEffect(data, user);
            if (inv.CountOf(itemId) <= 0)
            {
                SlotItemIds[slotIndex] = 0;
            }

            return true;
        }

        /// <summary>
        /// 인벤토리 동기화 후 잔량 0이 된 슬롯은 자동으로 비운다(설계: 전투 외 변경 시).
        /// </summary>
        public void PruneEmptySlots(InventoryState inv)
        {
            if (inv == null)
            {
                return;
            }

            for (int i = 0; i < SlotItemIds.Length; i++)
            {
                if (SlotItemIds[i] != 0 && inv.CountOf(SlotItemIds[i]) <= 0)
                {
                    SlotItemIds[i] = 0;
                }
            }
        }

        private static void ApplyConsumableEffect(ItemData data, EntityBase user)
        {
            if (data == null || user?.Stats == null)
            {
                return;
            }

            string effectKey = ResolveEffectKey(data);
            if (EffectHandlers.TryGetValue(effectKey, out ConsumableEffectHandler handler))
            {
                handler(data, user);
                return;
            }

            ApplyHealHP(data, user);
        }

        private static string ResolveEffectKey(ItemData data)
        {
            if (data.IsRetreat || data.SubCategory == "Escape" || data.ConsumeEffectKey == "Escape")
            {
                return "Escape";
            }

            if (!string.IsNullOrEmpty(data.ConsumeEffectKey))
            {
                return data.ConsumeEffectKey;
            }

            return data.SubCategory;
        }

        private static void ApplyEscape(ItemData data, EntityBase user)
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                gsm.EndCombat(CombatResult.Retreat, null);
            }
        }

        private static void ApplyHealHP(ItemData data, EntityBase user)
        {
            int amount = UnityEngine.Mathf.RoundToInt(data.ParamValue);
            user.ApplyHeal(amount);
        }

        private static void ApplyHealMP(ItemData data, EntityBase user)
        {
            int amount = UnityEngine.Mathf.RoundToInt(data.ParamValue);
            user.Stats.CurrentMP = UnityEngine.Mathf.Min(user.Stats.MaxMP, user.Stats.CurrentMP + amount);
        }
    }
}

