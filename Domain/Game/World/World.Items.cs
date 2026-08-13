using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public void RemoveItem(ItemId id)
    {
        GetItem(id);
        _items.Remove(id);
        _itemReservations.ReleaseForItem(id);
    }
}
