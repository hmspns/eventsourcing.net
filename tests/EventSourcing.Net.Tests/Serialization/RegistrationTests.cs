using System.Text.Json;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Engine.Serialization;
using EventSourcing.Net.Tests.Shared;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net.Tests.Serialization;

public sealed class RegistrationTests
{
    [Fact]
    public void RegisterSystemTextJsonSerializers()
    {
        IServiceCollection services = new ServiceCollection();
        EventSourcingOptions options = new EventSourcingOptions(services);
        
        options.Build();

        IServiceProvider provider = services.BuildServiceProvider();

        IPayloadSerializerFactory? payloadSerializerFactory = provider.GetService<IPayloadSerializerFactory>();
        ISnapshotSerializerFactory? snapshotSerializerFactory = provider.GetService<ISnapshotSerializerFactory>();

        payloadSerializerFactory.Should().NotBeNull().And.BeOfType<SystemTextJsonPayloadSerializerFactory>();
        snapshotSerializerFactory.Should().NotBeNull().And.BeOfType<SystemTextJsonSnapshotSerializerFactory>();

        JsonSerializerOptions? payloadOptions = payloadSerializerFactory?.GetSerializer().GetPrivateField<JsonSerializerOptions>("_options");
        JsonSerializerOptions? snapshotOptions = snapshotSerializerFactory?.GetSerializer().GetPrivateField<JsonSerializerOptions>("_options");

        payloadOptions.Should().NotBeNull();
        snapshotOptions.Should().NotBeNull();
    }
    
    [Fact]
    public void RegisterSystemTextJsonSerializers_WithOptions()
    {
        IServiceCollection services = new ServiceCollection();
        EventSourcingOptions options = new EventSourcingOptions(services);
        
        JsonSerializerOptions payloadSerializationOptions = new JsonSerializerOptions();
        JsonSerializerOptions snapshotSerializationOptions = new JsonSerializerOptions();
        
        options.Serialization.UseSystemTextJson(configurator =>
        {
            configurator.PayloadSerializationOptions = payloadSerializationOptions;
            configurator.SnapshotSerializationOptions = snapshotSerializationOptions;
        });
        
        options.Build();

        IServiceProvider provider = services.BuildServiceProvider();

        IPayloadSerializerFactory? payloadSerializerFactory = provider.GetService<IPayloadSerializerFactory>();
        ISnapshotSerializerFactory? snapshotSerializerFactory = provider.GetService<ISnapshotSerializerFactory>();

        payloadSerializerFactory.Should().NotBeNull().And.BeOfType<SystemTextJsonPayloadSerializerFactory>();
        snapshotSerializerFactory.Should().NotBeNull().And.BeOfType<SystemTextJsonSnapshotSerializerFactory>();
        
        JsonSerializerOptions? payloadOptions = payloadSerializerFactory?.GetSerializer().GetPrivateField<JsonSerializerOptions>("_options");
        JsonSerializerOptions? snapshotOptions = snapshotSerializerFactory?.GetSerializer().GetPrivateField<JsonSerializerOptions>("_options");

        payloadOptions.Should().Be(payloadSerializationOptions);
        snapshotOptions.Should().Be(snapshotSerializationOptions);
    }
}