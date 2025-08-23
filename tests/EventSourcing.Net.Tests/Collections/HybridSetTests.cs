namespace EventSourcing.Net.Tests.Collections;

using Engine.Collections;
using FluentAssertions;

public class HybridSetTests
{
    [Fact]
    public void HybridSet_Add_Enumerate_Test()
    {
        HybridSet<int> set = new HybridSet<int>(EqualityComparer<int>.Default);
        set.Add(1).Should().BeTrue();
        set.Add(2).Should().BeTrue();
        set.Add(3).Should().BeTrue();

        int index = 1;
        foreach (int value in set)
        {
            value.Should().Be(index);
            index++;
        }
    }

    [Fact]
    public void HybridSet_Remove_Test()
    {
        HybridSet<int> set = new HybridSet<int>(EqualityComparer<int>.Default);
        set.Add(1);
        set.Add(2);
        set.Add(3);

        set.Remove(2).Should().BeTrue();
        set.Count.Should().Be(2);

        set.Contains(1).Should().BeTrue();
        set.Contains(2).Should().BeFalse();
        set.Contains(3).Should().BeTrue();
    }

    [Fact]
    public void HybridSet_CopyTo_Test()
    {
        HybridSet<int> set = new HybridSet<int>(EqualityComparer<int>.Default);
        set.Add(3);
        set.Add(5);
        set.Add(7);

        int[] array = new int[5];
        set.CopyTo(array, 1);

        array[0].Should().Be(0);
        array[1].Should().Be(3);
        array[2].Should().Be(5);
        array[3].Should().Be(7);
        array[4].Should().Be(0);
    }

    [Fact]
    public void HybridSet_UnionWith_Forces_Swap_And_Merges()
    {
        HybridSet<int> set = new HybridSet<int>(EqualityComparer<int>.Default);
        set.Add(1);
        set.Add(2);
        set.Add(3);

        set.UnionWith(new[] { 3, 4, 5 });

        set.Count.Should().Be(5);
        set.Contains(1).Should().BeTrue();
        set.Contains(2).Should().BeTrue();
        set.Contains(3).Should().BeTrue();
        set.Contains(4).Should().BeTrue();
        set.Contains(5).Should().BeTrue();
    }

    [Fact]
    public void HybridSet_IntersectWith_Forces_Swap_And_Filters()
    {
        HybridSet<int> set = new HybridSet<int>(EqualityComparer<int>.Default);
        set.Add(1);
        set.Add(2);
        set.Add(3);
        set.Add(4);

        set.IntersectWith(new[] { 2, 4, 6 });

        set.Count.Should().Be(2);
        set.Contains(2).Should().BeTrue();
        set.Contains(4).Should().BeTrue();
        set.Contains(1).Should().BeFalse();
        set.Contains(3).Should().BeFalse();
    }

    [Fact]
    public void HybridSet_ExceptWith_Forces_Swap_And_Removes()
    {
        HybridSet<int> set = new HybridSet<int>(EqualityComparer<int>.Default);
        set.Add(1);
        set.Add(2);
        set.Add(3);
        set.Add(4);

        set.ExceptWith(new[] { 2, 4, 6 });

        set.Count.Should().Be(2);
        set.Contains(1).Should().BeTrue();
        set.Contains(3).Should().BeTrue();
        set.Contains(2).Should().BeFalse();
        set.Contains(4).Should().BeFalse();
    }

    [Fact]
    public void HybridSet_SymmetricExceptWith_Forces_Swap_And_Toggles()
    {
        HybridSet<int> set = new HybridSet<int>(EqualityComparer<int>.Default);
        set.Add(1);
        set.Add(2);
        set.Add(3);

        set.SymmetricExceptWith(new[] { 2, 3, 4, 5 });

        set.Count.Should().Be(3);
        set.Contains(1).Should().BeTrue();
        set.Contains(2).Should().BeFalse();
        set.Contains(3).Should().BeFalse(); 
        set.Contains(4).Should().BeTrue(); 
        set.Contains(5).Should().BeTrue(); 
    }

    [Fact]
    public void HybridSet_Relations_Delegated_To_HashSet()
    {
        HybridSet<int> set = new HybridSet<int>(EqualityComparer<int>.Default);
        set.Add(1);
        set.Add(2);
        set.Add(3);

        set.IsSubsetOf(new[] { 0, 1, 2, 3, 4 }).Should().BeTrue();
        set.IsSupersetOf(new[] { 1, 2 }).Should().BeTrue();
        set.Overlaps(new[] { 5, 6, 2 }).Should().BeTrue();
        set.SetEquals(new[] { 1, 2, 3 }).Should().BeTrue();

        set.IsProperSubsetOf(new[] { 0, 1, 2, 3 }).Should().BeTrue();
        set.IsProperSupersetOf(new[] { 1, 2 }).Should().BeTrue();
    }
}
