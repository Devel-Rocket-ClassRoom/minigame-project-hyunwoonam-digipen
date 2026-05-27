using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tempt
{
    /// <summary>
    /// 게임 씬 전환 FSM. Unity 기본 SceneManager 위에서 동작하며,
    /// 씬별 컨트롤러(SceneControllerBase 파생)의 라이프사이클을 관리한다.
    /// </summary>
    public sealed class GameSceneManager : MonoBehaviour
    {
        /// <summary>현재 활성 씬 ID.</summary>
        public SceneId CurrentSceneId { get; private set; }

        /// <summary>현재 활성 씬 컨트롤러(없으면 null).</summary>
        public SceneControllerBase ActiveController { get; private set; }

        /// <summary>전환 직전 발생. (from, to)</summary>
        public event Action<SceneId, SceneId> OnSceneWillChange;

        /// <summary>전환 완료 후 발생. (from, to)</summary>
        public event Action<SceneId, SceneId> OnSceneChanged;

        /// <summary>
        /// 임의 씬으로 전환을 요청한다.
        /// </summary>
        /// <param name="next">전환할 씬 ID.</param>
        public void RequestScene(SceneId next)
        {
            SceneId from = CurrentSceneId; //Wave0write
            if (from == next && ActiveController != null) //Wave0write
            { //Wave0write
                ActiveController.OnEnter(); //Wave0write
                return; //Wave0write
            } //Wave0write

            OnSceneWillChange?.Invoke(from, next); //Wave0write
            ActiveController?.OnExit(); //Wave0write
            StartCoroutine(LoadSceneRoutine(next, from)); //Wave0write
        }

        /// <summary>메인 메뉴로 이동.</summary>
        public void LoadMainMenu()
        {
            RequestScene(SceneId.MainMenu); //Wave0write
        }

        /// <summary>지정한 안전지대로 이동.</summary>
        /// <param name="safeZoneIndex">0~5.</param>
        public void LoadSafeZone(int safeZoneIndex)
        {
            if (safeZoneIndex < 0 || safeZoneIndex > 5) //Wave0write
            { //Wave0write
                Debug.LogWarning("[GameSceneManager] SafeZone index out of range: " + safeZoneIndex); //Wave0write
                return; //Wave0write
            } //Wave0write

            RequestScene((SceneId)((int)SceneId.Safe0 + safeZoneIndex)); //Wave0write
        }

        /// <summary>플로어 맵으로 이동.</summary>
        public void LoadFloorMap()
        {
            RequestScene(SceneId.FloorMap); //Wave0write
        }

        /// <summary>전투 씬으로 이동.</summary>
        public void LoadCombat()
        {
            RequestScene(SceneId.Combat); //Wave0write
        }

        /// <summary>
        /// 씬 ID에서 Unity 씬 이름을 얻는다.
        /// </summary>
        private string SceneNameOf(SceneId id)
        {
            switch (id) //Wave0write
            { //Wave0write
                case SceneId.Boot: return "Boot"; //Wave0write
                case SceneId.MainMenu: return "MainMenu"; //Wave0write
                case SceneId.Safe0: return "Safe0"; //Wave0write
                case SceneId.Safe1: return "Safe1"; //Wave0write
                case SceneId.Safe2: return "Safe2"; //Wave0write
                case SceneId.Safe3: return "Safe3"; //Wave0write
                case SceneId.Safe4: return "Safe4"; //Wave0write
                case SceneId.Safe5: return "Safe5"; //Wave0write
                case SceneId.FloorMap: return "FloorMap"; //Wave0write
                case SceneId.Combat: return "Combat"; //Wave0write
                default: throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown scene id"); //Wave0write
            } //Wave0write
        }

        private IEnumerator LoadSceneRoutine(SceneId next, SceneId from) //Wave0write
        { //Wave0write
            string sceneName = SceneNameOf(next); //Wave0write
            if (Application.CanStreamedLevelBeLoaded(sceneName)) //Wave0write
            { //Wave0write
                AsyncOperation op = SceneManager.LoadSceneAsync(sceneName); //Wave0write
                while (op != null && !op.isDone) //Wave0write
                { //Wave0write
                    yield return null; //Wave0write
                } //Wave0write
            } //Wave0write
            else //Wave0write
            { //Wave0write
                yield return null; //Wave0write
            } //Wave0write

            ActiveController = FindAnyObjectByType<SceneControllerBase>(); //Wave0write
            if (ActiveController == null)
            {
                Debug.LogError("[GameSceneManager] SceneControllerBase missing in scene: " + sceneName);
            }

            CurrentSceneId = next; //Wave0write
            ActiveController?.OnEnter(); //Wave0write
            OnSceneChanged?.Invoke(from, next); //Wave0write
        } //Wave0write

        private void Update() //Wave0write
        { //Wave0write
            ActiveController?.OnSceneUpdate(); //Wave0write
        } //Wave0write
    }

    /// <summary>씬 ID 열거.</summary>
    public enum SceneId
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
    public abstract class SceneControllerBase : MonoBehaviour
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
