#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Crypto.Common.Hash.ShaWrapper;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides a means of retrieving a <see cref="ISha"/> instance.
/// </summary>
public interface IShaFactory
{
    /// <summary>
    /// Gets an <see cref="ISha"/> based on the <see cref="HashFunction"/>
    /// </summary>
    /// <param name="hashFunction">Used to determine the <see cref="ISha"/> instance to retrieve.</param>
    /// <returns></returns>
    ISha GetShaInstance(HashFunction hashFunction);

    IShake GetShakeInstance(HashFunction hashFunction);
    IShaMct GetShaMctInstance(HashFunction hashFunction, bool oddLength = false);
}
