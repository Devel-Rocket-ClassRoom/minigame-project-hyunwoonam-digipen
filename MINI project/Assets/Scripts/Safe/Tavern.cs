namespace Tempt
{
    /// <summary>
    /// 주점 동료 모집 도메인. 모집 후 CompanionRosterState.Bench 에 추가한다.
    /// </summary>
    public static class Tavern
    {
        /// <summary>이미 명부에 있는지(Active 또는 Bench). 중복 모집 방지.</summary>
        public static bool IsAlreadyRecruited(int companionId, CompanionRosterState roster)
        {
            if (roster == null)
            {
                return false;
            }

            if (roster.Active != null)
            {
                for (int i = 0; i < roster.Active.Count; i++)
                {
                    if (roster.Active[i] != null && roster.Active[i].CompanionDataId == companionId)
                    {
                        return true;
                    }
                }
            }

            if (roster.Bench != null)
            {
                for (int i = 0; i < roster.Bench.Count; i++)
                {
                    if (roster.Bench[i] != null && roster.Bench[i].CompanionDataId == companionId)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>현재 모집 가격. 층/침식 인플레이션 적용. 데이터 누락 시 0.</summary>
        public static int GetRecruitPrice(int companionId, GameRunState run, DataManager data)
        {
            if (!TryResolveInputs(companionId, run, data, out CompanionData companion))
            {
                return 0;
            }

            if (companion.RecruitPrice <= 0)
            {
                return 0;
            }

            int stageIndex = StageIndexResolver.FromFloor(run.CurrentFloor, data.World);
            float erosionRate = run.Erosion != null ? run.Erosion.GetRate(stageIndex) : 0f;
            float inflation = data.ComputeInflation(stageIndex, erosionRate);
            return UnityEngine.Mathf.Max(
                1,
                UnityEngine.Mathf.RoundToInt(companion.RecruitPrice * inflation)
            );
        }

        /// <summary>모집 가능 여부 확인. UI 버튼 활성 표시용.</summary>
        public static bool CanRecruit(int companionId, GameRunState run, DataManager data)
        {
            if (!TryResolveInputs(companionId, run, data, out CompanionData companion))
            {
                return false;
            }

            if (run.CurrentFloor < companion.RequiredFloor)
            {
                return false;
            }

            if (run.Roster != null && IsAlreadyRecruited(companionId, run.Roster))
            {
                return false;
            }

            int price = GetRecruitPrice(companionId, run, data);
            return run.Gold >= price;
        }

        /// <summary>
        /// 동료 모집. 성공 시 Bench 에 추가, 골드 차감, 이벤트 발행, 저장.
        /// 실패 시 변동 없음.
        /// </summary>
        public static bool TryRecruit(int companionId, GameRunState run, DataManager data)
        {
            if (!TryResolveInputs(companionId, run, data, out CompanionData companion))
            {
                return false;
            }

            if (run.CurrentFloor < companion.RequiredFloor)
            {
                return false;
            }

            if (run.Roster == null)
            {
                run.Roster = new CompanionRosterState();
            }

            if (IsAlreadyRecruited(companionId, run.Roster))
            {
                return false;
            }

            int price = GetRecruitPrice(companionId, run, data);
            if (run.Gold < price)
            {
                return false;
            }

            run.Gold -= price;

            var instance = new CompanionInstance
            {
                CompanionDataId = companionId,
                Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue),
                Level = 1,
                Exp = 0,
            };

            run.Roster.Recruit(instance);
            RaiseChanged(companionId, run);
            return true;
        }

        private static bool TryResolveInputs(
            int companionId,
            GameRunState run,
            DataManager data,
            out CompanionData companion
        )
        {
            companion = null;
            if (run?.Player == null)
            {
                UnityEngine.Debug.LogError("[Tavern] run / Player 참조가 없습니다.");
                return false;
            }

            if (data?.Companions == null)
            {
                UnityEngine.Debug.LogError("[Tavern] DataManager.Companions 참조가 없습니다.");
                return false;
            }

            if (!data.Companions.TryGetValue(companionId, out companion) || companion == null)
            {
                UnityEngine.Debug.LogError("[Tavern] 동료 ID 없음: " + companionId);
                return false;
            }

            return true;
        }

        private static void RaiseChanged(int companionId, GameRunState run)
        {
            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                return;
            }

            gsm.Events?.RaiseGoldChanged(run != null ? run.Gold : 0);
            gsm.Events?.RaiseRosterChanged(companionId, true);
            gsm.Save?.SaveSnapshot();
        }
    }
}
