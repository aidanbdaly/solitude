using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Solitude.Domain.Game.Agents;
using Solitude.Domain.Game.Items;

namespace Solitude.Domain.Game;

public sealed partial class World
{
    private int _nextItemId = 1;
    private readonly Dictionary<ItemId, Item> _items = new();

    public IReadOnlyCollection<Item> Items => _items.Values;

    public ItemId CreateItem(Vector2I cell, ItemType type, int count)
    {
        if (!Grid.Contains(cell))
            throw new ArgumentOutOfRangeException(nameof(cell), cell, "Item cell is outside the grid.");
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "A world item must have a positive count.");

        var existing = ItemAt(cell, type);
        if (existing is not null)
        {
            existing.Count += count;
            return existing.Id;
        }

        var id = new ItemId(_nextItemId++);
        _items.Add(id, new Item { Id = id, Cell = cell, Type = type, Count = count });
        return id;
    }

    public bool TryGetItem(ItemId id, out Item item) =>
        _items.TryGetValue(id, out item!);

    public Item GetItem(ItemId id)
    {
        if (!TryGetItem(id, out var item))
            throw new KeyNotFoundException($"Item {id.Value} does not exist.");
        return item;
    }

    public IEnumerable<Item> ItemsAt(Vector2I cell) =>
        _items.Values.Where(item => item.Cell == cell);

    public Item? ItemAt(Vector2I cell, ItemType type) =>
        _items.Values.FirstOrDefault(item => item.Cell == cell && item.Type == type);

    public bool TryCollectItem(
        AgentId agentId,
        ItemId itemId,
        int count)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));

        var agent = GetAgent(agentId);
        if (!TryGetItem(itemId, out var item))
            return false;
        if (agent.Cell != item.Cell)
            throw new InvalidOperationException(
                $"Agent {agentId.Value} cannot collect item {itemId.Value} from another cell.");
        if (item.Count < count || agent.Inventory.AvailableCapacity < count)
            return false;

        var collected = item.Take(count);
        if (collected.Count != count)
            throw new InvalidOperationException(
                $"Item {itemId.Value} did not produce its requested count.");
        if (agent.Inventory.Add(collected.Type, collected.Count) != collected.Count)
            throw new InvalidOperationException(
                $"Agent {agentId.Value} inventory rejected a validated item collection.");
        if (item.IsDepleted) RemoveItem(item.Id);
        return true;
    }

    public void RemoveItem(ItemId id)
    {
        GetItem(id);
        _items.Remove(id);
    }
}
