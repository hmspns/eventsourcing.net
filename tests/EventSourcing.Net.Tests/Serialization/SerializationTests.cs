using EventSourcing.Net.Abstractions.Contracts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net.Tests.Serialization;

public sealed class SerializationTests
{
    [Fact]
    public void PayloadSerialization()
    {
        IServiceCollection services = new ServiceCollection();
        EventSourcingOptions options = new EventSourcingOptions(services);
        
        options.Build();

        IServiceProvider provider = services.BuildServiceProvider();

        IPayloadSerializerFactory payloadSerializerFactory = provider.GetRequiredService<IPayloadSerializerFactory>();

        SomeData data = new SomeData(Guid.NewGuid());
        byte[] serialized = payloadSerializerFactory.GetSerializer().Serialize(data);
        object deserialized = payloadSerializerFactory.GetSerializer().Deserialize(typeof(SomeData), serialized);
        
        deserialized.Should().BeOfType<SomeData>().And.Be(data);
    }
    
    [Fact]
    public void SnapshotSerialization()
    {
        IServiceCollection services = new ServiceCollection();
        EventSourcingOptions options = new EventSourcingOptions(services);
        
        options.Build();

        IServiceProvider provider = services.BuildServiceProvider();

        ISnapshotSerializerFactory snapshotSerializerFactory = provider.GetRequiredService<ISnapshotSerializerFactory>();

        SomeData data = new SomeData(Guid.NewGuid());
        byte[] serialized = snapshotSerializerFactory.GetSerializer().Serialize(data);
        object deserialized = snapshotSerializerFactory.GetSerializer().Deserialize(typeof(SomeData), serialized);
        
        deserialized.Should().BeOfType<SomeData>().And.Be(data);
    }

    private record SomeData(Guid Id);
}