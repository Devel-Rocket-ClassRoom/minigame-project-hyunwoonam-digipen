using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 템플릿 행 복제 풀링 공통 헬퍼. CombatRewardPage/ForgeEnhanceController/
    /// TavernRecruitmentController/GuildPartyController 가 각자 반복하던 동일 EnsureRowCount 골격을 통합한다.
    /// 첫 행(rows[0])을 템플릿으로 삼아 count 개까지 복제한다(축소는 호출부가 비활성화/숨김 처리).
    /// </summary>
    public static class UIRowPool
    {
        public static void EnsureCount<TView>(
            List<TView> rows,
            Transform parent,
            int count,
            Func<TView, GameObject> getRoot,
            Func<GameObject, TView> createView)
            where TView : class
        {
            if (rows == null || parent == null || rows.Count == 0)
            {
                return;
            }

            GameObject template = getRoot(rows[0]);
            while (rows.Count < count)
            {
                GameObject clone = UnityEngine.Object.Instantiate(template, parent);
                clone.name = template.name + "_Generated_" + rows.Count;
                TView view = createView(clone);
                if (view == null)
                {
                    UnityEngine.Object.Destroy(clone);
                    break;
                }

                rows.Add(view);
            }
        }
    }
}
