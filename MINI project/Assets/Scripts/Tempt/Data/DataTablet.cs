namespace Tempt
{
    /// <summary>
    /// CSV에서 로드되는 정적 데이터 1행의 공통 베이스.
    /// 모든 데이터는 정수 ID를 1열로 가진다.
    /// </summary>
    public abstract class DataTablet
    {
        /// <summary>고유 ID(CSV 1열).</summary>
        public int Id;

        /// <summary>표시 이름(언어 변환 키).</summary>
        public string NameKey;

        /// <summary>설명 키.</summary>
        public string DescKey;

        /// <summary>
        /// CSV 한 행을 받아 필드를 채운다.
        /// </summary>
        /// <param name="cells">셀 배열.</param>
        public abstract void Parse(string[] cells);
    }
}
