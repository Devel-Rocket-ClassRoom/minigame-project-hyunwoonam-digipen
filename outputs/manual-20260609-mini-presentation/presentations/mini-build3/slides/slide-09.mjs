import { ASSETS, C, foot } from "./_shared.mjs";

export async function slide09(presentation, ctx) {
  const slide = presentation.slides.add();
  await ctx.addImage(slide, { path: ASSETS.combat, x: 0, y: 0, w: 1280, h: 720, fit: "cover", alt: "Combat background" });
  ctx.addShape(slide, { x: 320, y: 205, w: 640, h: 220, fill: C.ink, line: { style: "solid", fill: C.gold, width: 2 } });
  ctx.addShape(slide, { x: 420, y: 252, w: 440, h: 3, fill: C.gold, line: ctx.line() });
  ctx.addText(slide, { text: "Q & A", x: 320, y: 282, w: 640, h: 90, fontSize: 68, bold: true, color: C.text, typeface: "Malgun Gothic", align: "center" });
  ctx.addText(slide, { text: "질문 받겠습니다", x: 320, y: 374, w: 640, h: 34, fontSize: 22, color: C.muted, typeface: "Malgun Gothic", align: "center" });
  foot(ctx, slide, 9, "Source: local combat background asset");
  return slide;
}
