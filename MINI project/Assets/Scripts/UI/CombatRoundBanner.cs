using System.Collections;
using TMPro;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 라운드 배너는 표시하지 않는다. 이전 세이브/씬 참조 호환을 위해 타입만 유지한다.
    /// </summary>
    public sealed class CombatRoundBanner : MonoBehaviour
    {
        /// <summary>라운드 배너 비활성화 정책에 따라 아무 것도 표시하지 않는다.</summary>
        public static void Show(int roundNumber)
        {
        }
    }
}
