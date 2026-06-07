using System.Collections.Generic;

namespace Tempt
{
    public sealed partial class SaveSnapshot
    {
        private static RosterSnapshot FromRoster(CompanionRosterState roster)
        {
            var snapshot = new RosterSnapshot
            {
                Active = new List<CompanionSnapshot>(),
                Bench = new List<CompanionSnapshot>(),
            };
            if (roster == null)
            {
                return snapshot;
            }

            foreach (CompanionInstance inst in roster.Active)
                snapshot.Active.Add(FromCompanion(inst));
            foreach (CompanionInstance inst in roster.Bench)
                snapshot.Bench.Add(FromCompanion(inst));
            return snapshot;
        }

        private static CompanionRosterState ToRoster(RosterSnapshot snapshot)
        {
            var roster = new CompanionRosterState();
            if (snapshot == null)
            {
                return roster;
            }

            if (snapshot.Active != null)
                foreach (CompanionSnapshot src in snapshot.Active)
                    roster.Active.Add(ToCompanion(src));
            if (snapshot.Bench != null)
                foreach (CompanionSnapshot src in snapshot.Bench)
                    roster.Bench.Add(ToCompanion(src));
            return roster;
        }

        private static CompanionSnapshot FromCompanion(CompanionInstance inst)
        {
            return new CompanionSnapshot
            {
                CompanionId = inst.CompanionDataId,
                Seed = inst.Seed,
                Level = inst.Level,
                Exp = inst.Exp,
                FixedRuneSequence =
                    inst.Rune?.FixedSequence != null
                        ? new List<int>(inst.Rune.FixedSequence)
                        : new List<int>(),
                UnlockedCount = inst.Rune != null ? inst.Rune.UnlockedCount : 0,
                Stats = FromStats(inst.Stats),
                Equipment = FromEquipment(inst.Equipment),
            };
        }

        private static CompanionInstance ToCompanion(CompanionSnapshot src)
        {
            return new CompanionInstance
            {
                CompanionDataId = src.CompanionId,
                Seed = src.Seed,
                Level = src.Level <= 0 ? 1 : src.Level,
                Exp = src.Exp,
                Stats = ToStats(src.Stats),
                Rune = new CompanionRuneState
                {
                    FixedSequence =
                        src.FixedRuneSequence != null
                            ? new List<int>(src.FixedRuneSequence)
                            : new List<int>(),
                    UnlockedCount = src.UnlockedCount,
                },
                Equipment = new EquipmentSlots(),
            };
        }

    }
}
