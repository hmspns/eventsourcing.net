using EventSourcing.Net.Abstractions.Contracts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net.Serialization.NewtonsoftJson.Tests;

using Abstractions.Identities;

public sealed class SerializationTests
{
    [Fact]
    public void PayloadSerialization()
    {
        IServiceCollection services = new ServiceCollection();
        EventSourcingOptions options = new EventSourcingOptions(services);
        
        options.Serialization.UseNewtonsoftJson();

        IServiceProvider provider = services.BuildServiceProvider();

        IPayloadSerializerFactory payloadSerializerFactory = provider.GetRequiredService<IPayloadSerializerFactory>();

        SomeData data = new SomeData(Guid.NewGuid(), CommandId.New());
        byte[] serialized = payloadSerializerFactory.GetSerializer().Serialize(data);
        object deserialized = payloadSerializerFactory.GetSerializer().Deserialize(typeof(SomeData), serialized);
        
        deserialized.Should().BeOfType<SomeData>().And.Be(data);
    }
    
    [Fact]
    public void SnapshotSerialization()
    {
        IServiceCollection services = new ServiceCollection();
        EventSourcingOptions options = new EventSourcingOptions(services);
        
        options.Serialization.UseNewtonsoftJson();

        IServiceProvider provider = services.BuildServiceProvider();

        ISnapshotSerializerFactory snapshotSerializerFactory = provider.GetRequiredService<ISnapshotSerializerFactory>();

        SomeData data = new SomeData(Guid.NewGuid(), CommandId.New());
        byte[] serialized = snapshotSerializerFactory.GetSerializer().Serialize(data);
        object deserialized = snapshotSerializerFactory.GetSerializer().Deserialize(typeof(SomeData), serialized);
        
        deserialized.Should().BeOfType<SomeData>().And.Be(data);
    }

    private record SomeData(Guid Id, CommandId CommandId);
}