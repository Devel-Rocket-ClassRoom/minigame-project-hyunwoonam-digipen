import { C, bg, foot, title, topKicker } from "./_shared.mjs";

function lessonCard(ctx, slide, x, y, w, h, headline, detail, accent) {
  ctx.addShape(slide, { x, y, w, h, fill: C.panel, line: { style: "solid", fill: C.line, width: 1 } });
  ctx.addShape(slide, { x, y, w: 5, h, fill: accent, line: ctx.line() });
  ctx.addText(slide, { text: headline, x: x + 20, y: y + 15, w: w - 40, h: 50, fontSize: 18.5, bold: true, color: C.text, typeface: "Malgun Gothic" });
  ctx.addText(slide, { text: detail, x: x + 20, y: y + 72, w: w - 40, h: h - 82, fontSize: 14.5, color: C.muted, typeface: "Malgun Gothic" });
}

export async function slide08(presentation, ctx) {
  const slide = presentation.slides.add();
  bg(ctx, slide);
  topKicker(ctx, slide, "LESSONS LEARNED", 8);
  title(ctx, slide, "느낀점: 완성도는 큰 아이디어보다 닫힌 루프에서 나왔습니다", 54, 84, 940, 86, 36);

  lessonCard(ctx, slide, 78, 218, 510, 122, "1. 스코프를 줄이는 것이 기능을 버리는 일은 아니었습니다", "100층 대규모 구상보다 49층/6단계와 Safe0~5로 압축하니 발표 가능한 루프가 생겼습니다.", C.gold);
  lessonCard(ctx, slide, 692, 218, 510, 122, "2. 데이터 권위가 생기니 수정 속도가 빨라졌습니다", "CSV/JSON과 DataValidator가 기준이 되면서 밸런스, 리소스 키, 몬스터/동료 구성이 코드 밖에서 보이기 시작했습니다.", C.cyan);
  lessonCard(ctx, slide, 78, 388, 510, 122, "3. Unity 검증은 마지막에 몰아서 할 수 없었습니다", "컴파일, EditMode, RunCommand, 리소스 로드 스모크를 반복해야 넓은 변경 범위를 감당할 수 있었습니다.", C.green);
  lessonCard(ctx, slide, 692, 388, 510, 122, "4. 폰트, 오디오, raw key 같은 작은 문제도 경험을 크게 바꿉니다", "전투 시스템만큼이나 한글 표시, SFX 볼륨, BGM 라우팅, Safe 배경이 최종 인상을 결정했습니다.", C.red);

  ctx.addShape(slide, { x: 210, y: 552, w: 860, h: 46, fill: C.panel2, line: { style: "solid", fill: C.line, width: 1 } });
  ctx.addText(slide, { text: "다음에 한다면: 초반부터 데이터 검증과 발표용 리소스 연결을 개발 일정 안에 넣겠습니다.", x: 230, y: 564, w: 820, h: 24, fontSize: 17, bold: true, color: C.text, typeface: "Malgun Gothic", align: "center" });
  foot(ctx, slide, 8, "Source: Build 3 report issues/lessons, HANDOFF19 residual risks");
  return slide;
}
