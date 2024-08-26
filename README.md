
  

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
- PostGIS
- Confluent/Bitnami- Schema Registry and Kafka [docker-compose.yml](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/Genie.Benchmarks/docker-compose.yml) 
- [Crank](https://github.com/dotnet/crank) - Web benchmark [agent](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/Genie.Benchmarks/benchmark.yaml)

## Spatial Map Data
To do: include setup instructions for creating the PostGIS database

## Benchmarks
__All benchmarks were produced with Crank in **standalone mode**__ on a single node Intel 13980hx with 32GB DDR5 3200+.  IPC was avoided for all brokers supporting it.

__What's in  the benchmark__
 - Client: Serialize mock object #1 GRPC to Byte[]
 - Client: Send payload to broker
 - Client: If __NOT__ Fire & Forget, wait for server response.
	 - Server: Deserialize payload from client
	 - Server: Perform change data capture vs mock object #2
	 - Server: Perform PostGIS ST_Intersects query on OpenDataSoft postal code data
	 - Server: Send response to client (Avro serialization)
	 - Client: Deserialize and validate response from server

### Configuration
| Rank  | Broker   | License | Client| Configuration
|---|---|---|---|---|
1| [ActiveMQ](https://activemq.apache.org) | Apache v2 |[Apache.NMS.ActiveMQ](https://activemq.apache.org/components/nms/providers/activemq/) | ActiveMQ Artemis 2.36.0, Producer/Consumer
2| [Aeron](https://aeron.io)| Apache v2|[Aeron.NET](https://github.com/AdaptiveConsulting/Aeron.NET) | [Media Driver 1.40](https://github.com/AdaptiveConsulting/Aeron.NET/tree/master/driver), Pub/Sub
3| [Kafka](https://kafka.apache.org/) | Apache v2 | [Confluent.Kafka](https://github.com/confluentinc/confluent-kafka-dotnet) | Bitnami Kafka 3.7.1
4| [MQTT](https://mqtt.org/) |Apache v2 | [MQTTnet](https://github.com/dotnet/MQTTnet) | EMQX 5.7.2
5| [NATS](https://nats.io) | Apache v2 | [NATS .NET](https://github.com/nats-io/nats.net)| 2.10.18
6| [Proto.Actor](https://proto.actor) | Apache v2 | [Proto.Actor](https://github.com/asynkron/protoactor-dotnet) | 1.6, [Consul](https://www.consul.io/) mesh provider, virtual grains
7| [Pulsar](https://pulsar.apache.org) | Apache v2 | [Pulsar.Client](https://github.com/fsprojects/pulsar-client-dotnet) | 3.3.0
8| [RabbitMQ](https://www.rabbitmq.com/) | MPL 2.0 | [RabbitMQ.Client](https://github.com/rabbitmq/rabbitmq-dotnet-client) | 3.13.6
9| [ZeroMQ](https://zeromq.org) | MPL 2.0 | [NetMQ](https://github.com/zeromq/netmq) | 4.0.1.13, [Dealer/Dealer](https://sachabarbs.wordpress.com/2014/08/21/zeromq-2-the-socket-types-2/)

### Round trip
#### Baseline
| Broker   | Conn   | Req/sec  | Mean Lat (ms)   | Max Lat (ms)   | First Req (ms)   |
|---|---|---|---|---|---|
| None  | 1  | 22,738   | 0.04  | 9.06 | 347
| ZeroMQ  | 1  | 721 | 1.38 | 11.10 | 714
| Proto.Actor  | 1  | 687  | 1.44 | 13.68 | 428
 NATS| 1  | 492 | 2.03  | 60.88 | 744
| RabbitMQ  | 1  | 466   | 2.14  | 47.63 | 690
| ActiveMQ  | 1  | 408   | 2.45  | 51.38 | 952
| Pulsar  | 1  | 37   | 31.86  | 56.65 | 1,218
| Kafka  | 1  | 33   | 31.68  | 50.59 | 3,570
| MQTT| 1  | 22 | 50.00  | 69.14 | 766
| Aeron| 1  | 21 | 48.97  | 110.74 | 2,620

#### Scaled
| Broker   | Conn   | Range (Req/sec) | Req/sec  | Mean Lat (ms)   | Max Lat (ms)  | First Req (ms) | Bad Responses |
|---|---|---|---|---|---|---|---|
| ZeroMQ| 128| 3300-4000| 3,351   | 40.76  | 135 | 843
| Kafka  | 128  | 3100-3800 | 3,340   | 38.45  | 5,699 | 3,618 | 636 (1 hour), 192 (8 hours), possibly startup related
| Proto.Actor  | 32  | 3300-3700 | 3,329   | 10.20 | 93 | 548
| ZeroMQ| 64 | n/a |3,031   | 22.12  | 129 | 1,372
| NATS| 64 | 2400-2600 | 2,542   | 26.45  | 1,908 | 574
| RabbitMQ  | 32  | 1700-1900 | 1,889 | 8.5  | 1719 | 774 | 82 (1 hour) message corruption
| ActiveMQ  | 32  | 1700-1900 | 1,880   | 17.88  | 135 | 1,180 | 1 hour, no errors
| MQTT | 128| 1650-1750 |1,706  | 78.33  | 6,396 | 538 | Errors out < 10 min
| MQTT | 64| n/a |1,015   | 63.77  | 2,038 | 951 | 1 hour, no errors
| Pulsar  | 32  | 550 - 625 | 607| 55.18  | 184 | 1,472
| Aeron| | | | || | Duplicates and loses messages with multiple threads


### Fire & Forget
#### Baseline
| Broker   | Conn  | Req/sec | Mean Lat (ms) | Max Lat (ms) | First Req (ms)
|---|---|---|---|---|---|
| None  | 1  | 22,738   | 0.04  | 9.06 | 347
| Pulsar  | 1 |  18,213  | 0.05 | 8.04 | 1,014
| ZeroMQ*| 1 |  15,564  | 0.06 | 6.50 | 700
| RabbitMQ  | 1|  12,262 | 0.08 | 26.73 | 465
| Proto.Actor* | 1| 11,915  | 0.08 | 12.59 | 263
| Aeron | 1| 11,693  | 0.08 | 28.14 | 688
| NATS | 1| 10,355  | 0.09| 8.10 | 543
| ActiveMQ  | 1 | 10,259  | 0.10 | 6.65 | 848
| MQTT| 1 | 7,142   | 0.14 | 6.45 | 481
| Kafka  |  1 | 64 | 15.86 | 47.97 | 1,994

ZeroMQ and Proto.Actor have no persistence so it's a synthetic benchmark for comparison only

#### Scaled
| Broker   | Connections   | Req/sec | Mean Lat (ms) | Max Lat(ms) | First Req (ms)
|---|---|---|---|---|---|
| Pulsar  | 64 |  88,748  | 0.72 | 801 | 1,078
| RabbitMQ  | 32 |  78,543 | 0.40 | 296 | 543
| NATS | 128 | 76,144   | 1.63 | 292 | 580
| ActiveMQ  | 48 | 58,179   | 0.87 | 7,596 | 755
| MQTT| 128 | 51,975   | 2.42 | 198 | 496
| Aeron| 64 | 20.779   | 3.08 | 227 | 594
| Kafka  |  96 | 4,298 | 23.24 | 12,104 | 2,036

### Overall Ranking
| Rank  | Broker   | Opinion
|---|---|---|
|1 | Proto.Actor | Top overall performer. Ranks a close third in throughput but with 1/4 latency.  Virtual grains provide stateful possibilities. Requires no external dependencies for IPC.
|2 | ZeroMQ | Best overall performance ranking in roundtrip (1st), scaled (1st) and Fire & Forget (2nd).  Requires no external dependencies.
|3 | Kafka | High initial overhead is mitigated with great scalability to reach 2nd in overall throughput.  Suffers from a very low error count _(possibly startup related)_
|4 | NATS | Solid overall performer in both roundtrip and Fire & Forget throughput
|5 | Pulsar | Clear choice for Fire & Forget
|6 | ActiveMQ |Close in performance to RabbitMQ but also error free
|7 | RabbitMQ | Top performer for scaled latency in both roundtrip and Fire & Forget, second overall in Fire & Forget throughput. Suffers from occasional message corruption issues _(possibly memory related)_.
|8 | MQTT | EMQX wouldn't run at peak throughput (scaled, 128conn, 1700rps) for longer than 10 minutes.
|9 | Aeron |  Financial markets are reliant on low latency where trading is time-sensitive. Unfortunately it doesn't appear to have the throughput we need in an more generic event sourcing model.  Even according to the maintainers it [may not be the best option](https://github.com/AdaptiveConsulting/Aeron.NET/issues/59#issuecomment-278673122). **Suffered from message loss and duplication when scaled and the [service implementation](https://github.com/AdaptiveConsulting/Aeron.NET/tree/master/src/Samples/Adaptive.Aeron.Samples.ClusterService) returned messages with no payload**  


# Roadmap
- Setup documentation
- Benchmarks - ~~Broker~~, Spatial DB (Investigate Couch, Cockroach, Crate and Snowflake)
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
