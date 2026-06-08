import { ASSETS, C, bg, body, foot, title, topKicker } from "./_shared.mjs";

export async function slide03(presentation, ctx) {
  const slide = presentation.slides.add();
  bg(ctx, slide);
  topKicker(ctx, slide, "PROGRESSION", 3);
  title(ctx, slide, "49층 / 6단계 / Safe0~5 구조로 진행을 읽히게 만들었습니다", 54, 84, 760, 92, 36);
  await ctx.addImage(slide, { path: ASSETS.floorMap, x: 54, y: 205, w: 750, h: 414, fit: "contain", alt: "Floor map tutorial" });
  ctx.addShape(slide, { x: 850, y: 198, w: 334, h: 136, fill: C.panel, line: { style: "solid", fill: C.line, width: 1 } });
  ctx.addText(slide, { text: "진행 규칙", x: 874, y: 224, w: 280, h: 30, fontSize: 22, bold: true, color: C.gold, typeface: "Malgun Gothic" });
  body(ctx, slide, "- 현재 위치에서 다음 층만 선택\n- 보스 층 클리어 시 다음 Safe 해금\n- 침식률 100% 단계는 잠금", 874, 270, 275, 48, 15.2, C.text);
  ctx.addShape(slide, { x: 850, y: 362, w: 334, h: 128, fill: C.panel, line: { style: "solid", fill: C.line, width: 1 } });
  ctx.addText(slide, { text: "침식 압박", x: 874, y: 384, w: 280, h: 30, fontSize: 22, bold: true, color: C.red, typeface: "Malgun Gothic" });
  body(ctx, slide, "시간과 노드 진입이 누적되면서\n전투 난도와 동선 선택에 긴장감을 줍니다.", 874, 426, 275, 42, 15.2, C.text);
  ctx.addShape(slide, { x: 850, y: 518, w: 334, h: 88, fill: C.panel, line: { style: "solid", fill: C.line, width: 1 } });
  ctx.addText(slide, { text: "왜 중요한가", x: 874, y: 532, w: 280, h: 24, fontSize: 18, bold: true, color: C.cyan, typeface: "Malgun Gothic" });
  body(ctx, slide, "단순 전투 반복이 아니라,\n돌아갈지 밀고 갈지를 계속 판단하게 합니다.", 874, 562, 275, 28, 13.5, C.muted);
  foot(ctx, slide, 3, "Source: floormaptutorial.png, Build 3 build notes");
  return slide;
}
