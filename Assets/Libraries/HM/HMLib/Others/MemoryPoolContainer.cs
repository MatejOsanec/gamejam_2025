using System.Collections.Generic;
using Zenject;

public class MemoryPoolContainer<T> {

    public List<T> activeItems => _activeItems.items;

    private readonly LazyCopyHashSet<T> _activeItems = new LazyCopyHashSet<T>();
    private readonly IMemoryPool<T> _memoryPool;

    public MemoryPoolContainer(IMemoryPool<T> memoryPool) {

        _memoryPool = memoryPool;
    }

    public T Spawn() {

        var item = _memoryPool.Spawn();
        _activeItems.Add(item);
        return item;
    }

    public void Despawn(T item) {

        _activeItems.Remove(item);
        _memoryPool.Despawn(item);
    }

    public void DespawnAll() {

        foreach (var activeItem in activeItems) {
            _memoryPool.Despawn(activeItem);
        }
        _activeItems.Clear();
    }

    public void DestroyAll() {

        DespawnAll();
        _memoryPool.Clear();
    }
}

public class MemoryPoolContainer<T0, T1> where T0 : T1 {

    public List<T1> activeItems => _activeItems.items;
    private readonly LazyCopyHashSet<T1> _activeItems = new LazyCopyHashSet<T1>();

    private readonly IMemoryPool<T0> _memoryPool;

    public MemoryPoolContainer(IMemoryPool<T0> memoryPool) {

        _memoryPool = memoryPool;
    }

    public T0 Spawn() {

        var item = _memoryPool.Spawn();
        _activeItems.Add(item);
        return item;
    }

    public void Despawn(T0 item) {

        _activeItems.Remove(item);
        _memoryPool.Despawn(item);
    }

    public void DespawnAll() {

        foreach (var activeItem in activeItems) {
            _memoryPool.Despawn(activeItem);
        }
        _activeItems.Clear();
    }

    public void DestroyAll() {

        DespawnAll();
        _memoryPool.Clear();
    }
}
