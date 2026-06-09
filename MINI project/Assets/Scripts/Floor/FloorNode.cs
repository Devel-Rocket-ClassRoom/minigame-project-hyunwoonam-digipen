using System.Collections.Generic;

/// <summary>
/// 한 노드. 전투 노드는 층 위치 + 난이도 + 몬스터 수 + 클리어 상태를 가진다.
/// 안전지대 노드는 IsSafeZone=true 이며 전투 진입 대상이 아니다.
/// </summary>
public sealed class FloorNode
{
    /// <summary>전역 고유 노드 ID.</summary>
    public int NodeId;

    /// <summary>층 번호(1~49).</summary>
    public int Floor;

    /// <summary>해당 단계(1~6).</summary>
    public int StageIndex;

    /// <summary>난이도(생성 시 고정).</summary>
    public int Difficulty;

    /// <summary>몬스터 수(1~3, 고정).</summary>
    public int MonsterCount;

    /// <summary>이 노드가 보스 노드인가.</summary>
    public bool IsBoss;

    /// <summary>이 노드가 안전지대 표시 노드인가.</summary>
    public bool IsSafeZone;

    /// <summary>이미 클리어됐는가.</summary>
    public bool IsCleared;

    /// <summary>다음 층의 연결 노드 ID 목록(랜덤 그래프).</summary>
    public List<int> NextNodeIds;
}

