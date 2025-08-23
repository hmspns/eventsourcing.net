namespace EventSourcing.Net.Tests.Collections;

using Engine.Collections;
using FluentAssertions;

public class LinkedDictionary
{
    [Fact]
    public void Enumerator_Test()
    {
        LinkedDictionary<int, int> dictionary = new LinkedDictionary<int, int>(EqualityComparer<int>.Default);
        dictionary.Add(1, 1);
        dictionary.Add(2, 2);
        dictionary.Add(3, 3);

        int index = 1;
        foreach (KeyValuePair<int, int> pair in dictionary)
        {
            pair.Key.Should().Be(index);
            pair.Value.Should().Be(index);
            index++;
        }
    }
    
    [Fact]
    public void HybirdDictionary_Test()
    {
        HybridDictionary<int, int> dictionary = new HybridDictionary<int, int>(EqualityComparer<int>.Default);
        dictionary.Add(1, 1);
        dictionary.Add(2, 2);
        dictionary.Add(3, 3);

        int index = 1;
        foreach (KeyValuePair<int, int> pair in dictionary)
        {
            pair.Key.Should().Be(index);
            pair.Value.Should().Be(index);
            index++;
        }
    }
}