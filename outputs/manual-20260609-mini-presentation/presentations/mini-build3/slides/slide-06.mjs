import { C, bg, connector, foot, node, title, topKicker } from "./_shared.mjs";

export async function slide06(presentation, ctx) {
  const slide = presentation.slides.add();
  bg(ctx, slide);
  topKicker(ctx, slide, "TECHNICAL STRUCTURE", 6);
  title(ctx, slide, "구조는 데이터, 런 상태, 씬 흐름, 전투, UI로 나뉩니다", 54, 84, 820, 88, 36);

  node(ctx, slide, 84, 230, 210, 72, "Resources/Tables\nCSV · JSON", C.gold);
  node(ctx, slide, 374, 230, 210, 72, "DataManager\nDataValidator", C.cyan);
  node(ctx, slide, 664, 230, 210, 72, "GameRunState\nSaveSnapshot", C.violet);
  node(ctx, slide, 954, 230, 210, 72, "Scene Flow\nSafe / Floor / Combat", C.gold);
  connector(ctx, slide, 294, 266, 374, 266, C.gold);
  connector(ctx, slide, 584, 266, 664, 266, C.cyan);
  connector(ctx, slide, 874, 266, 954, 266, C.violet);

  node(ctx, slide, 188, 420, 220, 76, "CombatFlow\nAction Selectors", C.red);
  node(ctx, slide, 530, 420, 220, 76, "Rune / Skill\nRuntime Resolver", C.cyan);
  node(ctx, slide, 872, 420, 220, 76, "UI Controllers\nSafe1 Facilities", C.green);
  connector(ctx, slide, 1058, 302, 1058, 380, C.gold);
  connector(ctx, slide, 1058, 380, 982, 420, C.gold);
  connector(ctx, slide, 770, 302, 640, 420, C.violet);
  connector(ctx, slide, 770, 302, 298, 420, C.violet);

  ctx.addShape(slide, { x: 92, y: 552, w: 1090, h: 54, fill: C.panel, line: { style: "solid", fill: C.line, width: 1 } });
  ctx.addText(slide, { text: "핵심 판단: 하드코딩을 줄이되, 세이브/씬/프리팹 회귀 위험이 큰 변경은 Build3 범위 밖으로 남겼습니다.", x: 118, y: 566, w: 1040, h: 30, fontSize: 19, bold: true, color: C.text, typeface: "Malgun Gothic", valign: "middle" });
  foot(ctx, slide, 6, "Source: Assets/Scripts scan, DataManager, GameSystemManager, CombatFlow, SaveSnapshot");
  return slide;
}
