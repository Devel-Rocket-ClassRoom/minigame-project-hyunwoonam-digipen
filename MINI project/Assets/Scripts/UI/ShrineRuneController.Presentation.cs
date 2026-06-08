using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    public sealed partial class ShrineRuneController
    {
        private void SetTabVisual(Button button, bool active)
        {
            if (button == null)
            {
                return;
            }

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            SetTextColor(label, active ? activeText : inactiveText);
            Transform indicator = button.transform.Find("Indicator");
            if (indicator != null)
            {
                indicator.gameObject.SetActive(active);
            }
        }

        private static void SetContentActive(GameObject content, bool active)
        {
            if (content != null)
            {
                content.SetActive(active);
            }
        }

        private static void SetText(TMP_Text label, string value)
        {
            if (label != null)
            {
                label.text = value;
            }
        }

        private static void SetTextColor(TMP_Text label, Color color)
        {
            if (label != null)
            {
                label.color = color;
            }
        }

        private static string ClassTitle(RuneClass runeClass)
        {
            switch (runeClass)
            {
                case RuneClass.Dealer:
                    return "CRIMSON OATH";
                case RuneClass.Tanker:
                    return "IRON VOW";
                case RuneClass.MagicDealer:
                    return "ASHEN SCRIPT";
                case RuneClass.Supporter:
                    return "SILVER HYMN";
                default:
                    return "SELECT RUNE";
            }
        }

        private static string ClassTag(RuneClass runeClass)
        {
            return runeClass == RuneClass.MagicDealer
                ? "MAGIC DEALER"
                : runeClass.ToString().ToUpperInvariant();
        }

        private static string ClassRole(RuneClass runeClass)
        {
            return ClassTag(runeClass) + " RUNE";
        }

        private static string ClassDescription(RuneClass runeClass)
        {
            switch (runeClass)
            {
                case RuneClass.Dealer:
                    return "A blood-bound oath enhancing physical prowess.";
                case RuneClass.Tanker:
                    return "A defensive vow focused on survival and armor.";
                case RuneClass.MagicDealer:
                    return "An ashen script strengthening spell pressure.";
                case RuneClass.Supporter:
                    return "A silver hymn improving recovery and stability.";
                default:
                    return string.Empty;
            }
        }

        private static string ClassBonus(RuneClass runeClass)
        {
            switch (runeClass)
            {
                case RuneClass.Dealer:
                    return "ATK + SPD";
                case RuneClass.Tanker:
                    return "HP + DEF";
                case RuneClass.MagicDealer:
                    return "MP + ATK";
                case RuneClass.Supporter:
                    return "HP + MP";
                default:
                    return string.Empty;
            }
        }
    }
}
