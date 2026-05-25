using System;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 게임 씬 전환 FSM. Unity 기본 SceneManager 위에서 동작하며,
    /// 씬별 컨트롤러(SceneControllerBaset 파생)의 라이프사이클을 관리한다.
    /// </summary>
    public sealed class GameSceneManagert : MonoBehaviour
    {
        /// <summary>현재 활성 씬 ID.</summary>
        public SceneIdt CurrentSceneId { get; private set; }

        /// <summary>현재 활성 씬 컨트롤러(없으면 null).</summary>
        public SceneControllerBaset ActiveController { get; private set; }

        /// <summary>전환 직전 발생. (from, to)</summary>
        public event Action<SceneIdt, SceneIdt> OnSceneWillChange;

        /// <summary>전환 완료 후 발생. (from, to)</summary>
        public event Action<SceneIdt, SceneIdt> OnSceneChanged;

        /// <summary>
        /// 임의 씬으로 전환을 요청한다.
        /// </summary>
        /// <param name="next">전환할 씬 ID.</param>
        public void RequestScene(SceneIdt next)
        {
            // 동작 요약:
            // - OnSceneWillChange 발생.
            // - ActiveController.OnExit() 호출.
            // - SceneManager.LoadSceneAsync(SceneNameOf(next)).
            // - 로드 완료 후 ActiveController = FindObjectOfType<SceneControllerBaset>().
            // - ActiveController.OnEnter() 호출.
            // - CurrentSceneId 갱신.
            // - OnSceneChanged 발생.
        }

        /// <summary>메인 메뉴로 이동.</summary>
        public void LoadMainMenu()
        {
            // 동작 요약: RequestScene(SceneIdt.MainMenu).
        }

        /// <summary>지정한 안전지대로 이동.</summary>
        /// <param name="safeZoneIndex">0~5.</param>
        public void LoadSafeZone(int safeZoneIndex)
        {
            // 동작 요약:
            // - safeZoneIndex 검증(0~5).
            // - RequestScene(SceneIdt.Safe0 + safeZoneIndex).
        }

        /// <summary>플로어 맵으로 이동.</summary>
        public void LoadFloorMap()
        {
            // 동작 요약: RequestScene(SceneIdt.FloorMap).
        }

        /// <summary>전투 씬으로 이동.</summary>
        public void LoadCombat()
        {
            // 동작 요약: RequestScene(SceneIdt.Combat).
        }

        /// <summary>
        /// 씬 ID에서 Unity 씬 이름을 얻는다.
        /// </summary>
        private string SceneNameOf(SceneIdt id)
        {
            // 동작 요약:
            // - enum → 실제 씬 이름 매핑(상수 테이블).
            // - 미정의 ID 입력 시 예외 발생.
            return string.Empty;
        }
    }

    /// <summary>씬 ID 열거.</summary>
    public enum SceneIdt
    {
        /// <summary>부팅 씬(선택, 비어 있을 수 있음).</summary>
        Boot,

        /// <summary>메인 메뉴.</summary>
        MainMenu,

        /// <summary>안전지대 0 (시작/비석/묘비/최초 룬 선택).</summary>
        Safe0,

        /// <summary>안전지대 1 (주점/상점/길드/대장간/신전).</summary>
        Safe1,

        /// <summary>안전지대 2 (성소 - 침식 관리).</summary>
        Safe2,

        /// <summary>안전지대 3 (광산1).</summary>
        Safe3,

        /// <summary>안전지대 4 (광산2).</summary>
        Safe4,

        /// <summary>안전지대 5 (광산3).</summary>
        Safe5,

        /// <summary>플로어 맵 (노드 스크롤).</summary>
        FloorMap,

        /// <summary>전투 씬.</summary>
        Combat,
    }

    /// <summary>
    /// 모든 씬 컨트롤러의 베이스. GameSceneManagert가 라이프사이클을 호출한다.
    /// </summary>
    public abstract class SceneControllerBaset : MonoBehaviour
    {
        /// <summary>씬 진입 직후 호출.</summary>
        public abstract void OnEnter();

        /// <summary>씬 이탈 직전 호출.</summary>
        public abstract void OnExit();

        /// <summary>매 프레임 호출(필요한 컨트롤러만 override).</summary>
        public virtual void OnSceneUpdate()
        {
            // 동작 요약: 기본 구현 없음.
        }
    }
}
