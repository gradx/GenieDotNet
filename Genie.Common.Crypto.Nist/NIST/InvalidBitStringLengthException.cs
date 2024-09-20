using System;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Math.Exceptions
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    /// <summary>
    /// Thrown when a <see cref="BitString"/> as hex cannot be parsed due to invalid length.
    /// </summary>
    public class InvalidBitStringLengthException : Exception
    {
#pragma warning disable IDE0290 // Use primary constructor
        public InvalidBitStringLengthException(string message)
#pragma warning restore IDE0290 // Use primary constructor
            : base(message)
        {

        }
    }
}
