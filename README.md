# GenieDotNet

GenieDotNet consists of 3 main components to showcase how to create the most advanced, high performance cloud native open-source distributed game licensing system available using C# and .NET 8
complete with [Crank](https://github.com/dotnet/crank) benchmarks [here](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/Genie.Benchmarks/benchmark.yaml)

# Genie
Geospatial Event Network Inference Engine - (Event-Sourced Microservice Broker) Reverse Geocoder using [OvertureMaps](https://overturemaps.org/) 
(and MaxMind) supporting ActiveMQ, RabbitMQ, Kafka, [Pulsar](https://github.com/fsprojects/pulsar-client-dotnet), [Proto.Actor](https://github.com/asynkron/protoactor-dotnet) and [DuckDB](https://github.com/Giorgi/DuckDB.NET)

# Genius
Geospatial Event Network Information User Stream - (Stream processor) for time & location sensitive eventing featuring [Proto.Actor](https://github.com/asynkron/protoactor-dotnet)

# Genuine
Geospatial Event Network User Integrated Network Encryption - [Double Ratchet](https://signal.org/docs/specifications/doubleratchet/) (Signal algorithm) + Replay & Tamper Resistant [Protocol](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/GameLicenseExample/Game.cs) featuring [Bouncycastle X25519 & Ed25519](https://github.com/bcgit/bc-csharp), [HKDF](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.hkdf?view=net-8.0), [AES](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes?view=net-8.0), and [CityHash](https://aras-p.info/blog/2016/08/09/More-Hash-Function-Tests/)

## Prerequisites
[Consul](https://developer.hashicorp.com/consul) - Mesh provider

Bitnami/Confluent- Schema Registry and Kafka [docker-compose.yml](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/Genie.Benchmarks/docker-compose.yml) 

[Crank](https://github.com/dotnet/crank) - Web benchmark [agent](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/Genie.Benchmarks/benchmark.yaml)

## Optional
Pulsar
RabbitMQ
ActiveMQ
