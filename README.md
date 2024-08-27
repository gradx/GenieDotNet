

  

# GenieDotNet

GenieDotNet consists of 3 main components to showcase how to create a (1) secure, (2) cloud native, (3) scalable, (4) distributed, (5) [high performance](https://learn.microsoft.com/en-us/aspnet/core/performance/objectpool?view=aspnetcore-8.0) (6) inference engine and (7) extensible (8) real-time streaming processor with a license provisioning [example](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/Genie.Extensions.Genius/GeniusGrain.cs#L84) using C# and .NET 8
complete with [Crank](https://github.com/dotnet/crank) benchmarks [here](https://github.com/gradx/GenieDotNet/blob/main/GenieDotNet/Genie.Benchmarks/benchmark.yaml)

# Genie
Geospatial Event Network Inference Engine - (Event-Sourced Microservice Broker) Reverse Geocoder
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
 - Client: Serialize mock object #1 GRPC to Byte[] _(Kafka uses the [Avro](https://github.com/ch-robinson/dotnet-avro) serializer for other testing purposes)_
 - Client: Send payload to broker
 - Client: If __NOT__ Fire & Forget, wait for server response.
	 - Server: Deserialize payload from client
	 - Server: Perform change data capture vs mock object #2
	 - Server: Perform PostGIS ST_Intersects query on OpenDataSoft postal code data
	 - Server: Send response to client (Avro serialization)
	 - Client: Deserialize and validate response from server

### Configuration
Startup [Docker](https://github.com/gradx/GenieDotNet/tree/main/GenieDotNet/Docker) files are located here
 | Broker   | License | Client| Configuration
|---|---|---|---|---|
 [ActiveMQ](https://activemq.apache.org) | Apache v2 |[Apache.NMS.ActiveMQ](https://activemq.apache.org/components/nms/providers/activemq/) | ActiveMQ Artemis 2.36.0, Producer/Consumer
 [Aeron](https://aeron.io)| Apache v2|[Aeron.NET](https://github.com/AdaptiveConsulting/Aeron.NET) | [Media Driver 1.40](https://github.com/AdaptiveConsulting/Aeron.NET/tree/master/driver), Pub/Sub
[Kafka](https://kafka.apache.org/) | Apache v2 | [Confluent.Kafka](https://github.com/confluentinc/confluent-kafka-dotnet) | Bitnami Kafka 3.7.1
[MQTT](https://mqtt.org/) |Apache v2 | [MQTTnet](https://github.com/dotnet/MQTTnet) | EMQX 5.7.2
[NATS](https://nats.io) | Apache v2 | [NATS .NET](https://github.com/nats-io/nats.net)| 2.10.18
[Proto.Actor](https://proto.actor) | Apache v2 | [Proto.Actor](https://github.com/asynkron/protoactor-dotnet) | 1.6, [Consul](https://www.consul.io/) mesh provider, virtual actors
 [Pulsar](https://pulsar.apache.org) | Apache v2 | [Pulsar.Client](https://github.com/fsprojects/pulsar-client-dotnet) | 3.3.0
[RabbitMQ](https://www.rabbitmq.com/) | MPL 2.0 | [RabbitMQ.Client](https://github.com/rabbitmq/rabbitmq-dotnet-client) | 3.13.6
[Redpanda](https://www.rabbitmq.com/) | BSL | [Confluent.Kafka](https://github.com/confluentinc/confluent-kafka-dotnet) | 24.2.2
[ZeroMQ](https://zeromq.org) | LGPL | [NetMQ](https://github.com/zeromq/netmq) | 4.0.1.13, [Dealer/Dealer](https://sachabarbs.wordpress.com/2014/08/21/zeromq-2-the-socket-types-2/)

### Round trip
#### Baseline
| Broker   | Conn   | Req/sec  | Mean Lat (ms)   | p99 Lat (ms)   | First Req (ms)   |
|---|---|---|---|---|---|
| None  | 1  | 22,738   | 0.04  | 0.55 | 347
| ZeroMQ  | 1  | 723 | 1.38 | 2.55 | 714
| Proto.Actor  | 1  | 687  | 1.44 | 2.70 | 428
 NATS| 1  | 492 | 2.03  | 4.22 | 744
| RabbitMQ  | 1  | 466   | 2.14  | 4.23 | 690
| ActiveMQ  | 1  | 408   | 2.45  | 51.38 | 952
| Pulsar  | 1  | 37   | 31.86  | 40.87 | 1,218
| MQTT| 1  | 22 | 50.00  | 65.47 | 766
| Aeron| 1  | 21 | 48.97  | 64.22 | 2,620
| Redpanda| 1  | 17   | 64.67  | 222.95 | 2,791
| Kafka| 1  | 16   | 66.54  | 264.75 | 4,256

#### Scaled
| Broker   | Conn   | Range (Req/sec) | Req/sec  | Mean Lat (ms)   | p99 Lat (ms)  | First Req (ms) | Bad Responses |
|---|---|---|---|---|---|---|---|
| ZeroMQ| 128| 3300-4000| 3,207   |42.14  | 72.92| 1,128
| Proto.Actor  | 32  | 3300-3700 | 3,288  | 10.35 | 25.91 | 610
| ZeroMQ| 64 | n/a |3,031   | 22.12  | 37.28| 1,372
| NATS| 64 | 2400-2600 | 2,572   | 26.20  | 44.21 | 762
| RabbitMQ  | 32  | 1700-1900 | 1,825 | 17.97  | 34.07 | 881 | Uses Avro serialier since Google Protobuf suffers from intermittent corrupted messages
| ActiveMQ  | 32  | 1700-1900 | 1,890   | 17.80  | 65.56 | 1,255 | 1 hour, no errors but does suffer from sporatic issues
| MQTT | 128| 1650-1750 |1,706  | 78.33  | 80.38 | 538 | Errors out < 10 min
| Kafka| 32| 1300-1450 | 1,390   | 23.82  | 39.97 | 3,998
| MQTT | 64| n/a |1,011   | 63.97  | 80.15 | 908 | 1 hour, no errors
| Pulsar  | 32  | 550 - 625 | 540| 61.28  | 119.26 | 1,292
| Aeron| | | | || | Duplicates and loses messages with multiple threads


### Fire & Forget
#### Baseline
| Broker   | Conn  | Req/sec | Mean Lat (ms) | p99 Lat (ms) | First Req (ms)
|---|---|---|---|---|---|
| None  | 1  | 22,738   | 0.04  | 0.55 | 347
| Pulsar  | 1 |  18,213  | 0.05 | 0.54 | 1,014
| ZeroMQ*| 1 |  15,564  | 0.06 | 0.55 | 700
| RabbitMQ  | 1|  12,262 | 0.08 | 0.55 | 465
| Proto.Actor* | 1| 11,915  | 0.08 |  | 263
| Aeron | 1| 11,693  | 0.08 | 0.55 | 688
| NATS | 1| 10,355  | 0.09| 0.55 | 543
| ActiveMQ  | 1 | 10,259  | 0.10 | 0.55 | 848
| MQTT| 1 | 7,142   | 0.14 | 0.55 | 481
| Redpanda |  1 | 65 | 15.79 | 22.62 | 971
| Kafka|  1 | 29 | 36.38 | 170.65 | 2,362

ZeroMQ and Proto.Actor have no persistence so it's a synthetic benchmark for comparison only

#### Scaled
| Broker   | Connections   | Req/sec | Mean Lat (ms) | p99 Lat(ms) | First Req (ms)
|---|---|---|---|---|---|
| Pulsar  | 64 |  88,748  | 0.72 | 3.17 | 1,078
| RabbitMQ  | 32 |  78,543 | 0.40 | 7.55 | 543
| NATS | 96 | 73,025   | 1.28 | 7.59 | 473
| ActiveMQ  | 48 | 58,179   | 0.87 | 4.55 | 755
| MQTT| 128 | 51,975   | 2.42 | 8.19 | 496
| Aeron| 64 | 20.779   | 3.08 | 11.55 | 594
| Redpanda |  96 | 4,214 | 23.90 | 24.53 | 1,278
| Kafka |  96 | 4,130 | 24.72 | 46.86 | 2,699

### Overall Ranking
| Rank  | Broker   | Opinion
|---|---|---|
|1 | Proto.Actor | Top overall performer. Ranks a close third in throughput but with 1/4 latency.  Virtual actors provide stateful possibilities. Requires no external dependencies for IPC.
|2 | ZeroMQ | Best overall performance ranking in roundtrip (1st), scaled (1st) and Fire & Forget (2nd).  Requires no external dependencies.
|3 | Zlogger | High initial overhead is mitigated with great scalability to reach 2nd in overall throughput.  Suffers from a very low error count _(possibly startup related)_
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
