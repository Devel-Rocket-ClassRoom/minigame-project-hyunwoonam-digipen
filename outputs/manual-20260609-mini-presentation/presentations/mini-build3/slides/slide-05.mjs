import { ASSETS, C, bg, card, foot, title, topKicker } from "./_shared.mjs";

export async function slide05(presentation, ctx) {
  const slide = presentation.slides.add();
  bg(ctx, slide);
  topKicker(ctx, slide, "IMPLEMENTED SCOPE", 5);
  title(ctx, slide, "구현 범위는 플레이 루프 전체를 닫는 데 집중했습니다", 54, 84, 770, 88, 36);
  await ctx.addImage(slide, { path: ASSETS.safe1, x: 830, y: 86, w: 350, h: 214, fit: "cover", alt: "Safe1 facilities background" });
  ctx.addShape(slide, { x: 830, y: 300, w: 350, h: 2, fill: C.gold, line: ctx.line() });

  const cards = [
    ["데이터 권위", "Monster/Skill/Item/Drop/Rune/Companion/World/Balance를 파일 로딩으로 전환", C.cyan],
    ["저장/이어하기", "플레이어, 월드, 인벤토리, 장비, 동료, 룬 진행 상태 저장/복원", C.gold],
    ["보상/인벤토리", "전투 승리 보상, 드랍, 장착/해제, 폐기 모달, 슬롯 흐름", C.green],
    ["Safe1 시설", "Shop, Guild, Forge, Shrine, Tavern 기능 연결", C.violet],
    ["침식 시스템", "Safe2 활성화, 단계별 증가/감소, 잠금, 게임오버 경로", C.red],
    ["콘텐츠 데이터", "몬스터 24종, 동료 8명, 룬 48개, 직업군 기반 스킬 변형", C.cyan],
    ["Safe2~5", "성소와 광산, 누적 골드, Safe HUD와 이동 흐름", C.gold],
    ["전투 연출", "SPUM 애니, 이펙트 13종, 스킬/기본공격 SFX, BGM 라우팅", C.red],
    ["유지보수", "GameLog, partial 분리, UIEventPageBase, 폰트/raw key 수정", C.green],
  ];
  let idx = 0;
  for (let row = 0; row < 3; row += 1) {
    for (let col = 0; col < 3; col += 1) {
      const [h, d, a] = cards[idx++];
      card(ctx, slide, 56 + col * 250, 224 + row * 132, 220, 104, h, d, a);
    }
  }
  foot(ctx, slide, 5, "Source: Build 3 result report, HANDOFF19");
  return slide;
}
