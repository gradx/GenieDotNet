using NetTopologySuite.Utilities;
using Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium;
using Org.BouncyCastle.Pqc.Crypto.Crystals.Kyber;
using Org.BouncyCastle.Pqc.Crypto.Saber;
using Org.BouncyCastle.Pqc.Crypto.Utilities;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities;
using Utf8StringInterpolation;
using Genie.Common.Types;
using Genie.Common.Crypto.Adapters.Pqc;

namespace Genie.Scratch.Quantum
{
    public class Pqc
    {
        public static void Test()
        {
            var random = new SecureRandom();

            // generating key pair
            var kpGenParams = new KyberKeyGenerationParameters(random, KyberParameters.kyber768);
            var kpGen = new KyberKeyPairGenerator();
            kpGen.Init(kpGenParams);
            var keyPair = kpGen.GenerateKeyPair();

            // parsing out private and public params
            var privateParams = (KyberPrivateKeyParameters)keyPair.Private;
            var publicParams = (KyberPublicKeyParameters)keyPair.Public;

            // START - faux deserialization for comparison
            // var privPkcs8 = PqcPrivateKeyInfoFactory.CreatePrivateKeyInfo(privateParams).GetEncoded();
            var privPkcs8 = PqcPrivateKeyInfoFactory.CreatePrivateKeyInfo(privateParams).ToAsn1Object().GetDerEncoded();
            // var pubPkcs8 = PqcSubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicParams).GetEncoded();
            var pubPkcs8 = PqcSubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicParams).ToAsn1Object().GetDerEncoded();

            var privPkcs8InB64 = Convert.ToBase64String(privPkcs8);
            var pubPkcs8InB64 = Convert.ToBase64String(pubPkcs8);

            var privateParams2 = (KyberPrivateKeyParameters)PqcPrivateKeyFactory.CreateKey(Convert.FromBase64String(privPkcs8InB64));
            var publicParams2 = (KyberPublicKeyParameters)PqcPublicKeyFactory.CreateKey(Convert.FromBase64String(pubPkcs8InB64));

            Console.WriteLine($"Public Comparison: {Arrays.AreEqual(publicParams.GetEncoded(), publicParams2.GetEncoded())}");
            Console.WriteLine($"Private Comparison: {Arrays.AreEqual(privateParams.GetEncoded(), privateParams2.GetEncoded())}");
            // END - faux deserialization for comparison

            // generate shared secret and encapsulated cipher text
            var kemGenerator = new KyberKemGenerator(random);
            var secretWithEncapsulation = kemGenerator.GenerateEncapsulated(publicParams2);
            var rawSecret = secretWithEncapsulation.GetSecret();
            var cipherText = secretWithEncapsulation.GetEncapsulation();

            // make extractor with private params and get secret
            var kemExtractor = new KyberKemExtractor(privateParams2);
            var extractedSecret = kemExtractor.ExtractSecret(cipherText);

            Console.WriteLine($"\n\nGenerated Symmetric Key: {Hex.ToHexString(rawSecret)}\nRetrieved Symmetric Key: {Hex.ToHexString(extractedSecret)}");

            var secretsEqual = Arrays.AreEqual(rawSecret, extractedSecret);
            Console.WriteLine($"Secrets Equal ---> {secretsEqual}");
        }

        public static void DilithiumExample()
        {
            var kp = DilithiumAdapter.GenerateKeyPair();
            var priv = (DilithiumPrivateKeyParameters)kp.Private;
            var pub = (DilithiumPublicKeyParameters)kp.Public;

            var data = Utf8String.Format($"Help");
            var signed = DilithiumAdapter.Instance.Sign(data, priv);

            var verify = DilithiumAdapter.Instance.Verify(data, signed, pub);

            var export_private = DilithiumAdapter.Instance.Export(kp.Private, true);
            var import_private = DilithiumAdapter.Import(new GeoCryptoKey { IsPrivate = true, X509 = export_private });

            var export_public = DilithiumAdapter.Instance.Export(kp.Public, false);
            var import_public = DilithiumAdapter.Import(new GeoCryptoKey { IsPrivate = false, X509 = export_public });


            var alice = DilithiumAdapter.GenerateKeyPair();
            var bob = DilithiumAdapter.GenerateKeyPair();
            File.WriteAllBytes("AliceKyber.key", DilithiumAdapter.Instance.Export(alice.Private, true));
            File.WriteAllBytes("AliceKyber.cer", DilithiumAdapter.Instance.Export(alice.Public, false));
            File.WriteAllBytes("BobKyber.key", DilithiumAdapter.Instance.Export(bob.Private, true));
            File.WriteAllBytes("BobKyber.cer", DilithiumAdapter.Instance.Export(bob.Public, false));

        }

        public static void KyberExample()
        {
            var alice = KyberAdapter.GenerateKeyPair();
            var bob = KyberAdapter.GenerateKeyPair();


            var alice_public = (KyberPublicKeyParameters)alice.Public;
            var bob_private = (KyberPrivateKeyParameters)bob.Private;
            var bob_secret = KyberAdapter.GenerateSecret(alice_public);


            // For Alice
            var encap = bob_secret.GetEncapsulation();
            var aliceKemExtractor = new KyberKemExtractor((KyberPrivateKeyParameters)alice.Private);
            var aliceSecret = aliceKemExtractor.ExtractSecret(encap);
            Assert.IsTrue(aliceSecret.SequenceEqual(bob_secret.GetSecret()));


            var export_private = KyberAdapter.Export(alice.Private);
            var import_private = KyberAdapter.Import(new GeoCryptoKey { IsPrivate = true, X509 = export_private });

            var export_public = KyberAdapter.Export(alice.Public);
            var import_public = KyberAdapter.Import(new GeoCryptoKey { IsPrivate = false, X509 = export_public });


            //File.WriteAllBytes("AliceKyber.key", KyberAdapter.Export(alice.Private));
            //File.WriteAllBytes("AliceKyber.cer", KyberAdapter.Export(alice.Public));
            //File.WriteAllBytes("BobKyber.key", KyberAdapter.Export(bob.Private));
            //File.WriteAllBytes("BobKyber.cer", KyberAdapter.Export(bob.Public));

            // // System.ArgumentException: 'Class provided no convertible: Org.BouncyCastle.Pqc.Crypto.Crystals.Kyber.KyberPublicKeyParameters'
            //var export_cert = KyberAdapter.ExportX509PublicCertificate(alice, "Help");
        }

