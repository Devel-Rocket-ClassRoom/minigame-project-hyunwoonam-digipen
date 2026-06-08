export const ROOT = "F:/git/MINI";
export const ASSETS = {
  icon: `${ROOT}/MINI project/Assets/Imported/GameIcon.png`,
  combat: `${ROOT}/MINI project/Assets/Resources/CombatBackgrounds/BG_Combat_Stage01.png`,
  safe1: `${ROOT}/MINI project/Assets/Resources/safe/safe1.png`,
  floorMap: `${ROOT}/MINI project/Assets/Resources/tutorial/floormaptutorial.png`,
};

export const C = {
  ink: "#0B0D10",
  panel: "#151922",
  panel2: "#1C2330",
  line: "#303846",
  text: "#F4F1E8",
  muted: "#A7B0BE",
  gold: "#D5A74D",
  red: "#D84A42",
  cyan: "#67E8F9",
  violet: "#A78BFA",
  green: "#8BC34A",
};

export function bg(ctx, slide, color = C.ink) {
  ctx.addShape(slide, { x: 0, y: 0, w: 1280, h: 720, fill: color, line: ctx.line() });
}

export async function imageBg(ctx, slide, imagePath) {
  await ctx.addImage(slide, { path: imagePath, x: 0, y: 0, w: 1280, h: 720, fit: "cover" });
}

export function topKicker(ctx, slide, label, index) {
  ctx.addShape(slide, { x: 54, y: 40, w: 8, h: 24, fill: C.gold, line: ctx.line(), name: `kicker-${index}-marker` });
  ctx.addText(slide, {
    text: label,
    x: 76,
    y: 33,
    w: 560,
    h: 38,
    fontSize: 18,
    bold: true,
    color: C.gold,
    typeface: "Malgun Gothic",
    valign: "middle",
    name: `kicker-${index}-label`,
  });
}

export function title(ctx, slide, text, x = 54, y = 86, w = 780, h = 86, size = 40) {
  ctx.addText(slide, {
    text,
    x,
    y,
    w,
    h,
    fontSize: size,
    bold: true,
    color: C.text,
    typeface: "Malgun Gothic",
    insets: { left: 0, right: 0, top: 0, bottom: 0 },
  });
}

export function body(ctx, slide, text, x, y, w, h, size = 22, color = C.text) {
  ctx.addText(slide, {
    text,
    x,
    y,
    w,
    h,
    fontSize: size,
    color,
    typeface: "Malgun Gothic",
    insets: { left: 0, right: 0, top: 0, bottom: 0 },
  });
}

export function foot(ctx, slide, page, source = "Source: HANDOFF19, Build 3 report, local code/resources") {
  ctx.addShape(slide, { x: 54, y: 660, w: 1172, h: 1.2, fill: C.line, line: ctx.line() });
  ctx.addText(slide, { text: source, x: 54, y: 672, w: 780, h: 24, fontSize: 12, color: C.muted, typeface: "Malgun Gothic" });
  ctx.addText(slide, { text: String(page).padStart(2, "0"), x: 1180, y: 672, w: 46, h: 24, fontSize: 13, bold: true, color: C.gold, align: "right", typeface: "Malgun Gothic" });
}

export function card(ctx, slide, x, y, w, h, headline, detail, accent = C.gold) {
  ctx.addShape(slide, { x, y, w, h, fill: C.panel, line: { style: "solid", fill: C.line, width: 1 } });
  ctx.addShape(slide, { x, y, w: 5, h, fill: accent, line: ctx.line() });
  ctx.addText(slide, { text: headline, x: x + 20, y: y + 16, w: w - 40, h: 34, fontSize: 20, bold: true, color: C.text, typeface: "Malgun Gothic" });
  ctx.addText(slide, { text: detail, x: x + 20, y: y + 58, w: w - 40, h: h - 72, fontSize: 15.5, color: C.muted, typeface: "Malgun Gothic" });
}

export function metric(ctx, slide, x, y, w, h, value, label, note, accent = C.cyan) {
  ctx.addShape(slide, { x, y, w, h, fill: C.panel, line: { style: "solid", fill: C.line, width: 1 } });
  ctx.addText(slide, { text: value, x: x + 16, y: y + 13, w: w - 32, h: 48, fontSize: 34, bold: true, color: accent, typeface: "Malgun Gothic" });
  ctx.addText(slide, { text: label, x: x + 16, y: y + 62, w: w - 32, h: 28, fontSize: 16, bold: true, color: C.text, typeface: "Malgun Gothic" });
  ctx.addText(slide, { text: note, x: x + 16, y: y + 98, w: w - 32, h: h - 114, fontSize: 13.2, color: C.muted, typeface: "Malgun Gothic" });
}

export function node(ctx, slide, x, y, w, h, label, accent = C.gold) {
  ctx.addShape(slide, { x, y, w, h, fill: C.panel2, line: { style: "solid", fill: accent, width: 1.4 } });
  ctx.addText(slide, { text: label, x: x + 12, y: y + 10, w: w - 24, h: h - 20, fontSize: 16, bold: true, color: C.text, typeface: "Malgun Gothic", valign: "middle", align: "center" });
}

export function connector(ctx, slide, x1, y1, x2, y2, color = C.line) {
  const x = Math.min(x1, x2);
  const y = Math.min(y1, y2);
  const w = Math.max(1, Math.abs(x2 - x1));
  const h = Math.max(1, Math.abs(y2 - y1));
  if (h <= 1) {
    ctx.addShape(slide, { x, y, w, h: 2, fill: color, line: ctx.line() });
  } else if (w <= 1) {
    ctx.addShape(slide, { x, y, w: 2, h, fill: color, line: ctx.line() });
  } else {
    ctx.addShape(slide, { x, y, w, h: 2, fill: color, line: ctx.line() });
    ctx.addShape(slide, { x: x2 - 1, y, w: 2, h, fill: color, line: ctx.line() });
  }
}
