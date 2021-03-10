[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/samuel-lucas6/Geralt/blob/main/LICENSE)

# ChaCha20-BLAKE2b
An AEAD implementation using [libsodium](https://doc.libsodium.org/). This library supports both [ChaCha20](https://doc.libsodium.org/advanced/stream_ciphers/chacha20) and [XChaCha20](https://doc.libsodium.org/advanced/stream_ciphers/xchacha20).

## Should I use this?
*It depends*. This implementation has not been standardised like [ChaCha20-Poly1305](https://tools.ietf.org/html/rfc7539). If that is important to you, then obviously avoid using this.

However, ChaCha20-BLAKE2b is essentially Encrypt-then-MAC with keyed BLAKE2b rather than HMAC. To put things in perspective, the [Signal protocol](https://www.signal.org/docs/specifications/doubleratchet/#recommended-cryptographic-algorithms) uses Encrypt-then-MAC with AES-CBC and HMAC.

## Why should I use this?
This library does several things for you:

- Derives a unique 256-bit encryption key and 512-bit MAC key based on the (master) key and nonce.
- Appends the authentication tag to the ciphertext.
- Compares the authentication tags in constant time during decryption and only returns plaintext if they match.

## What is wrong with ChaCha20-Poly1305?
1. ChaCha20-Poly1305 is not key committing, meaning it is possible to decrypt a ciphertext using [multiple keys](https://eprint.iacr.org/2020/1491.pdf). The recommended approach for avoiding this problem (zero padding) has to be manually implemented, is potentially vulnerable to timing attacks, and will slow down decryption.
2. Poly1305 produces a 128-bit tag, which is rather short. The recommended hash length is typically 256-bit because it offers 128-bit security.

## How does ChaCha20-BLAKE2b solve these problems?
1. This implementation is key committing because it uses keyed BLAKE2b and both the encryption key and MAC key are derived from the same master key.
2. This implementation defaults to a 256-bit tag but also supports 128-bit and 512-bit tags. Using a longer tag offers improved security.

## How do I use this?
1. Install the [Sodium.Core](https://www.nuget.org/packages/Sodium.Core) NuGet package in [Visual Studio](https://docs.microsoft.com/en-us/nuget/quickstart/install-and-use-a-package-in-visual-studio).
2. Download the latest [release](https://github.com/samuel-lucas6/ChaCha20-BLAKE2b/releases).
3. Move the downloaded DLL file into your Visual Studio project folder.
3. Click on the ```Project``` tab and ```Add Project Reference...``` in Visual Studio.
4. Go to ```Browse```, click the ```Browse``` button, and select the downloaded DLL file.

### ChaCha20
⚠️**WARNING: Never reuse a nonce with the same key.**
```c#
const string message = "This is a test.";
const int nonceLength = 8;
const int keyLength = 32;
const int version = 1;

// The message could be a file
byte[] message = Encoding.UTF8.GetBytes(message);

// The nonce should be a counter that gets incremented for each message encrypted using the same key
byte[] nonce = new byte[nonceLength];

// The key can be random or derived using a KDF (e.g. Argon2, HKDF, etc)
byte[] key = SodiumCore.GetRandomBytes(keyLength);

// The additional data can be null or version numbers, timestamps, etc
byte[] additionalData = BitConverter.GetBytes(version);

// Encrypt the message and use a 256-bit authentication tag
byte[] ciphertext = ChaCha20BLAKE2b.Encrypt(message, nonce, key, additionalData, TagLength.Medium);

// Decrypt the ciphertext
byte[] plaintext = ChaCha20BLAKE2b.Decrypt(ciphertext, nonce, key, additionalData, TagLength.Medium);
```

### XChaCha20
⚠️**WARNING: Never reuse a nonce with the same key.**
```c#
const string message = "This is a test.";
const int nonceLength = 24;
const int keyLength = 32;
const int version = 1;

// The message could be a file
byte[] message = Encoding.UTF8.GetBytes(message);

// The nonce can be random. Increment the nonce for each message encrypted using the same key
byte[] nonce = SodiumCore.GetRandomBytes(nonceLength);

// The key can be random or derived using a KDF (e.g. Argon2, HKDF, etc)
byte[] key = SodiumCore.GetRandomBytes(keyLength);

// The additional data can be null or version numbers, timestamps, etc
byte[] additionalData = BitConverter.GetBytes(version);

// Encrypt the message and use a 512-bit authentication tag
byte[] ciphertext = XChaCha20BLAKE2b.Encrypt(message, nonce, key, additionalData, TagLength.Long);

// Decrypt the ciphertext
byte[] plaintext = XChaCha20BLAKE2b.Decrypt(ciphertext, nonce, key, additionalData, TagLength.Long);
```

## How fast is it?
ChaCha20-BLAKE2b is slower than regular ChaCha20-Poly1305, but when you implement the [padding fix](https://eprint.iacr.org/2020/1491.pdf) to add key commitment, the decryption time is roughly the same. Furthermore, if you take into account the longer tag length, then it is a worthwhile tradeoff.

The following benchmarks were done using [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet/) in a .NET Core 3.1 console application with a 34.1 MiB JPG file as the message, 16 bytes of additional data, and a 256-bit tag length:
|                   Function |     Mean |    Error |   StdDev |
|------------------------- |----------|----------|----------|
| ChaCha20-BLAKE2b.Encrypt | 70.39 ms | 1.373 ms | 1.410 ms |
| ChaCha20-BLAKE2b.Decrypt | 71.10 ms | 1.102 ms | 1.031 ms |
| ChaCha20-Poly1305.Encrypt | 54.24 ms | 0.143 ms | 0.127 ms |
| ChaCha20-Poly1305.Decrypt | 54.76 ms | 0.416 ms | 0.389 ms |
| ChaCha20-Poly1305.Encrypt (with padding fix) | 66.68 ms | 1.090 ms | 1.020 ms |
| ChaCha20-Poly1305.Decrypt (with padding fix) | 66.72 ms | 1.160 ms | 1.085 ms |

## How does it work?
### Constants
```c#
internal const int SaltLength = 16;
internal const int EncryptionKeyLength = 32;
internal const int MacKeyLength = 64;
internal const int ChaChaNonceLength = 8;
internal const int XChaChaNonceLength = 24;
internal static readonly byte[] EncryptionPersonal = Encoding.UTF8.GetBytes("ChaCha20.Encrypt");
internal static readonly byte[] AuthenticationPersonal = Encoding.UTF8.GetBytes("BLAKE2b.KeyedMAC");
```

### Encryption
1. A 128-bit counter salt is created.
```c#
byte[] salt = new byte[Constants.SaltLength];
```
2. BLAKE2b-256 is used to derive a 32 byte encryption key. The nonce is used as the message, the key as the key, the counter as the salt, and ```Constants.EncryptionPersonal``` as the personalisation parameter.
```c#
byte[] encryptionKey = GenericHash.HashSaltPersonal(nonce, inputKeyingMaterial, salt, Constants.EncryptionPersonal, Constants.EncryptionKeyLength);
```
3. The salt counter is incremented.
```c#
salt = Utilities.Increment(salt);
```
4. BLAKE2b-512 is used to derive a 64 byte MAC key. The nonce is used as the message, the key as the key, the counter as the salt, and ```Constants.AuthenticationPersonal``` as the personalisation parameter.
```c#
byte[] macKey = GenericHash.HashSaltPersonal(nonce, inputKeyingMaterial, salt, Constants.AuthenticationPersonal, Constants.MacKeyLength);
```
5. The plaintext message is encrypted using ChaCha20 with the encryption key and nonce.
```c#
byte[] ciphertext = StreamEncryption.EncryptChaCha20(message, nonce, encryptionKey);
```
6. The additional data, ciphertext, additional data length, and ciphertext length are concatenated. The array lengths are always little-endian.
```c#
byte[] tagMessage = Arrays.Concat(additionalData, ciphertext, Arrays.ConvertLength(additionalData.Length), Arrays.ConvertLength(ciphertext.Length));
```
7. BLAKE2b is used to hash this message with the MAC key as the key. The tag length defaults to 32 bytes but can also be 16 or 64 bytes.
```c#
byte[] tag = GenericHash.Hash(tagMessage, macKey, (int)tagLength);
```
8. The authentication tag is appended to the ciphertext.
```c#
return Arrays.Concat(ciphertext, tag);
```

### Decryption
1. A 128-bit counter salt is created.
```c#
byte[] salt = new byte[Constants.SaltLength];
```
2. BLAKE2b-256 is used to derive a 32 byte encryption key. The nonce is used as the message, the key as the key, the counter as the salt, and ```Constants.EncryptionPersonal``` as the personalisation parameter.
```c#
byte[] encryptionKey = GenericHash.HashSaltPersonal(nonce, inputKeyingMaterial, salt, Constants.EncryptionPersonal, Constants.EncryptionKeyLength);
```
3. The salt counter is incremented.
```c#
salt = Utilities.Increment(salt);
```
4. BLAKE2b-512 is used to derive a 64 byte MAC key. The nonce is used as the message, the key as the key, the counter as the salt, and ```Constants.AuthenticationPersonal``` as the personalisation parameter.
```c#
byte[] macKey = GenericHash.HashSaltPersonal(nonce, inputKeyingMaterial, salt, Constants.AuthenticationPersonal, Constants.MacKeyLength);
```
5. The authentication tag is read and removed from the ciphertext.
```c#
byte[] tag = Tag.Read(ciphertext, tagSize);
ciphertext = Tag.Remove(ciphertext, tagSize);
```
6. The additional data, ciphertext, additional data length, and ciphertext length are concatenated. The array lengths are always little-endian.
```c#
byte[] tagMessage = Arrays.Concat(additionalData, ciphertext, Arrays.ConvertLength(additionalData.Length), Arrays.ConvertLength(ciphertext.Length));
```
7. BLAKE2b is used to hash this message with the MAC key as the key. The tag length defaults to 32 bytes but can also be 16 or 64 bytes.
```c#
byte[] computedTag = GenericHash.Hash(tagMessage, macKey, tagSize);
```
8. The computed tag is compared in constant time to the tag read from the ciphertext. If they do not match, then a ```CryptographicException``` is thrown and decryption stops.
```c#
bool validTag = Utilities.Compare(tag, computedTag);
if (!validTag) { throw new CryptographicException(); }
```
9. Otherwise, the ciphertext is decrypted using the encryption key and nonce.
```c#
return StreamEncryption.DecryptChaCha20(ciphertext, nonce, encryptionKey);
```
