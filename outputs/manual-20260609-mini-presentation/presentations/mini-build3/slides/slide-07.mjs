import { C, bg, foot, metric, title, topKicker } from "./_shared.mjs";

export async function slide07(presentation, ctx) {
  const slide = presentation.slides.add();
  bg(ctx, slide);
  topKicker(ctx, slide, "EVIDENCE", 7);
  title(ctx, slide, "완성 범위는 숫자와 검증 기록으로 확인했습니다", 54, 84, 820, 84, 38);

  metric(ctx, slide, 70, 214, 178, 150, "24", "몬스터", "능력치, 보상, 드랍, 행동 가중치 연결", C.red);
  metric(ctx, slide, 274, 214, 178, 150, "8", "동료", "직업군과 파티 편성, 상세 UI 연결", C.gold);
  metric(ctx, slide, 478, 214, 178, 150, "48", "룬", "룬 트리와 패시브 스킬 해금 흐름", C.violet);
  metric(ctx, slide, 682, 214, 178, 150, "13", "이펙트 리소스", "기본공격 + 스킬 이펙트 키 검증", C.cyan);
  metric(ctx, slide, 886, 214, 178, 150, "13", "SFX", "스킬 12종 + basicattack 연결", C.green);
  metric(ctx, slide, 1090, 214, 120, 150, "6", "Safe 배경", "Safe0~5 실제 이미지 적용", C.gold);

  metric(ctx, slide, 118, 412, 220, 138, "159", "C# scripts", "현재 Assets/Scripts 기준 로컬 카운트", C.cyan);
  metric(ctx, slide, 388, 412, 220, 138, "0/0", "컴파일 기록", "HANDOFF19/Build3 보고서의 Unity MCP 기록", C.green);
  metric(ctx, slide, 658, 412, 220, 138, "EditMode", "테스트 확대", "Equipment, Save, Rune, BGM, Options 등", C.violet);
  metric(ctx, slide, 928, 412, 220, 138, "Data", "로드 스모크", "Monster/Skill/Item/Rune/Companion/World 검증", C.gold);

  ctx.addShape(slide, { x: 146, y: 584, w: 990, h: 38, fill: C.panel, line: { style: "solid", fill: C.line, width: 1 } });
  ctx.addText(slide, { text: "주의: 현재 세션에서는 Unity를 다시 실행하지 않았고, 최신 문서의 검증 기록과 로컬 파일/리소스 검사를 근거로 정리했습니다.", x: 168, y: 594, w: 950, h: 20, fontSize: 13.8, color: C.muted, typeface: "Malgun Gothic", align: "center" });
  foot(ctx, slide, 7, "Source: HANDOFF19, Build 3 report, local resource/script counts");
  return slide;
}
