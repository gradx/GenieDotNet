using System.Numerics;
using NIST.CVP.ACVTS.Libraries.Math;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Crypto.Common.Hash
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public class HashResult
    {
        public BitString Digest
        {
            get
            {
                if (_digest != null) return _digest;
#pragma warning disable CS8603 // Possible null reference return.
                if (_digestBytes == null) return null;
#pragma warning restore CS8603 // Possible null reference return.
                _digest = new BitString(_digestBytes);
                return _digest;
            }

            private set => _digest = value;
        }

        public string ErrorMessage { get; private set; }
        private BitString _digest;
        private readonly byte[] _digestBytes;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public HashResult(BitString digest)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            Digest = digest;
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public HashResult(string errorMessage)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            ErrorMessage = errorMessage;
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public HashResult(byte[] digest)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            _digestBytes = digest;
        }

        public bool Success => string.IsNullOrEmpty(ErrorMessage);

        public BigInteger ToBigInteger()
        {
            if (!Success)
            {
                return 0;
            }

            return Digest.ToPositiveBigInteger();
        }

        public override string ToString()
        {
            if (!Success)
            {
                return ErrorMessage;
            }

            return $"Digest: {Digest.ToHex()}";
        }
    }
}
