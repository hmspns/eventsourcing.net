using EventSourcing.Net.Abstractions.Contracts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf;

namespace EventSourcing.Net.Serialization.ProtobufNet.Tests;

public sealed class SerializationTests
{
    [Fact]
    public void PayloadSerialization()
    {
        IServiceCollection services = new ServiceCollection();
        EventSourcingOptions options = new EventSourcingOptions(services);
        
        options.Serialization.UseProtobufNet();

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
        
        options.Serialization.UseProtobufNet();

        IServiceProvider provider = services.BuildServiceProvider();

        ISnapshotSerializerFactory snapshotSerializerFactory = provider.GetRequiredService<ISnapshotSerializerFactory>();

        SomeData data = new SomeData(Guid.NewGuid());
        byte[] serialized = snapshotSerializerFactory.GetSerializer().Serialize(data);
        object deserialized = snapshotSerializerFactory.GetSerializer().Deserialize(typeof(SomeData), serialized);
        
        deserialized.Should().BeOfType<SomeData>().And.Be(data);
    }

    [ProtoContract]
    private record SomeData()
    {
        internal SomeData(Guid id) : this()
        {
            Id = id;
        }
        
        [ProtoMember(1)] 
        internal Guid Id { get; set; }
    };
}