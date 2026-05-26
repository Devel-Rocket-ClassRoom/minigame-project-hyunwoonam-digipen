using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// Continue 저장/이어하기 + 영구 기록(비석/묘비) 관리.
    /// 데이터 저장 형식은 JSON, 단일 슬롯.
    /// Continue JSON은 플로어 맵 전체 노드 구조와 다른 런 진행 상태를 하나의 SaveSnapshott로 보관한다.
    /// </summary>
    public sealed class SaveLoadert
    {
        /// <summary>현재 저장된 Continue 데이터(없으면 null).</summary>
        public SaveSnapshott Continue { get; private set; }

        /// <summary>영구 기록(비석 = 클리어, 묘비 = 사망).</summary>
        public RecordBookt Records { get; private set; }

        /// <summary>
        /// 저장 파일과 기록 파일을 일괄 로드.
        /// </summary>
        public void LoadAll()
        {
            // 동작 요약:
            // - PersistentDataPath/save.json 읽기 → Continue.
            // - PersistentDataPath/records.json 읽기 → Records.
            // - 파일이 없으면 빈 객체 생성.
            // - Continue.FloorMap은 seed가 아니라 저장된 전체 노드 목록을 포함한다.
            // - 실제 FloorMapModelt 복원은 GameSystemManagert.ContinueGame() 또는 별도 복원 헬퍼가 담당.
            //TODO: string savePath    = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "save.json");
            //TODO: string recordPath  = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "records.json");
            //TODO: Continue = System.IO.File.Exists(savePath)
            //TODO:     ? UnityEngine.JsonUtility.FromJson<SaveSnapshott>(System.IO.File.ReadAllText(savePath))
            //TODO:     : null;
            //TODO: Records = System.IO.File.Exists(recordPath)
            //TODO:     ? UnityEngine.JsonUtility.FromJson<RecordBookt>(System.IO.File.ReadAllText(recordPath))
            //TODO:     : new RecordBookt();
            string savePath = Path.Combine(Application.persistentDataPath, "save.json"); //Wave0write
            string recordPath = Path.Combine(Application.persistentDataPath, "records.json"); //Wave0write
            Continue = File.Exists(savePath) ? JsonUtility.FromJson<SaveSnapshott>(File.ReadAllText(savePath)) : null; //Wave0write
            Records = File.Exists(recordPath) ? JsonUtility.FromJson<RecordBookt>(File.ReadAllText(recordPath)) : new RecordBookt(); //Wave0write
            if (Records == null) //Wave0write
            { //Wave0write
                Records = new RecordBookt(); //Wave0write
            } //Wave0write
        }

        /// <summary>
        /// 현재 런 상태 전체 스냅샷을 저장.
        /// </summary>
        public void SaveSnapshot()
        {
            // 동작 요약:
            // - GameSystemManagert.Instance.CurrentRun 전체를 SaveSnapshott로 직렬화.
            // - CurrentRun.FloorMap.NodesById/NodesByFloor의 모든 FloorNodet을 FloorNodeSnapshott로 변환.
            // - FloorMapSnapshott.NextSelectableFloor와 Nodes를 함께 저장.
            // - seed 저장/seed 기반 재생성은 사용하지 않음.
            // - JSON 저장.
            // - Continue = snapshot 저장.
            //TODO: var run = GameSystemManagert.Instance.CurrentRun;
            //TODO: if (run == null) return;
            //TODO: var snapshot = SaveSnapshott.FromGameRunStatet(run, GameSystemManagert.Instance.Scenes.CurrentSceneId);
            //TODO: string json = UnityEngine.JsonUtility.ToJson(snapshot, prettyPrint: false);
            //TODO: string path = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "save.json");
            //TODO: System.IO.File.WriteAllText(path, json);
            //TODO: Continue = snapshot;
            if (!GameSystemManagert.TryGetInstance(out GameSystemManagert gsm) || gsm.CurrentRun == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            SceneIdt sceneId = gsm.Scenes != null ? gsm.Scenes.CurrentSceneId : SceneIdt.MainMenu; //Wave0write
            SaveSnapshott snapshot = SaveSnapshott.FromGameRunStatet(gsm.CurrentRun, sceneId); //Wave0write
            if (snapshot == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            string path = Path.Combine(Application.persistentDataPath, "save.json"); //Wave0write
            File.WriteAllText(path, JsonUtility.ToJson(snapshot, true)); //Wave0write
            Continue = snapshot; //Wave0write
        }

        /// <summary>
        /// Continue 데이터가 있는지 여부.
        /// </summary>
        public bool HasContinue()
        {
            // 동작 요약: Continue != null && !Continue.IsCompleted 반환.
            //TODO: return Continue != null && !Continue.IsCompleted;
            return Continue != null && !Continue.IsCompleted; //Wave0write
        }

        /// <summary>
        /// Continue 데이터 삭제(새 게임으로 덮어쓸 때).
        /// </summary>
        public void ClearContinue()
        {
            // 동작 요약:
            // - Continue = null.
            // - save.json 삭제 또는 빈 JSON 덮어쓰기.
            //TODO: Continue = null;
            //TODO: string path = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "save.json");
            //TODO: if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            Continue = null; //Wave0write
            string path = Path.Combine(Application.persistentDataPath, "save.json"); //Wave0write
            if (File.Exists(path)) //Wave0write
            { //Wave0write
                File.Delete(path); //Wave0write
            } //Wave0write
        }

        /// <summary>
        /// 묘비 기록 추가. 사망 시 GameSystemManagert가 호출.
        /// </summary>
        public void AppendGrave(string playerName, System.DateTime when)
        {
            // 동작 요약:
            // - Records.Graves에 새 항목 추가.
            // - records.json 저장.
            //TODO: Records.Graves.Add(new RecordEntryt { Name = playerName, TimestampIso = when.ToString("o") });
            //TODO: SaveRecords();
            EnsureRecords(); //Wave0write
            Records.Graves.Add(new RecordEntryt { Name = playerName, TimestampIso = when.ToString("o") }); //Wave0write
            SaveRecords(); //Wave0write
        }

        /// <summary>
        /// 비석(클리어) 기록 추가.
        /// </summary>
        public void AppendClearRecord(string playerName, System.DateTime when)
        {
            // 동작 요약:
            // - Records.Clears에 새 항목 추가.
            // - records.json 저장.
            //TODO: Records.Clears.Add(new RecordEntryt { Name = playerName, TimestampIso = when.ToString("o") });
            //TODO: SaveRecords();
            // SaveRecords 헬퍼:
            //TODO: // string json = JsonUtility.ToJson(Records); File.WriteAllText(recordPath, json);
            EnsureRecords(); //Wave0write
            Records.Clears.Add(new RecordEntryt { Name = playerName, TimestampIso = when.ToString("o") }); //Wave0write
            SaveRecords(); //Wave0write
        }

        private void EnsureRecords() //Wave0write
        { //Wave0write
            if (Records == null) //Wave0write
            { //Wave0write
                Records = new RecordBookt(); //Wave0write
            } //Wave0write

            if (Records.Clears == null) //Wave0write
            { //Wave0write
                Records.Clears = new List<RecordEntryt>(); //Wave0write
            } //Wave0write

            if (Records.Graves == null) //Wave0write
            { //Wave0write
                Records.Graves = new List<RecordEntryt>(); //Wave0write
            } //Wave0write
        } //Wave0write

        private void SaveRecords() //Wave0write
        { //Wave0write
            EnsureRecords(); //Wave0write
            string path = Path.Combine(Application.persistentDataPath, "records.json"); //Wave0write
            File.WriteAllText(path, JsonUtility.ToJson(Records, true)); //Wave0write
        } //Wave0write
    }

    /// <summary>비석/묘비 영구 기록.</summary>
    public sealed class RecordBookt
    {
        /// <summary>클리어 기록(비석).</summary>
        public List<RecordEntryt> Clears = new List<RecordEntryt>();

        /// <summary>사망 기록(묘비).</summary>
        public List<RecordEntryt> Graves = new List<RecordEntryt>();
    }

    /// <summary>한 줄의 기록.</summary>
    public sealed class RecordEntryt
    {
        /// <summary>이름.</summary>
        public string Name;

        /// <summary>일시.</summary>
        public string TimestampIso;
    }
}
