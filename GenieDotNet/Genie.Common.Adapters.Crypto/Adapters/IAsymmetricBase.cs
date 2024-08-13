﻿using Genie.Common.Types;

namespace Genie.Common.Crypto.Adapters;

public interface IAsymmetricBase
{
    T GenerateKeyPair<T>();
    T ImportX509<T>(byte[] x509);
    T Import<T>(GeoCryptoKey g);
}