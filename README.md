[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/samuel-lucas6/ChaCha20-BLAKE2b/blob/main/LICENSE)
[![CodeQL](https://github.com/samuel-lucas6/ChaCha20-BLAKE2b/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/samuel-lucas6/ChaCha20-BLAKE2b/actions)

# ChaCha20-BLAKE2b
Committing ChaCha20-BLAKE2b, XChaCha20-BLAKE2b, and XChaCha20-BLAKE2b-SIV AEAD implementations using [libsodium](https://doc.libsodium.org/).

## Features
This library does several things for you:

- Derives a 256-bit encryption key and 512-bit MAC key based on the key and nonce.
- Supports additional data in the calculation of the authentication tag, unlike most Encrypt-then-MAC implementations.
- Appends the authentication tag to the ciphertext.
- Compares the authentication tags in constant time during decryption and only returns plaintext if they match.
- Offers access to an SIV implementation that does not take a nonce.

## Justification
The popular AEADs in use today, such as (X)ChaCha20-Poly1305, AES-GCM, AES-GCM-SIV, XSalsa20-Poly1305, AES-OCB, and so on, are not key or message committing. This means it is possible to decrypt a ciphertext using [multiple keys](https://eprint.iacr.org/2020/1491.pdf) without an authentication error, which can lead to [partitioning oracle attacks](https://eprint.iacr.org/2020/1491.pdf) and [deanonymisation](https://github.com/LoupVaillant/Monocypher/issues/218#issuecomment-886997371) in certain online scenarios. Furthermore, if an attacker knows the key, then they can find other messages that have the [same tag](https://neilmadden.blog/2021/02/16/when-a-kem-is-not-enough/).

This library was created because there are currently no standardised committing AEAD schemes, adding the commitment property to a non-committing AEAD requires using a MAC, and Encrypt-then-MAC offers improved security guarantees, both in terms of the longer authentication tag and commitment properties.

Finally, (X)ChaCha20-BLAKE2b is the ideal combination for an Encrypt-then-MAC scheme because:
1. ChaCha20 has a [higher security margin](https://eprint.iacr.org/2019/1492.pdf) than AES, performs well on older devices, and runs in [constant time](https://cr.yp.to/chacha/chacha-20080128.pdf), [unlike](https://cr.yp.to/antiforgery/cachetiming-20050414.pdf) AES.
2. BLAKE2b provides a [similar security margin](https://eprint.iacr.org/2019/1492.pdf) to SHA3 whilst being considerably faster.

## Installation
1. Install the [Sodium.Core](https://www.nuget.org/packages/Sodium.Core) NuGet package in [Visual Studio](https://docs.microsoft.com/en-us/nuget/quickstart/install-and-use-a-package-in-visual-studio).
2. Download the latest [release](https://github.com/samuel-lucas6/ChaCha20-BLAKE2b/releases).
3. Move the downloaded DLL file into your Visual Studio project folder.
4. Click on the `Project` tab and `Add Project Reference...` in Visual Studio.
5. Go to `Browse`, click the `Browse` button, and select the downloaded DLL file.
6. Add `using ChaCha20BLAKE2;` to the top of each code file that will use the library.

Note that the [libsodium](https://doc.libsodium.org/) library requires the [Visual C++ Redistributable for Visual Studio 2015-2019](https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads) to work on Windows. If you want your program to be portable, then you must keep the relevant (x86 or x64) `vcruntime140.dll` file in the same folder as your executable on Windows.

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

// The key can be randomly generated using a CSPRNG or derived using a KDF (e.g. Argon2, HKDF, etc)
byte[] key = SodiumCore.GetRandomBytes(keyLength);

// The additional data can be null but is ideal for file headers, version numbers, timestamps, etc
byte[] additionalData = BitConverter.GetBytes(version);

// Encrypt the message and use a 256-bit authentication tag
byte[] ciphertext = ChaCha20BLAKE2b.Encrypt(message, nonce, key, additionalData, TagLength.BLAKE2b256);

// Decrypt the ciphertext
byte[] plaintext = ChaCha20BLAKE2b.Decrypt(ciphertext, nonce, key, additionalData, TagLength.BLAKE2b256);
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

// The nonce can be random. Increment or randomly generate the nonce for each message encrypted using the same key
byte[] nonce = SodiumCore.GetRandomBytes(nonceLength);

// The key can be randomly generated using a CSPRNG or derived using a KDF (e.g. Argon2, HKDF, etc)
byte[] key = SodiumCore.GetRandomBytes(keyLength);

// The additional data can be null but is ideal for file headers, version numbers, timestamps, etc
byte[] additionalData = BitConverter.GetBytes(version);

// Encrypt the message and use a 512-bit authentication tag
byte[] ciphertext = XChaCha20BLAKE2b.Encrypt(message, nonce, key, additionalData, TagLength.BLAKE2b512);

// Decrypt the ciphertext
byte[] plaintext = XChaCha20BLAKE2b.Decrypt(ciphertext, nonce, key, additionalData, TagLength.BLAKE2b512);
```

### XChaCha20-BLAKE2b-SIV
⚠️**WARNING: A new key should be used for each message. Otherwise, you should include at least 16 bytes of unique, random data as part of the additional data to ensure semantic security.**

```c#
const string filePath = "C:\\Users\\samuel-lucas6\\Pictures\\test.jpg";
const int keyLength = 32;
const int randomAdditionalDataLength = 32;

// The message does not have to be a file
byte[] message = File.ReadAllBytes(filePath);

// The key can be randomly generated using a CSPRNG or derived using a KDF (e.g. Argon2, HKDF, etc)
byte[] key = SodiumCore.GetRandomBytes(keyLength);

// The additional data can be null, serve as the nonce, or be used for file headers, version numbers, timestamps, etc
byte[] additionalData = SodiumCore.GetRandomBytes(randomAdditionalDataLength);

// Encrypt the message
byte[] ciphertext = XChaCha20BLAKE2bSIV.Encrypt(message, key, additionalData);

// Decrypt the ciphertext
byte[] plaintext = XChaCha20BLAKE2bSIV.Decrypt(ciphertext, key, additionalData);
```

## Benchmarks
TO DO.
