# Sparcpoint.Extensions

Various useful extensions for the .NET Core Framework

## Sparcpoint.Extensions.DependencyInjection

Decoration and child service extensions for `Microsoft.Extensions.DependencyInjection`

### Installation

#### Package Manager Console
```
Install-Package Sparcpoint.Extensions
```

#### .NET Core CLI
```
dotnet add package Sparcpoint.Extensions
```

### Usage

This library adds two extension methods to `IServiceCollection`

* `Decorate` - Allows usage of the decorator pattern with Microsoft DI.
* `WithChildServices` - Allows an isolated scope of service injection for a parent service.

### Examples

#### Decoration

Decoration allows for extending functionality of a class without changing the interface 
or the original implementation. Instead, you wrap instances of implementations that provide
common functionality.

An example might be that you have a service that sends and receives from an endpoint called `DefaultEndpointClient : IEndpointClient`. You would like to introduce logging and performance measurements. However, you know if you add it directly to your implementation, you cannot use those again. Instead, you decorate `IEndpointClient` with a `LoggedEndpointClient : IEndpointClient` and a `MeasurePerformanceClient : IEndpointClient`, both of which take a `IEndpointClient` into its constructor. Now you have 

`LoggedEndpointClient` -> `MeasurePerformanceClient` -> `DefaultEndpointClient` 

chained together decorating the inner services.

See [Decorator Pattern](https://en.wikipedia.org/wiki/Decorator_pattern)

```csharp
/*
 * Basic Usage
 * The most common usage of decorators
 */
ServiceCollection services = new();

// Add the root service
services.AddSingleton<IService, ServiceImplementation>();

// Decorate all services currently registered
services.Decorate<IService, LoggingImplementation>();

// Build the provider
IServiceProvider provider = services.BuildServiceProvider();

// When we provide the service, we will receive ServiceDecorator first, 
// while the inner service will be the ServiceImplementation
// LoggingImplementation -> ServiceImplementation -> IService
var instance = provider.GetRequiredService<IService>();
```

```csharp
/*
 * Open Generic Usage
 * We can also decorate open generics
 */
ServiceCollection services = new();

// Add the root service
services.AddSingleton<IEventProcessor<ServiceEvent>, DefaultEventProcessor<ServiceEvent>>();

// Decorate all services currently registered
services.Decorate(typeof(IEventProcessor<>), typeof(MeasurePerformanceProcessor<>));

// Build the provider
IServiceProvider provider = services.BuildServiceProvider();

// Like the basic use case, we have decorated the previously registered services
// MeasurePerformanceProcessor<ServiceEvent> -> DefaultEventProcessor<ServiceEvent> -> IEventProcessor<ServiceEvent>
var instance = provider.GetRequiredService<IEventProcessor<ServiceEvent>>();
```

#### Child Services

Child Services allow for certain instances of services to be built with an 
isolated set of registered services separated from the main provider. This
allows for sub-services to be different as needed from implementation
to implementation

```csharp
/*
 * Basic Usage
 */

ServiceCollection services = new();

// This service is registered with the root provider.
services.AddSingleton<IMessageBus, InMemoryServiceBus>();
services.WithChildServices<DefaultAccountService>(child => 
{
  // All services in this container will be used to create the DefaultAccountService
  // Extra services are ignored
  services.AddSingleton<IMessageBus>(p => new AzureServiceBus("connectionString"));
});

var provider = services.BuildServiceProvider();

// This will return a DefaultAccountService with an AzureServiceBus implementation
// DefaultAccountService -> AzureServiceBus
var accountService = provider.GetRequiredService<DefaultAccountService>();

// This will return the "default" implementation of InMemoryServiceBus
var messageBus = provider.GetRequiredService<IMessageBus>();
```

```csharp
/*
 * Setting the Parent's Lifetime
 * By default, a parent is setup as a Singleton. If you need
 * a different lifetime, it is specified as a parameter on
 * WithChildServices(...)
 */

 // DefaultAccountService will have a Transient lifetime
 // and will be created on every provide of this service
services.WithChildServices<DefaultAccountService>(child => 
{
  services.AddSingleton<IMessageBus>(p => new AzureServiceBus("connectionString"));
}, lifetime: ServiceLifetime.Transient);
```

```csharp
/*
 * Decoration Example
 * Sometimes you may not want to simply replace the parent service
 * Instead, you may want to decorate it
 */

ServiceCollection services = new();

// This service is registered with the root provider.
services.AddSingleton<IMessageBus, InMemoryServiceBus>();
services.WithChildServices<DefaultAccountService>(child => 
{
  services.Decorate<IMessageBus, LoggedMessageBus>();
});

var provider = services.BuildServiceProvider();

// This will return a DefaultAccountService with a LoggedMessageBus implementation
// DefaultAccountService -> LoggedMessageBus -> InMemoryServiceBus
var accountService = provider.GetRequiredService<DefaultAccountService>();

// This will return the "default" implementation of InMemoryServiceBus
var messageBus = provider.GetRequiredService<IMessageBus>();
```