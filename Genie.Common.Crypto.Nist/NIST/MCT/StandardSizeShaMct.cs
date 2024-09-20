using System;
using System.Collections.Generic;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.Hash;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.Hash.ShaWrapper;
using NIST.CVP.ACVTS.Libraries.Math;
using NIST.CVP.ACVTS.Libraries.Math.Domain;
using NLog;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Crypto.SHA.MCT
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public class StandardSizeShaMct : IShaMct
    {
        private readonly ISha _sha;
        private List<BitString> _digests;
        private int NUM_OF_RESPONSES = 100;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public StandardSizeShaMct(ISha sha)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            _sha = sha;
        }

        #region MonteCarloAlgorithm Pseudocode
        /*
        * Seed = random n bits, where n is digest size.
        * 
        * For 100 iterations (j = 0)
        *     MD[0] = MD[1] = MD[2] = Seed
        *     
        *     For 1000 iterations (i = 3)
        *         M[i] = MD[i-3] || MD[i-2] || MD[i-1]
        *         MD[i] = SHA(M[i])
        *         
        *     MD[j] = Seed = MD[1002]      (last MD from inner loop)
        */
        #endregion MonteCarloAlgorithm Pseudocode
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        public MctResult<AlgoArrayResponse> MctHash(BitString message, bool isSample = false, MathDomain domain = null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        {
          if (isSample)
          {
            NUM_OF_RESPONSES = 3;
          }
          
          var i = 0;
          var j = 0;
          
          var responses = new List<AlgoArrayResponse>();

          try
          {
            for (i = 0; i < NUM_OF_RESPONSES; i++)
            {
#pragma warning disable CS8604 // Possible null reference argument.
              BitString innerMessage = ResetDigestList(message);
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
              BitString innerDigest = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

              var iterationResponse = new AlgoArrayResponse { Message = innerMessage };

              for (j = 0; j < 1000; j++)
              {
                var innerResult = _sha.HashMessage(innerMessage);
                innerDigest = innerResult.Digest;
                AddDigestToList(innerDigest);
                innerMessage = GetNextMessage();
              }

#pragma warning disable CS8601 // Possible null reference assignment.
              iterationResponse.Digest = innerDigest;
#pragma warning restore CS8601 // Possible null reference assignment.
              responses.Add(iterationResponse);
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
              message = innerDigest;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }
          }
          catch (Exception ex)
          {
            ThisLogger.Debug($"i count {i}, j count {j}");
            ThisLogger.Error(ex);
            return new MctResult<AlgoArrayResponse>(ex.Message);
          }

          return new MctResult<AlgoArrayResponse>(responses);
        }

        private BitString ResetDigestList(BitString message)
        {
            _digests = [message, message, message];

          return GetNextMessage();
        }

        private BitString GetNextMessage()
        {
          return BitString.ConcatenateBits(_digests[0],
            BitString.ConcatenateBits(_digests[1], _digests[2]));
        }

        private void AddDigestToList(BitString newDigest)
        {
            _digests.RemoveAt(0);
            _digests.Add(newDigest);
        }

        private static Logger ThisLogger => LogManager.GetCurrentClassLogger();
    }
}
