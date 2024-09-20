using System;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.Hash.ShaWrapper;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.Hash.ShaWrapper.Enums;
using NIST.CVP.ACVTS.Libraries.Math;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Crypto.Common.PQC.Helpers;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public class PreHashProperties
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public PreHashProperties()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        // PreHash = false
    }
    
    public PreHashProperties(ModeValues mode, DigestSizes digest)
    {
        // Need the last flag to be true to indicate the hash is used for signing
        PreHashFunction = new HashFunction(mode, digest, true);
    }

    public bool PreHash => PreHashFunction != null;
    private HashFunction PreHashFunction { get; }

    public byte[] OID
    {
        get
        {
#pragma warning disable CS8603 // Possible null reference return.
            return PreHash ? PreHashFunction.OID : null;
#pragma warning restore CS8603 // Possible null reference return.
        }
    }

    public int HashOutputLength
    {
        get
        {
            return PreHash ? PreHashFunction.OutputLen : 0;
        }
    }
}
