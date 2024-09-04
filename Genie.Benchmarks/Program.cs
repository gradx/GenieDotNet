using BenchmarkDotNet.Running;
using Genie.Benchmarks;
using Genie.Benchmarks.Benchmarks;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.Hash.ShaWrapper;
using NIST.CVP.ACVTS.Libraries.Crypto.Dilithium;
using NIST.CVP.ACVTS.Libraries.Crypto.Kyber;
using NIST.CVP.ACVTS.Libraries.Crypto.SHA.NativeFastSha;
using NIST.CVP.ACVTS.Libraries.Math.Entropy;
using NIST.CVP.ACVTS.Libraries.Math;
using System.Collections;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.PQC.Dilithium;
using Genie.Common.Crypto.Adapters.Curve25519;
using Genie.Common.Crypto.Adapters.Pqc;
using Org.BouncyCastle.Pqc.Crypto.Crystals.Kyber;
using Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium;


//var test = new PqcBenchmarks();
//test.X25519_Dilithium();
//test.X25519_Ed25519();
//test.Kyber_Ed25519();
//test.Kyber_Dilithium();

//Console.ReadLine();

//var alice = Ed448Adapter.GenerateKeyPair();

//File.WriteAllBytes("AliceEd448.key", Ed448Adapter.Instance.Export(alice.Private, true));
//File.WriteAllBytes("AliceEd448.cer", Ed448Adapter.ExportX509PublicCertificate(alice, "Genie").GetRawCertData());

//var bob = Ed448Adapter.GenerateKeyPair();

//File.WriteAllBytes("BobEd448.key", Ed448Adapter.Instance.Export(bob.Private, true));
//File.WriteAllBytes("BobEd448.cer", Ed448Adapter.ExportX509PublicCertificate(bob, "Genie").GetRawCertData());




//var results1 = BenchmarkRunner.Run<EncryptionBenchmarks>();
//var results2 = BenchmarkRunner.Run<PqcNetworkBenchmarks>();
var results3 = BenchmarkRunner.Run<EncryptionBenchmarks>();
