using NUnit.Framework;

namespace Tempt
{
    /// <summary>
    /// 순수 밸런스/수식 회귀 테스트. 실제 Resources 테이블을 로드해 검증한다.
    /// Unity Test Runner(EditMode)에서 실행.
    /// </summary>
    public sealed class BalanceFormulaTests
    {
        private static DataManager LoadData()
        {
            var data = new DataManager();
            data.LoadAll();
            return data;
        }

        [Test]
        public void ComputeInflation_ZeroErosion_IsOne()
        {
            DataManager data = LoadData();
            Assert.AreEqual(1f, data.ComputeInflation(1, 0f), 0.0001f);
        }

        [Test]
        public void ComputeInflation_NonDecreasingInErosion()
        {
            DataManager data = LoadData();
            float low = data.ComputeInflation(1, 10f);
            float high = data.ComputeInflation(1, 90f);
            Assert.GreaterOrEqual(high, low);
            Assert.GreaterOrEqual(low, 1f);
        }

        [Test]
        public void ComputeInflation_ErosionClampedAt100()
        {
            DataManager data = LoadData();
            float at100 = data.ComputeInflation(1, 100f);
            float over = data.ComputeInflation(1, 250f);
            Assert.AreEqual(at100, over, 0.0001f, "침식률은 100%로 클램프되어야 한다");
        }

        [Test]
        public void RequiredExpForLevel_NonNegative_NonDecreasing_PositiveFromLevel1()
        {
            DataManager data = LoadData();
            int prev = 0;
            for (int level = 0; level < 5; level++)
            {
                int req = RunProgression.RequiredExpForLevel(data, level);
                Assert.GreaterOrEqual(req, 0, "레벨 " + level + " 요구 경험치 >= 0");
                Assert.GreaterOrEqual(req, prev, "요구 경험치는 비감소");
                prev = req;
            }
            // 레벨 0은 0(시작 레벨 1). 레벨 1 이상은 양수여야 진행 가능.
            Assert.Greater(RunProgression.RequiredExpForLevel(data, 1), 0);
        }

        [Test]
        public void ActionWeightTable_Default_IsExpected_AndFreshInstance()
        {
            ActionWeightTable a = ActionWeightTable.Default;
            ActionWeightTable b = ActionWeightTable.Default;
            Assert.AreEqual(80, a.Attack);
            Assert.AreEqual(0, a.Skill);
            Assert.AreEqual(20, a.Defend);
            Assert.AreNotSame(a, b, "호출마다 새 인스턴스여야 공유 변이 방지");
        }
    }
}
