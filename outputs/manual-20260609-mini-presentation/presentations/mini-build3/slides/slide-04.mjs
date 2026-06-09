import { C, bg, body, card, foot, title, topKicker } from "./_shared.mjs";

export async function slide04(presentation, ctx) {
  const slide = presentation.slides.add();
  bg(ctx, slide);
  topKicker(ctx, slide, "SCHEDULE", 4);
  title(ctx, slide, "개발은 수직 슬라이스에서 최종 제출 품질 보강까지 진행됐습니다", 54, 86, 930, 96, 36);
  body(ctx, slide, "문서상 Week1~3 계획과 Build3 보고서를 합쳐 보면, 기능을 넓히기보다 플레이 루프를 실제 데이터와 검증으로 고정하는 방향이었습니다.", 56, 192, 1080, 50, 18, C.muted);

  ctx.addShape(slide, { x: 116, y: 306, w: 1000, h: 4, fill: C.line, line: ctx.line() });
  const xs = [170, 500, 830];
  const labels = ["Week 1", "Week 2", "Week 3 / Build 3"];
  const details = [
    "전체 사이클 축소판\nBoot, Safe, Floor, Combat 연결",
    "시스템 확장\n데이터/저장/보상/시설 기반",
    "최종 제출 안정화\n연출, 오디오, 폰트, 테스트 보강",
  ];
  const accents = [C.gold, C.cyan, C.red];
  for (let i = 0; i < 3; i += 1) {
    ctx.addShape(slide, { x: xs[i] - 16, y: 292, w: 32, h: 32, geometry: "ellipse", fill: accents[i], line: ctx.line() });
    ctx.addText(slide, { text: labels[i], x: xs[i] - 95, y: 344, w: 190, h: 30, fontSize: 22, bold: true, color: C.text, typeface: "Malgun Gothic", align: "center" });
    body(ctx, slide, details[i], xs[i] - 130, 390, 260, 72, 17, C.muted);
  }

  card(ctx, slide, 78, 520, 340, 88, "계획의 핵심", "작은 완성 루프를 먼저 만들고, 다음 주차에서 실제 시스템으로 교체", C.gold);
  card(ctx, slide, 470, 520, 340, 88, "진행의 핵심", "CSV/JSON, SaveSnapshot, 시설 UI, 전투 선택이 서로 연결되도록 조정", C.cyan);
  card(ctx, slide, 862, 520, 340, 88, "마무리의 핵심", "기능 추가보다 검증, 리소스 키, 폰트, 오디오, 유지보수성을 정리", C.red);
  foot(ctx, slide, 4, "Source: Production Week docs, Roadmap - Week3, Build 3 report");
  return slide;
}