        public static void KyberExample2()
        {
            var random = new SecureRandom();

            // generate key pair
            var saberGenParams = new KyberKeyGenerationParameters(random, KyberParameters.kyber768);
            var saberGen = new KyberKeyPairGenerator();
            saberGen.Init(saberGenParams);
            var keyPair = saberGen.GenerateKeyPair();

            // parse out public and private
            var privateParams = (KyberPrivateKeyParameters)keyPair.Private;
            var publicParams = (KyberPublicKeyParameters)keyPair.Public;

            // faux deserialization
            var privPkcs8 = PqcPrivateKeyInfoFactory.CreatePrivateKeyInfo(privateParams).GetEncoded();
            var pubPkcs8 = PqcSubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicParams).GetEncoded();

            var privPkcs8InB64 = Convert.ToBase64String(privPkcs8);
            var pubPkcs8InB64 = Convert.ToBase64String(pubPkcs8);

            var privateParams2 = (KyberPrivateKeyParameters)PqcPrivateKeyFactory.CreateKey(Convert.FromBase64String(privPkcs8InB64));
            var publicParams2 = (KyberPublicKeyParameters)PqcPublicKeyFactory.CreateKey(Convert.FromBase64String(pubPkcs8InB64));

            Console.WriteLine($"Public Comparison: {Arrays.AreEqual(publicParams.GetEncoded(), publicParams2.GetEncoded())}");
            Console.WriteLine($"Private Comparison: {Arrays.AreEqual(privateParams.GetEncoded(), privateParams2.GetEncoded())}");

            // generate shared secret and encapsulated cipher text
            var kemGenerator = new KyberKemGenerator(random);
            var secretWithEncapsulation = kemGenerator.GenerateEncapsulated(publicParams2);
            var rawSecret = secretWithEncapsulation.GetSecret();
            var cipherText = secretWithEncapsulation.GetEncapsulation();

            // make extractor with private params and get secret
            var kemExtractor = new KyberKemExtractor(privateParams2);
            var extractedSecret = kemExtractor.ExtractSecret(cipherText);

            Console.WriteLine($"\n\nGenerated Symmetric Key: {Hex.ToHexString(rawSecret)}\nRetrieved Symmetric Key: {Hex.ToHexString(extractedSecret)}");

            var secretsEqual = Arrays.AreEqual(rawSecret, extractedSecret);
            Console.WriteLine($"Secrets Equal ---> {secretsEqual}");
        }

        public static void SaberExample()
        {
            var random = new SecureRandom();

            // generate key pair
            var saberGenParams = new SaberKeyGenerationParameters(random, SaberParameters.firesaberkem256r3);
            var saberGen = new SaberKeyPairGenerator();
            saberGen.Init(saberGenParams);
            var keyPair = saberGen.GenerateKeyPair();

            // parse out public and private
            var privateParams = (SaberPrivateKeyParameters)keyPair.Private;
            var publicParams = (SaberPublicKeyParameters)keyPair.Public;

            // faux deserialization
            var privPkcs8 = PqcPrivateKeyInfoFactory.CreatePrivateKeyInfo(privateParams).ToAsn1Object().GetDerEncoded();
            var pubPkcs8 = PqcSubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicParams).ToAsn1Object().GetDerEncoded();

            var privPkcs8InB64 = Convert.ToBase64String(privPkcs8);
            var pubPkcs8InB64 = Convert.ToBase64String(pubPkcs8);

            var privateParams2 = (SaberPrivateKeyParameters)PqcPrivateKeyFactory.CreateKey(Convert.FromBase64String(privPkcs8InB64));
            var publicParams2 = (SaberPublicKeyParameters)PqcPublicKeyFactory.CreateKey(Convert.FromBase64String(pubPkcs8InB64));

            Console.WriteLine($"Public Comparison: {Arrays.AreEqual(publicParams.GetEncoded(), publicParams2.GetEncoded())}");
            Console.WriteLine($"Private Comparison: {Arrays.AreEqual(privateParams.GetEncoded(), privateParams2.GetEncoded())}");

            // generate shared secret and encapsulated cipher text
            var kemGenerator = new SaberKemGenerator(random);
            var secretWithEncapsulation = kemGenerator.GenerateEncapsulated(publicParams2);
            var rawSecret = secretWithEncapsulation.GetSecret();
            var cipherText = secretWithEncapsulation.GetEncapsulation();

            // make extractor with private params and get secret
            var kemExtractor = new SaberKemExtractor(privateParams2);
            var extractedSecret = kemExtractor.ExtractSecret(cipherText);

            Console.WriteLine($"\n\nGenerated Symmetric Key: {Hex.ToHexString(rawSecret)}\nRetrieved Symmetric Key: {Hex.ToHexString(extractedSecret)}");

            var secretsEqual = Arrays.AreEqual(rawSecret, extractedSecret);
            Console.WriteLine($"Secrets Equal ---> {secretsEqual}");
        }

    }
}
