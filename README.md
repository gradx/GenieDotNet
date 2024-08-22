# GenieDotNet

GenieDotNet consists of 3 main components to showcase how to create a (1) secure, (2) cloud native, (3) scalable, (4) distributed, (5) [high performance](https://learn.microsoft.com/en-us/aspnet/core/performance/objectpool?view=aspnetcore-8.0) (6) inference engine and (7) extensible (8) real-time streaming processor with a license provisioning [example](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/Genie.Extensions.Genius/GeniusGrain.cs#L84) using C# and .NET 8
complete with [Crank](https://github.com/dotnet/crank) benchmarks [here](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/Genie.Benchmarks/benchmark.yaml)

# Genie
Geospatial Event Network Inference Engine - (Event-Sourced Microservice Broker) Reverse Geocoder using [OvertureMaps](https://overturemaps.org/) 
(Opendatasoft or MaxMind) supporting ActiveMQ, RabbitMQ, Kafka, [Pulsar](https://github.com/fsprojects/pulsar-client-dotnet), [Proto.Actor](https://github.com/asynkron/protoactor-dotnet) and [DuckDB](https://github.com/Giorgi/DuckDB.NET)

# Genius
Geospatial Event Network Information User Stream - (Real-time Streaming Processor) for [time & location-based](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/Genie.Extensions.Genius/GeniusGrain.cs#L93) eventing featuring Proto.Actor with any level of granularity

# Genuine
Geospatial Event Network User Integrated Network Encryption - [Double Ratchet](https://signal.org/docs/specifications/doubleratchet/) (Signal algorithm) + Replay & Tamper Resistant [Protocol](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/GameLicenseExample/Game.cs#L134) featuring [Bouncycastle X25519 & Ed25519](https://github.com/bcgit/bc-csharp), [HKDF](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.hkdf?view=net-8.0), [AES](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes?view=net-8.0), and [CityHash](https://aras-p.info/blog/2016/08/09/More-Hash-Function-Tests/)

## Prerequisites
[Consul](https://developer.hashicorp.com/consul) - Mesh provider (for Proto.actor)

Confluent/Bitnami- Schema Registry and Kafka [docker-compose.yml](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/Genie.Benchmarks/docker-compose.yml) 

[Crank](https://github.com/dotnet/crank) - Web benchmark [agent](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/Genie.Benchmarks/benchmark.yaml)

## Spatial Map Data
Map [paths](https://github.com/gradx/GenieDotNet/tree/main/GenieDotNet/SharedFiles/OvertureMaps) need to be updated as well as code [removed](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/Genie.Common/Utils/DuckDbSupport.cs) for the missing postal code file (too large to include).  Instructions on how to create these using a python script coming soon.

## Benchmarks
__All benchmarks were produced with Crank in **standalone mode**__ on a single node Intel 13980hx with 32GB DDR5 3200+ 

### Round trip
#### Baseline
| Broker   | Connections   | Requests/Sec  | Latency (ms)   |
|---|---|---|---|
| None  | 1  | 22738   | 0  |
| ActiveMQ  | 1  | 421   | 2  |
| Kafka  | 1  | 33   | 33  |
| Proto.Actor  | 1  | 653  | 1 |
| Pulsar  | 1  | 36   | 31  |
| RabbitMQ  | 1  | 457   | 2  |

#### Scaled
| Broker   | Connections   | Requests/Sec  | Latency (ms)   | Error Rate   |
|---|---|---|---|---|
| ActiveMQ  | 32  | 1900   | 17  | Extremely low, 30 min+ sustained
| Kafka  | 128  | 3300   | 41  | Very low
| Proto.Actor  | 32  | 3300   | 11 | None
| Pulsar  | 32  | 580   | 55  | None 
| RabbitMQ  | 32  | 1800   | 9  | Very Low (Message corruption)

#### Fire & Forget
| Broker   | Connections   | Requests/Sec
|---|---|---|
| ActiveMQ  | 48 | 5800   |
| Kafka  |  96 | 4300  |
| Proto.Actor | n/a |  n/a  |
| Pulsar  | 64 |  88000  |
| RabbitMQ  | 32 |  86000 |


ActiveMQ - 32 conn, 1900 rps, 17ms latency, **extremely** low error rate.  Fire & Forget 5800 rps
Kafka - 128 conn, 3300 rps, 41ms latency, very low error rate.  Fire & Forget 4300 rps
Proto.Actor - 32 conn, 3300 rps, 11ms latency, zero errors.  Fire & Forget not realistic
Pulsar - 128 conn, 3300 rps, 41ms latency, zero errors.  Fire & Forget 88000 rps
RabbitMQ - 32 conn, 1800 rps, 9ms latency, very low error rate.  Fire & Forget 86000 rps




ActiveMQ - lowest optimal connections (3), half the performance of others on average (200rps), with no clear advantage other than Java

# Roadmap
- Setup documentation
- Benchmarks
- Code cleanup
- Address support **(Genie)**
- Cashless transactions **(Genius)**
- KYC (Biometric ID) **(Genius)**
- KYC ID+++ **(Genius)**
- Multiplayer system **(Genius)**
- Reward system **(Genius)**
- Reporting **(Genius)**
- Leaderboard **(Genius)**
- Locator **(Genius)**
- Wigle **(Genius)**
- Bid system **(Genius)**
