using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Cryptography
{
    public enum SymmetricAlgorithms : int
    {
        AES = 0,
        DES = 1,
        RC2 = 2,
        Rijndael = 3,
        TripleDES = 4
    }
    public enum AsymmetricAlgorithms : int
    {
        DSA = 0,
        RSA = 1,
        ECDSA = 2,
        ECDiffieHellman = 3
    }
    public enum HashAlgorithms : int
    {
        MD5 = 0,
        RIPEMD160 = 1,
        SHA1 = 2,
        SHA256 = 3,
        SHA384 = 4,
        SHA512 = 5
    }
}
