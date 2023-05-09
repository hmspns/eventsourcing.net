using System.Text.Json;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Engine.Serialization;
using EventSourcing.Net.Tests.Shared;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace EventSourcing.Net.Serialization.NewtonsoftJson.Tests;

public sealed class RegistrationTests
{
    private const string FIELD_NAME = "_settings";

    [Fact]
    public void RegisterNewtonsoft()
    {
        IServiceCollection services = new ServiceCollection();
        EventSourcingOptions options = new EventSourcingOptions(services);
        
        options.Serialization.UseNewtonsoftJson();
        
        options.Build();

        IServiceProvider provider = services.BuildServiceProvider();

        IPayloadSerializerFactory? payloadSerializerFactory = provider.GetService<IPayloadSerializerFactory>();
        ISnapshotSerializerFactory? snapshotSerializerFactory = provider.GetService<ISnapshotSerializerFactory>();

        payloadSerializerFactory.Should().NotBeNull().And.BeOfType<NewtonsoftJsonPayloadSerializerFactory>();
        snapshotSerializerFactory.Should().NotBeNull().And.BeOfType<NewtonsoftJsonSnapshotSerializerFactory>();
        
        JsonSerializerSettings? payloadSettings = payloadSerializerFactory?.GetSerializer().GetPrivateField<JsonSerializerSettings>(FIELD_NAME);
        JsonSerializerSettings? snapshotSettings = snapshotSerializerFactory?.GetSerializer().GetPrivateField<JsonSerializerSettings>(FIELD_NAME);

        payloadSettings.Should().NotBeNull();
        snapshotSettings.Should().NotBeNull();
    }
    
    [Fact]
    public void RegisterNewtonsoft_WithOptions()
    {
        IServiceCollection services = new ServiceCollection();
        EventSourcingOptions options = new EventSourcingOptions(services);
        
        JsonSerializerSettings payloadSerializerSettings = new JsonSerializerSettings();
        JsonSerializerSettings snapshotSerializerSettings = new JsonSerializerSettings();
        
        options.Serialization.UseNewtonsoftJson(configurator =>
        {
            configurator.PayloadSerializationOptions = payloadSerializerSettings;
            configurator.SnapshotSerializationOptions = snapshotSerializerSettings;
        });

        options.Build();

        IServiceProvider provider = services.BuildServiceProvider();

        IPayloadSerializerFactory? payloadSerializerFactory = provider.GetService<IPayloadSerializerFactory>();
        ISnapshotSerializerFactory? snapshotSerializerFactory = provider.GetService<ISnapshotSerializerFactory>();

        payloadSerializerFactory.Should().NotBeNull().And.BeOfType<NewtonsoftJsonPayloadSerializerFactory>();
        snapshotSerializerFactory.Should().NotBeNull().And.BeOfType<NewtonsoftJsonSnapshotSerializerFactory>();
        
        JsonSerializerSettings? payloadSettings = payloadSerializerFactory?.GetSerializer().GetPrivateField<JsonSerializerSettings>(FIELD_NAME);
        JsonSerializerSettings? snapshotSettings = snapshotSerializerFactory?.GetSerializer().GetPrivateField<JsonSerializerSettings>(FIELD_NAME);

        payloadSettings.Should().Be(payloadSerializerSettings);
        snapshotSettings.Should().Be(snapshotSerializerSettings);
    }
}