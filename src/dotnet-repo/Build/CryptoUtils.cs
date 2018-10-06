namespace DotNet.Repo.Build
{
    internal class CryptoUtils
    {
        public const byte PRIVATEKEYBLOB = 0x07;
        public const byte BlobVersion = 0x02;
        public const uint CALG_RSA_SIGN = 0x00002400;
        public const uint MagicNumberRSA2 = 0x32415352; // https://docs.microsoft.com/en-us/windows/desktop/api/wincrypt/ns-wincrypt-_rsapubkey
    }
}
