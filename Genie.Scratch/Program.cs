﻿using Genie.Actors;
using Genie.Common;
using Genie.Extensions.Genius;
using Proto.Cluster;
using Genie.Common.Utils;
using Genie.Common.Adapters;
using Microsoft.Extensions.ObjectPool;
using Genie.Scratch.Benchmarks.Databases;
using Genie.Adapters.Persistence.Postgres;

//await LoadPostal.StartMongo();
//var test = new MongoTest(4000, new DefaultObjectPool<MongoPooledObject<PersistenceTest>>(new DefaultPooledObjectPolicy<MongoPooledObject<PersistenceTest>>()));


await Read.Arango();

//Pqc.DilithiumExample();

Console.WriteLine("Here");
Console.ReadLine();


//const string baseUrl = "https://luxur.ai:5003";`

//await LoadElastic.Test();


var geo = GeometryCalculator.Polygon(38.89781822004474, -77.03655126065402, 10, 4);

var pool = new DefaultObjectPool<PostgresPooledObject>(new DefaultPooledObjectPolicy<PostgresPooledObject>());
MapAdapter.ReverseGeoCode(pool, geo, []);

//await LoadPostgis.Start();

var context = GenieContext.Build().GenieContext;

var geniusSystem = ActorUtils.JoinActorSystem(context.Actor.ClusterName, context.Actor.ConsulProvider, [GeniusGrainReflection.Descriptor]);

await geniusSystem.Cluster().StartMemberAsync();


//string value = db.StringGet("mykey");

//Console.WriteLine(value); // writes: "abcdefg"

Console.WriteLine("Wait for Instructions");
while (Console.ReadLine() != null)
{
    var geniusClient = geniusSystem.Cluster().GetGeniusService("help");
    await geniusClient.Process(new GeniusRequest(), new CancellationToken());
}
