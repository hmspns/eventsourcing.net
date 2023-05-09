using EventSourcing.Net.Abstractions.Contracts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net.Serialization.ProtobufNet.Tests;

public sealed class RegistrationTests
{
    [Fact]
    public void RegisterProtobufNet()
    {
        IServiceCollection services = new ServiceCollection();
        EventSourcingOptions options = new EventSourcingOptions(services);
        
        options.Serialization.UseProtobufNet();
        
        options.Build();

        IServiceProvider provider = services.BuildServiceProvider();

        IPayloadSerializerFactory? payloadSerializerFactory = provider.GetService<IPayloadSerializerFactory>();
        ISnapshotSerializerFactory? snapshotSerializerFactory = provider.GetService<ISnapshotSerializerFactory>();

        payloadSerializerFactory.Should().NotBeNull().And.BeOfType<ProtobufNetPayloadSerializerFactory>();
        snapshotSerializerFactory.Should().NotBeNull().And.BeOfType<ProtobufNetSnapshotSerializerFactory>();
    }
}