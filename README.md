# GenieDotNet

GenieDotNet consists of 3 main components to showcase how to create the most advanced, [high](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/Genie.Common/Adapters/RabbitMQ/RabbitMQPump.cs) [performance](https://learn.microsoft.com/en-us/aspnet/core/performance/objectpool?view=aspnetcore-8.0) cloud native, open-source, and [distributed](https://github.com/grpc/grpc-dotnet) game [licensing system](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/SharedFiles/Protos/genius.proto) available using C# and .NET 8
complete with [Crank](https://github.com/dotnet/crank) benchmarks [here](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/Genie.Benchmarks/benchmark.yaml)

# Genie
Geospatial Event Network Inference Engine - (Event-Sourced Microservice Broker) Reverse Geocoder using [OvertureMaps](https://overturemaps.org/) 
(Opendatasoft or MaxMind) supporting ActiveMQ, RabbitMQ, Kafka, [Pulsar](https://github.com/fsprojects/pulsar-client-dotnet), [Proto.Actor](https://github.com/asynkron/protoactor-dotnet) and [DuckDB](https://github.com/Giorgi/DuckDB.NET)

# Genius
Geospatial Event Network Information User Stream - (Stream processor) for [time & location-based](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/Genie.Extensions.Genius/GeniusGrain.cs) eventing featuring [Proto.Actor](https://github.com/asynkron/protoactor-dotnet) with any level of granularity

# Genuine
Geospatial Event Network User Integrated Network Encryption - [Double Ratchet](https://signal.org/docs/specifications/doubleratchet/) (Signal algorithm) + Replay & Tamper Resistant [Protocol](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/GameLicenseExample/Game.cs) featuring [Bouncycastle X25519 & Ed25519](https://github.com/bcgit/bc-csharp), [HKDF](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.hkdf?view=net-8.0), [AES](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes?view=net-8.0), and [CityHash](https://aras-p.info/blog/2016/08/09/More-Hash-Function-Tests/)

## Prerequisites
[Consul](https://developer.hashicorp.com/consul) - Mesh provider (for Proto.actor)

Confluent/Bitnami- Schema Registry and Kafka [docker-compose.yml](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/Genie.Benchmarks/docker-compose.yml) 

[Crank](https://github.com/dotnet/crank) - Web benchmark [agent](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/Genie.Benchmarks/benchmark.yaml)

## Hosts
Most services are referenced by a named host so will require hosts file manipulation (or config updates)

## Spatial Map Data
Map [paths](https://github.com/gradx/GenieDotNet/tree/main/GenieDotNet/SharedFiles/OvertureMaps) need to be updated as well as code [removed](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/Genie.Common/Utils/DuckDbSupport.cs) for the missing postal code file (too large to include).  Instructions on how to create these using a python script coming soon.

## Optional
__All benchmarks were tested in "standalone" mode__ on a single node Intel 13980hx with 32GB DDR5 3200+ 

Pulsar - highest concurrent connections (256) with decent throughput (4700rps) in f&f.  Recommended for longer running tasks not requiring ack.

RabbitMQ - clear best peformer (5x others) in fire & forget, close second to Proto.Actor (640rps/8conn vs 580rps/10conn) in terms of acknwoledged delivery but with persistence built in. 
Like Proto.Actor (8) suffers from a low optional concurrent connection count (6-10) but also __has a very low error rate__ (33 errors with 26.7M req handled in f&f) while Proto.actor __is nearly errorless__!

ActiveMQ - lowest optimal connections (3), half the performance of others on average (200rps), with no clear advantage other than Java

