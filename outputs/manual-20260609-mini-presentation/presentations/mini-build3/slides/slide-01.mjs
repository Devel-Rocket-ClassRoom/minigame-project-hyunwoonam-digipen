import { ASSETS, C, foot, imageBg } from "./_shared.mjs";

export async function slide01(presentation, ctx) {
  const slide = presentation.slides.add();
  await imageBg(ctx, slide, ASSETS.combat);
  ctx.addShape(slide, { x: 0, y: 0, w: 710, h: 720, fill: "#0B0D10", line: ctx.line() });
  ctx.addShape(slide, { x: 54, y: 62, w: 126, h: 2.5, fill: C.gold, line: ctx.line() });
  ctx.addText(slide, { text: "MiniProject 발표", x: 54, y: 78, w: 500, h: 36, fontSize: 18, bold: true, color: C.gold, typeface: "Malgun Gothic" });
  ctx.addText(slide, { text: "THE TOWER", x: 52, y: 132, w: 600, h: 84, fontSize: 56, bold: true, color: C.text, typeface: "Malgun Gothic" });
  ctx.addText(slide, { text: "Build 3에서 완성한 데이터 기반\n다크 판타지 턴제 로그라이크", x: 56, y: 224, w: 560, h: 98, fontSize: 28, bold: true, color: C.text, typeface: "Malgun Gothic" });
  ctx.addText(slide, { text: "Unity 6.3 LTS · Windows PC · v0.3.0 Build 3", x: 56, y: 352, w: 560, h: 30, fontSize: 18, color: C.muted, typeface: "Malgun Gothic" });
  ctx.addShape(slide, { x: 56, y: 430, w: 110, h: 54, fill: C.panel2, line: { style: "solid", fill: C.line, width: 1 } });
  ctx.addText(slide, { text: "49층", x: 70, y: 440, w: 82, h: 24, fontSize: 22, bold: true, color: C.cyan, typeface: "Malgun Gothic", align: "center" });
  ctx.addText(slide, { text: "6단계", x: 70, y: 466, w: 82, h: 18, fontSize: 12, color: C.muted, typeface: "Malgun Gothic", align: "center" });
  ctx.addShape(slide, { x: 182, y: 430, w: 110, h: 54, fill: C.panel2, line: { style: "solid", fill: C.line, width: 1 } });
  ctx.addText(slide, { text: "Safe0~5", x: 196, y: 440, w: 82, h: 24, fontSize: 18, bold: true, color: C.gold, typeface: "Malgun Gothic", align: "center" });
  ctx.addText(slide, { text: "거점 구조", x: 196, y: 466, w: 82, h: 18, fontSize: 12, color: C.muted, typeface: "Malgun Gothic", align: "center" });
  await ctx.addImage(slide, { path: ASSETS.icon, x: 815, y: 102, w: 330, h: 330, fit: "contain", alt: "The Tower game icon" });
  foot(ctx, slide, 1, "Source: local project assets, Build 3 build notes");
  return slide;
}
