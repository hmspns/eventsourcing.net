namespace EventSourcing.Net.Tests.Collections;

using Engine.Collections;
using FluentAssertions;

public class LinkedSetTests
{
    [Fact]
    public void Enumerator_Test()
    {
        LinkedSet<int> set = new LinkedSet<int>(EqualityComparer<int>.Default);
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
    public void Add_Duplicate_ShouldNotIncreaseCount()
    {
        LinkedSet<int> set = new LinkedSet<int>(EqualityComparer<int>.Default);
        set.Add(42).Should().BeTrue();
        set.Add(42).Should().BeFalse();

        set.Count.Should().Be(1);
        set.Contains(42).Should().BeTrue();
    }

    [Fact]
    public void Remove_Test()
    {
        LinkedSet<int> set = new LinkedSet<int>(EqualityComparer<int>.Default);
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
    public void CopyTo_Test()
    {
        LinkedSet<int> set = new LinkedSet<int>(EqualityComparer<int>.Default);
        set.Add(10);
        set.Add(20);
        set.Add(30);

        int[] array = new int[5];
        set.CopyTo(array, 1);

        array[0].Should().Be(0);
        array[1].Should().Be(10);
        array[2].Should().Be(20);
        array[3].Should().Be(30);
        array[4].Should().Be(0);
    }
}
