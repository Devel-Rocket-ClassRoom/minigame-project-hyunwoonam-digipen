using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Continue 저장/이어하기 + 영구 기록(비석/묘비) 관리.
/// 데이터 저장 형식은 JSON, 단일 슬롯.
/// Continue JSON은 플로어 맵 전체 노드 구조와 다른 런 진행 상태를 하나의 SaveSnapshott로 보관한다.
/// </summary>
public sealed class SaveLoader
{
    /// <summary>현재 저장된 Continue 데이터(없으면 null).</summary>
    public SaveSnapshot Continue { get; private set; }

    /// <summary>영구 기록(비석 = 클리어, 묘비 = 사망).</summary>
    public RecordBook Records { get; private set; }

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
        // - 실제 FloorMapModel 복원은 GameSystemManager.ContinueGame() 또는 별도 복원 헬퍼가 담당.
        string savePath = Path.Combine(Application.persistentDataPath, "save.json");
        string recordPath = Path.Combine(Application.persistentDataPath, "records.json");
        Continue = File.Exists(savePath) ? JsonUtility.FromJson<SaveSnapshot>(File.ReadAllText(savePath)) : null;
        Records = File.Exists(recordPath) ? JsonUtility.FromJson<RecordBook>(File.ReadAllText(recordPath)) : new RecordBook();

        if (Records == null)
        {
            Records = new RecordBook();
        }
    }

    /// <summary>
    /// 현재 런 상태 전체 스냅샷을 저장.
    /// </summary>
    public void SaveSnapshot()
    {
        // 동작 요약:
        // - GameSystemManager.Instance.CurrentRun 전체를 SaveSnapshott로 직렬화.
        // - CurrentRun.FloorMap.NodesById/NodesByFloor의 모든 FloorNodet을 FloorNodeSnapshott로 변환.
        // - FloorMapSnapshot.NextSelectableFloor와 Nodes를 함께 저장.
        // - seed 저장/seed 기반 재생성은 사용하지 않음.
        // - JSON 저장.
        // - Continue = snapshot 저장.
        if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm) || gsm.CurrentRun == null)
        {
            return;
        }

        SceneId sceneId = gsm.Scenes != null ? gsm.Scenes.CurrentSceneId : SceneId.MainMenu;

        // 전투는 중간 상태를 저장하지 않는 일시 씬이므로 이어하기 위치로 기록하지 않는다.
        // (전투 종료/전투 중 종료 저장은 CurrentSceneId 가 아직 Combat 인 시점에 호출됨)
        // 전투의 자연 복귀 지점인 FloorMap 으로 정규화한다.
        if (sceneId == SceneId.Combat)
        {
            sceneId = SceneId.FloorMap;
        }

        SaveSnapshot snapshot = SaveSnapshot.FromGameRunStatet(gsm.CurrentRun, sceneId);

        if (snapshot == null)
        {
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, "save.json");
        File.WriteAllText(path, JsonUtility.ToJson(snapshot, true));
        Continue = snapshot;
    }

    /// <summary>
    /// Continue 데이터가 있는지 여부.
    /// </summary>
    public bool HasContinue()
    {
        // 동작 요약: Continue != null && !Continue.IsCompleted 반환.
        return Continue != null && !Continue.IsCompleted;
    }

    /// <summary>
    /// Continue 데이터 삭제(새 게임으로 덮어쓸 때).
    /// </summary>
    public void ClearContinue()
    {
        // 동작 요약:
        // - Continue = null.
        // - save.json 삭제 또는 빈 JSON 덮어쓰기.
        Continue = null;
        string path = Path.Combine(Application.persistentDataPath, "save.json");

        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// 묘비 기록 추가. 사망 시 GameSystemManagert가 호출.
    /// </summary>
    public void AppendGrave(string playerName, System.DateTime when)
    {
        // 동작 요약:
        // - Records.Graves에 새 항목 추가.
        // - records.json 저장.
        EnsureRecords();
        Records.Graves.Add(new RecordEntry { Name = playerName, TimestampIso = when.ToString("o") });
        SaveRecords();
    }

    /// <summary>
    /// 비석(클리어) 기록 추가. 최종 승리 시점의 플레이 가능한 스냅샷을 함께 보관한다.
    /// </summary>
    public void AppendClearRecord(string playerName, System.DateTime when, SaveSnapshot snapshot)
    {
        // 동작 요약:
        // - Records.Clears에 이름 + 일시 + 승리 직후 스냅샷을 추가.
        // - snapshot이 있으면 Record 화면에서 언제든 다시 플레이 가능.
        // - records.json 저장(save.json/Continue 와 무관, 영구 보존).
        EnsureRecords();
        Records.Clears.Add(
            new ClearRecord
            {
                Name = playerName,
                TimestampIso = when.ToString("o"),
                Snapshot = snapshot,
            }
        );
        SaveRecords();
    }

    private void EnsureRecords()
    {
        if (Records == null)
        {
            Records = new RecordBook();
        }

        if (Records.Clears == null)
        {
            Records.Clears = new List<ClearRecord>();
        }

        if (Records.Graves == null)
        {
            Records.Graves = new List<RecordEntry>();
        }
    }

    private void SaveRecords()
    {
        EnsureRecords();
        string path = Path.Combine(Application.persistentDataPath, "records.json");
        File.WriteAllText(path, JsonUtility.ToJson(Records, true));
    }
}

/// <summary>비석/묘비 영구 기록.</summary>
[System.Serializable]
public sealed class RecordBook
{
    /// <summary>클리어 기록(비석). 각 항목은 승리 직후 스냅샷을 포함해 다시 플레이 가능.</summary>
    public List<ClearRecord> Clears = new List<ClearRecord>();

    /// <summary>사망 기록(묘비).</summary>
    public List<RecordEntry> Graves = new List<RecordEntry>();
}

/// <summary>묘비 한 줄의 기록(사망).</summary>
[System.Serializable]
public sealed class RecordEntry
{
    /// <summary>이름.</summary>
    public string Name;

    /// <summary>일시.</summary>
    public string TimestampIso;
}

/// <summary>비석 한 줄의 기록(클리어). 승리 직후 플레이 가능한 스냅샷 포함.</summary>
[System.Serializable]
public sealed class ClearRecord
{
    /// <summary>이름.</summary>
    public string Name;

    /// <summary>일시.</summary>
    public string TimestampIso;

    /// <summary>승리 직후 스냅샷(safe0, 전 시스템 해금, 침식 정지). 구버전 기록은 null/빈 값.</summary>
    public SaveSnapshot Snapshot;

    /// <summary>스냅샷이 있어 다시 플레이 가능한 기록인지 여부.</summary>
    public bool IsPlayable =>
        Snapshot != null && !string.IsNullOrEmpty(Snapshot.SavedAtIso);
}

