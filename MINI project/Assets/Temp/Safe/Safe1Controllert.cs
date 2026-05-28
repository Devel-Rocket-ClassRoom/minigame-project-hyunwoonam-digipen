namespace Tempt
{
    /// <summary>
    /// 안전지대 1: 주점/상점/길드/대장간/신전. 튜토리얼 실행.
    /// </summary>
    public sealed class Safe1Controllert : SafeZoneControllerBaset
    {
        /// <summary>주점.</summary>
        public Innt Inn;

        /// <summary>상점.</summary>
        public Shopt Shop;

        /// <summary>길드.</summary>
        public Guildt Guild;

        /// <summary>대장간.</summary>
        public Forget Forge;

        /// <summary>신전.</summary>
        public Templet Temple;

        /// <summary>튜토리얼 컨트롤러.</summary>
        public TutorialControllert Tutorial;

        /// <inheritdoc/>
        protected override void SetupZoneFeatures()
        {
            // 동작 요약:
            // - 5개 시설 활성화.
            // - 튜토리얼 미완료(첫 진입)이면 TutorialControllert.StartIfNotCompleted("Safe1_FirstVisit").
            // - 각 시설 진입은 인터랙션(아이콘) 클릭 → 해당 모듈의 Open() 호출.
            //TODO: InnInteract.onClick.AddListener(Inn.Open);
            //TODO: ShopInteract.onClick.AddListener(Shop.Open);
            //TODO: GuildInteract.onClick.AddListener(Guild.Open);
            //TODO: ForgeInteract.onClick.AddListener(Forge.Open);
            //TODO: TempleInteract.onClick.AddListener(Temple.Open);
            //TODO: Tutorial.StartIfNotCompleted("Safe1_FirstVisit");
        }
    }
}
