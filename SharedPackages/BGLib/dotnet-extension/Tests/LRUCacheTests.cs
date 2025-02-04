using BGLib.DotnetExtension.Collections;
using NUnit.Framework;

public class LRUCacheTests {

    [Test]
    public void Add_RemovesTheLeastUsedElementAfterMaxNumberOfElements() {

        var cache = new LRUCache<int, string>(maxNumberElements: 5);

        var anythingRemoved = false;
        cache.itemWillBeRemovedFromCacheEvent += (_, _) => { anythingRemoved = true; };

        cache.Add(1, "test1");
        Assert.IsTrue(cache.IsInCache(1));
        cache.Add(2, "test2");
        cache.Add(3, "test3");
        cache.Add(4, "test4");
        cache.Add(5, "test5");
        Assert.IsFalse(anythingRemoved);

        // Overflow the cache
        cache.Add(6, "test6");
        Assert.IsFalse(cache.IsInCache(1));
        Assert.IsTrue(anythingRemoved);
    }

    [Test]
    public void Add_OverridesElementsWithSameKey() {

        var cache = new LRUCache<int, string>(maxNumberElements: 5);

        var anythingRemoved = false;
        cache.itemWillBeRemovedFromCacheEvent += (_, _) => { anythingRemoved = true; };

        cache.Add(1, "test1");
        Assert.IsTrue(cache.IsInCache(1));
        cache.Add(2, "test2");
        cache.Add(1, "test3");
        cache.Add(2, "test4");
        cache.Add(1, "test5");
        cache.Add(2, "test6");
        Assert.IsFalse(anythingRemoved);
        var hasOne = cache.TryGetFromCache(1, out var keyOneValue);
        var hasTwo = cache.TryGetFromCache(2, out var keyTwoValue);
        Assert.IsTrue(hasOne);
        Assert.AreEqual("test5", keyOneValue);
        Assert.IsTrue(hasTwo);
        Assert.AreEqual("test6", keyTwoValue);
        Assert.AreEqual(2, cache.Count);
    }

    [Test]
    public void GetFromCache_MovesElementToTheEndOfQueue() {

        var cache = new LRUCache<int, string>(maxNumberElements: 5);

        var anythingRemoved = false;
        cache.itemWillBeRemovedFromCacheEvent += (_, _) => { anythingRemoved = true; };

        cache.Add(2, "test2");
        cache.Add(3, "test3");
        cache.Add(4, "test4");
        cache.Add(5, "test5");
        cache.Add(6, "test6");
        Assert.IsFalse(anythingRemoved);

        cache.TryGetFromCache(2, out _);
        cache.Add(7, "test7");
        Assert.IsTrue(cache.IsInCache(2));
        Assert.IsFalse(cache.IsInCache(3));
        Assert.IsTrue(anythingRemoved);

        anythingRemoved = false;
        cache.Clear();
        Assert.IsTrue(anythingRemoved);
    }

    [Test]
    public void Clear_CallsElementRemovedCallback() {

        var cache = new LRUCache<int, string>(maxNumberElements: 5);

        var anythingRemoved = false;
        cache.itemWillBeRemovedFromCacheEvent += (_, _) => { anythingRemoved = true; };

        cache.Add(4, "test4");
        cache.Add(5, "test5");
        cache.Add(6, "test6");
        cache.Add(2, "test2");
        cache.Add(7, "test7");
        Assert.IsFalse(anythingRemoved);
        cache.Clear();
        Assert.IsTrue(anythingRemoved);
    }

    [Test]
    public void ItemRemovedCallback_ContainsConsistentValues() {

        var cache = new LRUCache<int, string>(maxNumberElements: 5);
        int i = 0;
        cache.itemWillBeRemovedFromCacheEvent += (key, value) => {
            Assert.AreEqual(i, key);
            Assert.AreEqual($"test{i}", value);
            i++;
        };

        cache.Add(0, "test0");
        cache.Add(1, "test1");
        cache.Add(2, "test2");
        cache.Add(3, "test3");
        cache.Add(4, "test4");
        cache.Clear();
        Assert.AreEqual(0, cache.Count);
    }
}
