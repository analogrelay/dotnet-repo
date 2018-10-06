using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DotNet.Repo.Build
{
    public class StrongNameModule
    {
        private static readonly Lazy<Task<string>> _moduleProps = new Lazy<Task<string>>(() => ResourceFiles.LoadResourceFile("Modules", "StrongName", "module.props"));
        private static readonly Lazy<Task<string>> _readme = new Lazy<Task<string>>(() => ResourceFiles.LoadResourceFile("Modules", "StrongName", "README.md"));
        private readonly ILogger<StrongNameModule> _logger;

        public StrongNameModule(ILogger<StrongNameModule> logger)
        {
            _logger = logger;
        }


        public async Task InstallAsync(string moduleDirectory)
        {
            _logger.LogInformation("Installing Strong Naming build module...");
            if (!Directory.Exists(moduleDirectory))
            {
                Directory.CreateDirectory(moduleDirectory);
            }

            // Drop the build module
            var propsFile = Path.Combine(moduleDirectory, "module.props");
            var readmeFile = Path.Combine(moduleDirectory, "README.md");
            var snkFile = Path.Combine(moduleDirectory, "StrongNameKey.snk");

            _logger.LogDebug("Generating strong name key...");
            var key = GenerateStrongNameKey();

            _logger.LogDebug("Installing strong naming");
            await File.WriteAllTextAsync(propsFile, await _moduleProps.Value);
            await File.WriteAllTextAsync(readmeFile, await _readme.Value);
            await File.WriteAllBytesAsync(snkFile, key);
        }

        private byte[] GenerateStrongNameKey()
        {
            // Generate a 4096-bit RSA Signing Key and export the parameters
            var rsa = new RSACryptoServiceProvider(4096);
            var parameters = rsa.ExportParameters(includePrivateParameters: true);

            var byteLen = rsa.KeySize / 8;

            // Write the content
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);

            // The file contains a "Private Key Blob" as defined in
            // https://docs.microsoft.com/en-us/windows/desktop/seccrypto/base-provider-key-blobs#private-key-blobs

            // Format:
            //  PUBLICKEYSTRUC  publickeystruc;
            //  RSAPUBKEY rsapubkey;
            //  BYTE modulus[rsapubkey.bitlen/8];
            //  BYTE prime1[rsapubkey.bitlen/16];
            //  BYTE prime2[rsapubkey.bitlen/16];
            //  BYTE exponent1[rsapubkey.bitlen/16];
            //  BYTE exponent2[rsapubkey.bitlen/16];
            //  BYTE coefficient[rsapubkey.bitlen/16];
            //  BYTE privateExponent[rsapubkey.bitlen/8];

            // PUBLICKEYSTRUC - https://docs.microsoft.com/en-us/windows/desktop/api/wincrypt/ns-wincrypt-_publickeystruc
            // typedef struct _PUBLICKEYSTRUC {
            //   BYTE   bType;
            //   BYTE   bVersion;
            //   WORD   reserved;
            //   ALG_ID aiKeyAlg;
            // } BLOBHEADER, PUBLICKEYSTRUC;
            writer.Write(CryptoUtils.PRIVATEKEYBLOB);
            writer.Write(CryptoUtils.BlobVersion);
            writer.Write((ushort)0); // Reserved
            writer.Write(CryptoUtils.CALG_RSA_SIGN);

            // RSAPUBKEY - https://docs.microsoft.com/en-us/windows/desktop/api/wincrypt/ns-wincrypt-_rsapubkey
            // typedef struct _RSAPUBKEY {
            //   DWORD magic;
            //   DWORD bitlen;
            //   DWORD pubexp;
            // } RSAPUBKEY;
            writer.Write(CryptoUtils.MagicNumberRSA2);
            writer.Write(rsa.KeySize); // Bit length of key
            writer.Write(BytesToDword(parameters.Exponent));

            // Assert the size of the RSA parameters and write them
            WriteReversed(writer, parameters.Modulus);
            WriteReversed(writer, parameters.P);
            WriteReversed(writer, parameters.Q);
            WriteReversed(writer, parameters.DP);
            WriteReversed(writer, parameters.DQ);
            WriteReversed(writer, parameters.InverseQ);
            WriteReversed(writer, parameters.D);

            writer.Flush();
            return ms.ToArray();
        }

        private static uint BytesToDword(ReadOnlySpan<byte> bytes)
        {
            // Yoinked from https://source.dot.net/#System.Security.Cryptography.Csp/System/Security/Cryptography/CapiHelper.Shared.cs,555c34d97d4628dc
            uint dword = 0;
            for (var i = 0; i < bytes.Length; i++)
            {
                dword <<= 8;
                dword |= bytes[i];
            }
            return dword;
        }

        /// <summary>
        /// Write out a byte array in reverse order.
        /// Borrowed from https://source.dot.net/#System.Security.Cryptography.Csp/System/Security/Cryptography/CapiHelper.Shared.cs,cb2a50d4837d3db5
        /// </summary>
        private static void WriteReversed(BinaryWriter bw, byte[] bytes)
        {
            Array.Reverse(bytes);
            bw.Write(bytes);
            return;
        }
    }
}
