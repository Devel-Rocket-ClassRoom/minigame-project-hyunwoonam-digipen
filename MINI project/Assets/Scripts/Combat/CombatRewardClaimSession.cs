using System;
using System.Collections.Generic;

public sealed class CombatRewardClaimSession
{
    public sealed class Entry
    {
        public int ItemId { get; }
        public int RemainingCount { get; internal set; }
        public bool IsSelected { get; internal set; }

        internal Entry(int itemId)
        {
            ItemId = itemId;
            RemainingCount = 1;
        }
    }

    private readonly List<Entry> entries = new List<Entry>();
    private readonly Func<int, bool> tryGrantItem;

    public IReadOnlyList<Entry> Entries => entries;
    public bool GetItemsPressed { get; private set; }

    public CombatRewardClaimSession(IEnumerable<int> itemIds, Func<int, bool> tryGrantItem)
    {
        this.tryGrantItem = tryGrantItem;
        var entriesById = new Dictionary<int, Entry>();

        if (itemIds == null)
        {
            return;
        }

        foreach (int itemId in itemIds)
        {
            if (itemId <= 0)
            {
                continue;
            }

            if (entriesById.TryGetValue(itemId, out Entry existing))
            {
                existing.RemainingCount++;
                continue;
            }

            var entry = new Entry(itemId);
            entriesById.Add(itemId, entry);
            entries.Add(entry);
        }
    }

    public void ToggleSelection(int itemId)
    {
        Entry entry = FindEntry(itemId);
        if (entry != null && entry.RemainingCount > 0)
        {
            entry.IsSelected = !entry.IsSelected;
        }
    }

    public int ClaimSelected()
    {
        GetItemsPressed = true;
        int claimedCount = 0;

        for (int i = 0; i < entries.Count; i++)
        {
            Entry entry = entries[i];
            if (!entry.IsSelected || entry.RemainingCount <= 0)
            {
                continue;
            }

            claimedCount += ClaimEntry(entry);
            if (entry.RemainingCount <= 0)
            {
                entry.IsSelected = false;
            }
        }

        return claimedCount;
    }

    public int ClaimRemainingSequentially()
    {
        if (GetItemsPressed)
        {
            return 0;
        }

        int claimedCount = 0;
        for (int i = 0; i < entries.Count; i++)
        {
            claimedCount += ClaimEntry(entries[i]);
        }

        return claimedCount;
    }

    private int ClaimEntry(Entry entry)
    {
        int claimedCount = 0;
        while (entry.RemainingCount > 0 && tryGrantItem?.Invoke(entry.ItemId) == true)
        {
            entry.RemainingCount--;
            claimedCount++;
        }

        return claimedCount;
    }

    private Entry FindEntry(int itemId)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].ItemId == itemId)
            {
                return entries[i];
            }
        }

        return null;
    }
}
