namespace Tempt
{
    public static class RuneRuntimeApplier
    {
        public static void ApplyToCurrentPlayer(PlayerRuneState state)
        {
            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                return;
            }

            if (
                gsm.CurrentRun?.Player != null
                && ReferenceEquals(gsm.CurrentRun.Player.Rune, state)
            )
            {
                ApplyToPlayerState(gsm.CurrentRun.Player);
            }

            if (gsm.ActivePlayer != null && ReferenceEquals(gsm.ActivePlayer.Rune, state))
            {
                gsm.ActivePlayer.SyncPassivesFromRunes(gsm.Data);
                gsm.ActivePlayer.RecalcBonusStats();
            }
        }

        public static void ApplyToPlayerState(PlayerState player)
        {
            if (player?.Stats == null || player.Rune == null)
            {
                return;
            }

            EquipmentStatMod runeMod = player.Rune.AggregateStatMod();
            player.Stats.ResetRuneBonuses();
            player.Stats.ApplyRuneBonus(StatType.HP, runeMod.HP);
            player.Stats.ApplyRuneBonus(StatType.MP, runeMod.MP);
            player.Stats.ApplyRuneBonus(StatType.ATK, runeMod.ATK);
            player.Stats.ApplyRuneBonus(StatType.DEF, runeMod.DEF);
            player.Stats.ApplyRuneBonus(StatType.SPD, runeMod.SPD);
        }
    }
}
