import { C, bg, body, card, connector, foot, node, title, topKicker } from "./_shared.mjs";

export async function slide02(presentation, ctx) {
  const slide = presentation.slides.add();
  bg(ctx, slide);
  topKicker(ctx, slide, "GAME INTRO", 2);
  title(ctx, slide, "침식 압박 속에서 탑을 오르는 턴제 로그라이크");
  body(ctx, slide, "플레이어는 안전지대에서 정비하고, 층을 선택해 전투를 치르고, 보상으로 다음 선택지를 넓힙니다.", 56, 174, 1060, 58, 21, C.muted);

  node(ctx, slide, 92, 310, 150, 64, "Safe\n정비/선택", C.gold);
  node(ctx, slide, 302, 310, 150, 64, "Floor Map\n층 선택", C.cyan);
  node(ctx, slide, 512, 310, 150, 64, "Combat\n턴제 전투", C.red);
  node(ctx, slide, 722, 310, 150, 64, "Reward\n보상/드랍", C.green);
  node(ctx, slide, 932, 310, 150, 64, "Growth\n룬/장비/동료", C.violet);
  connector(ctx, slide, 242, 342, 302, 342, C.gold);
  connector(ctx, slide, 452, 342, 512, 342, C.cyan);
  connector(ctx, slide, 662, 342, 722, 342, C.red);
  connector(ctx, slide, 872, 342, 932, 342, C.green);
  connector(ctx, slide, 1007, 374, 1007, 450, C.violet);
  connector(ctx, slide, 167, 450, 1007, 450, C.violet);
  connector(ctx, slide, 167, 374, 167, 450, C.violet);

  card(ctx, slide, 72, 510, 340, 96, "핵심 정체성", "다크 판타지, 영구 진행 압박, 안전지대와 전투 사이의 반복 선택", C.gold);
  card(ctx, slide, 470, 510, 340, 96, "플레이 목표", "6단계와 49층을 넘어 최종 컷씬까지 도달하는 압축형 탑 등반", C.cyan);
  card(ctx, slide, 868, 510, 340, 96, "Build3 변화", "fallback 중심 프로토타입에서 실제 데이터와 리소스 기반 루프로 전환", C.red);
  foot(ctx, slide, 2, "Source: Build 3 build notes, Game identity docs");
  return slide;
}
