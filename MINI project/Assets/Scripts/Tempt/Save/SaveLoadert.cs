using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// Continue 저장/이어하기 + 영구 기록(비석/묘비) 관리.
    /// 데이터 저장 형식은 JSON, 단일 슬롯.
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
        }

        /// <summary>
        /// 현재 런 상태 전체 스냅샷을 저장.
        /// </summary>
        public void SaveSnapshot()
        {
            // 동작 요약:
            // - GameSystemManagert.Instance.CurrentRun 전체를 SaveSnapshott로 직렬화.
            // - JSON 저장.
            // - Continue = snapshot 저장.
        }

        /// <summary>
        /// Continue 데이터가 있는지 여부.
        /// </summary>
        public bool HasContinue()
        {
            // 동작 요약: Continue != null && !Continue.IsCompleted 반환.
            return false;
        }

        /// <summary>
        /// Continue 데이터 삭제(새 게임으로 덮어쓸 때).
        /// </summary>
        public void ClearContinue()
        {
            // 동작 요약:
            // - Continue = null.
            // - save.json 삭제 또는 빈 JSON 덮어쓰기.
        }

        /// <summary>
        /// 묘비 기록 추가. 사망 시 GameSystemManagert가 호출.
        /// </summary>
        public void AppendGrave(string playerName, System.DateTime when)
        {
            // 동작 요약:
            // - Records.Graves에 새 항목 추가.
            // - records.json 저장.
        }

        /// <summary>
        /// 비석(클리어) 기록 추가.
        /// </summary>
        public void AppendClearRecord(string playerName, System.DateTime when)
        {
            // 동작 요약:
            // - Records.Clears에 새 항목 추가.
            // - records.json 저장.
        }
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
