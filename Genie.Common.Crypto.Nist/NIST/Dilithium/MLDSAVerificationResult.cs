#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Crypto.Common.PQC.Dilithium;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public class MLDSAVerificationResult : ICryptoResult
{
    public string ErrorMessage { get; }
    public bool Success => string.IsNullOrEmpty(ErrorMessage);
    
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public MLDSAVerificationResult() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public MLDSAVerificationResult(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}
