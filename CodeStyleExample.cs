// =============================================================================
// CodeStyleExample.cs  —  포맷(서식) 규칙 참고용 예제 파일
// =============================================================================
//
// 목적
//   - "코드 내용"이 아니라 "서식 규칙"(띄어쓰기/줄바꿈/중괄호 등)만 보여주는 예제다.
//   - 이 파일을 본인 스타일대로 고치면, AI가 이 파일을 기준 삼아 Assets/Scripts
//     전체 코드의 "서식"을 통일한다. 로직/식별자/주석 내용은 바꾸지 않는다.
//   - 이 파일은 Assets 폴더 밖(레포 루트)이라 컴파일되지 않는다. 자유롭게 편집 가능.
//
// 적용 범위
//   - 적용 대상: Assets/Scripts/ 아래 직접 작성한 .cs
//   - 적용 제외: Assets/Imported/ (외부 에셋: SPUM, Epic Toon FX, Hovl 등)
//
// 네임스페이스 방향 (요청 반영)
//   - [규칙 16] namespace Tempt 를 없애고 전역(top-level)으로 둔다.
//   - ※ 주의(블로커): 전역으로 펼치면 Tempt.PlayerState 와 SPUM 의 전역 enum
//     PlayerState 가 충돌한다(CS0104/CS0433). 이 충돌을 먼저 해소해야 전체 적용 가능.
//     (해소안은 채팅 참고: 둘 중 하나 rename / Tempt 유지 / 어셈블리 격리)
// =============================================================================


// -----------------------------------------------------------------------------
// 규칙 요약 체크리스트
// -----------------------------------------------------------------------------
//   [규칙 1]  들여쓰기            = 공백 4칸 (탭 금지)
//   [규칙 2]  중괄호 위치          = 새 줄(Allman). 여는 { 는 항상 다음 줄
//   [규칙 3]  제어문 중괄호 강제    = if/else/for/foreach/while/do 는 한 줄짜리라도 { } 필수
//   [규칙 4]  한 줄 한 문장        = 한 줄에 여러 문장(;) 금지
//   [규칙 5]  연산자 주변 공백      = a + b, x == y  (양쪽 공백 1칸)
//   [규칙 6]  쉼표/세미콜론 뒤 공백  = f(a, b);  for (i = 0; i < n; i++)
//   [규칙 7]  키워드 뒤 공백        = if (...) / for (...) / while (...)  (괄호 앞 공백 1칸)
//   [규칙 8]  메서드 호출/이름 뒤    = Foo(...)  (이름과 ( 사이 공백 없음)
//   [규칙 9]  괄호 안쪽 공백        = ( x ) 아님 → (x)  (안쪽 공백 없음)
//   [규칙 10] 빈 줄                = 멤버/메서드 사이 빈 줄 1줄, 연속 빈 줄 2줄 이상 금지
//   [규칙 11] using 정렬          = 파일 맨 위, System.* 먼저 그룹, 나머지 알파벳 순
//   [규칙 12] switch 들여쓰기      = case 는 switch 보다 한 단계 안, 본문은 case 보다 한 단계 안
//   [규칙 13] 줄 끝 공백 제거 / 파일 끝 개행 1개
//   [규칙 14] 긴 인자 목록 줄바꿈   = 한 줄에 안 들어가면 인자당 한 줄, 닫는 ) 는 새 줄
//   [규칙 15] 어트리뷰트는 줄 바꿈   = [SerializeField] 등은 멤버 위 별도 줄에
//   [규칙 16] 네임스페이스          = namespace 로 감싸지 않는다(전역)
// -----------------------------------------------------------------------------


// [규칙 11] using: System.* 그룹 먼저, 빈 줄, 그 다음 나머지 알파벳 순.
using System;
using System.Collections.Generic;

using UnityEngine;


// [규칙 16] namespace 없이 전역에 둔다.
/// <summary>
/// 서식 규칙을 한눈에 보여주는 예제 클래스. (내용은 의미 없음, 모양만 본다)
/// </summary>
public sealed class CodeStyleExample : MonoBehaviour
{
    // [규칙 15] 어트리뷰트는 멤버 선언과 같은 줄에 쓰지 않고 윗줄에 따로 둔다.
    [SerializeField]
    private int serializedValue;

    private const int MaxRetry = 3;
    private readonly List<int> values = new List<int>();

    // [규칙 10] 멤버 사이 빈 줄 1줄.
    // [규칙 7] if/for/while 같은 키워드와 괄호 사이 공백 1칸: if (
    // [규칙 8] 메서드 이름과 여는 괄호 사이 공백 없음: DoWork(
    public void DoWork(int count)
    {
        // [규칙 3] 한 줄짜리 조건이라도 중괄호 필수.
        if (count <= 0)
        {
            return;
        }

        // [규칙 3] 피해야 하는 형태(중괄호 생략) — 이렇게 쓰지 않는다:
        //     if (count <= 0)
        //         return;
        //     if (count <= 0) return;

        // [규칙 5][규칙 6] 연산자 공백 / 세미콜론 뒤 공백.
        // [규칙 9] 괄호 안쪽 공백 없음.
        for (int i = 0; i < count; i++)
        {
            // [규칙 4] 한 줄에 한 문장만.
            int doubled = i * 2;
            values.Add(doubled);
        }

        // [규칙 2][규칙 3] else 도 중괄호. else 는 닫는 } 다음 줄에.
        if (count > MaxRetry)
        {
            Log("재시도 횟수 초과");
        }
        else
        {
            Log("정상 범위");
        }
    }

    /// <summary>
    /// [규칙 12] switch 들여쓰기 예시.
    /// </summary>
    public string Describe(StyleKind kind)
    {
        switch (kind)
        {
            case StyleKind.Compact:
                return "compact";
            case StyleKind.Normal:
                return "normal";
            default:
                return "unknown";
        }
    }

    /// <summary>
    /// [규칙 14] 인자 목록이 길어 한 줄을 넘으면 인자당 한 줄로 나누고,
    ///          닫는 괄호 ) 는 마지막 인자 다음 새 줄에 둔다.
    /// </summary>
    public void CallWithManyArgs()
    {
        // 짧으면 한 줄.
        Configure(1, 2, 3);

        // 길면 인자당 한 줄.
        Configure(
            firstValueThatIsQuiteLong: 100,
            secondValueThatIsQuiteLong: 200,
            thirdValueThatIsQuiteLong: 300
        );
    }

    // [규칙 8][규칙 9] 호출부: 이름 뒤 공백 없음, 괄호 안쪽 공백 없음.
    private void Configure(int a, int b, int c)
    {
        Log($"{a},{b},{c}");
    }

    private void Configure(
        int firstValueThatIsQuiteLong,
        int secondValueThatIsQuiteLong,
        int thirdValueThatIsQuiteLong)
    {
        // 본문 생략.
    }

    private void Log(string message)
    {
        Debug.Log(message);
    }
}


// [규칙 2] enum 도 여는 중괄호 새 줄. [규칙 16] 전역.
public enum StyleKind
{
    Compact,
    Normal,
}
// [규칙 13] 파일은 개행 1개로 끝난다.
