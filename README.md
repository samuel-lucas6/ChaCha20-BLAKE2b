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
The popular AEADs in use today, such as (X)ChaCha20-Poly1305, AES-GCM, AES-GCM-SIV, XSalsa20-Poly1305, AES-OCB, and so on, are not key or message committing. This means it is possible to decrypt a ciphertext using [multiple keys](https://youtu.be/3M1jIO-jLHI) without an authentication error, which can lead to [partitioning oracle attacks](https://eprint.iacr.org/2020/1491.pdf) and [deanonymisation](https://github.com/LoupVaillant/Monocypher/issues/218#issuecomment-886997371) in certain online scenarios. Furthermore, if an attacker knows the key, then they can find other messages that have the [same tag](https://neilmadden.blog/2021/02/16/when-a-kem-is-not-enough/).

This library was created because there are currently no standardised committing AEAD schemes, adding the commitment property to a non-committing AEAD requires using a MAC, and Encrypt-then-MAC offers improved security guarantees, both in terms of the longer authentication tag and commitment properties.

Finally, (X)ChaCha20-BLAKE2b is the ideal combination for an Encrypt-then-MAC scheme because:
1. ChaCha20 has a [higher security margin](https://eprint.iacr.org/2019/1492.pdf) than AES, performs well on older devices, and runs in [constant time](https://cr.yp.to/chacha/chacha-20080128.pdf), [unlike](https://cr.yp.to/antiforgery/cachetiming-20050414.pdf) AES.
2. BLAKE2b is as real-world [secure](https://eprint.iacr.org/2019/1492.pdf) as SHA3 whilst being considerably [faster](https://www.blake2.net/). It relies on essentially the same core algorithm as BLAKE, which received a [significant amount of cryptanalysis](https://nvlpubs.nist.gov/nistpubs/ir/2012/NIST.IR.7896.pdf), even more than Keccak (the SHA3 finalist), as part of the [SHA3 competition](https://competitions.cr.yp.to/sha3.html).

## Installation
### NuGet
You can find the NuGet package [here](https://www.nuget.org/packages/ChaCha20BLAKE2b). The easiest way to install this is via the NuGet Package Manager in [Visual Studio](https://visualstudio.microsoft.com/vs/), as explained [here](https://docs.microsoft.com/en-us/nuget/quickstart/install-and-use-a-package-in-visual-studio). [JetBrains Rider](https://www.jetbrains.com/rider/) also has a package manager, and instructions can be found [here](https://www.jetbrains.com/help/rider/Using_NuGet.html).

### Manual
1. Install the [Sodium.Core](https://www.nuget.org/packages/Sodium.Core) NuGet package in [Visual Studio](https://docs.microsoft.com/en-us/nuget/quickstart/install-and-use-a-package-in-visual-studio).
2. Download the latest [release](https://github.com/samuel-lucas6/ChaCha20-BLAKE2b/releases/latest).
3. Move the downloaded `.dll` file into your Visual Studio project folder.
4. Click on the `Project` tab and `Add Project Reference...` in Visual Studio.
5. Go to `Browse`, click the `Browse` button, and select the downloaded `.dll` file.
6. Add `using ChaCha20BLAKE2;` to the top of each code file that will use the library.

### Requirements
Note that the [libsodium](https://doc.libsodium.org/) library requires the [Visual C++ Redistributable for Visual Studio 2015-2019](https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads) to work on Windows. If you want your program to be portable, then you must keep the relevant (x86 or x64) `vcruntime140.dll` file in the same folder as your executable on Windows.

## Usage
### ChaCha20-BLAKE2b
⚠️**WARNING: Never reuse a nonce with the same key.**
```c#
const string message = "This is a test.";
const string version = "application v2.0.0";

// The message could be a file
byte[] message = Encoding.UTF8.GetBytes(message);

// The nonce should be a counter that gets incremented for each message encrypted using the same key
byte[] nonce = new byte[ChaCha20BLAKE2b.NonceSize];

// The key can be randomly generated using a CSPRNG or derived using a KDF (e.g. Argon2, HKDF, etc)
byte[] key = SodiumCore.GetRandomBytes(ChaCha20BLAKE2b.KeySize);

// The additional data can be null but is ideal for file headers, version numbers, timestamps, etc
byte[] additionalData = Encoding.UTF8.GetBytes(version);

// Encrypt the message and use a 256-bit authentication tag
byte[] ciphertext = ChaCha20BLAKE2b.Encrypt(message, nonce, key, additionalData, TagLength.BLAKE2b256);

// Decrypt the ciphertext
byte[] plaintext = ChaCha20BLAKE2b.Decrypt(ciphertext, nonce, key, additionalData, TagLength.BLAKE2b256);
```

### XChaCha20-BLAKE2b
⚠️**WARNING: Never reuse a nonce with the same key.**
```c#
const string message = "This is a test.";
const string version = "application v2.0.0";

// The message could be a file
byte[] message = Encoding.UTF8.GetBytes(message);

// The nonce can be random. Increment or randomly generate the nonce for each message encrypted using the same key
byte[] nonce = SodiumCore.GetRandomBytes(XChaCha20BLAKE2b.NonceSize);

// The key can be randomly generated using a CSPRNG or derived using a KDF (e.g. Argon2, HKDF, etc)
byte[] key = SodiumCore.GetRandomBytes(XChaCha20BLAKE2b.KeySize);

// The additional data can be null but is ideal for file headers, version numbers, timestamps, etc
byte[] additionalData = Encoding.UTF8.GetBytes(version);

// Encrypt the message and use a 512-bit authentication tag
byte[] ciphertext = XChaCha20BLAKE2b.Encrypt(message, nonce, key, additionalData, TagLength.BLAKE2b512);

// Decrypt the ciphertext
byte[] plaintext = XChaCha20BLAKE2b.Decrypt(ciphertext, nonce, key, additionalData, TagLength.BLAKE2b512);
```

### XChaCha20-BLAKE2b-SIV
⚠️**WARNING: Never reuse a key. As a precaution, you can use at least 16 bytes of unique, random data as part of the additional data to act as a nonce.**

```c#
const string filePath = "C:\\Users\\samuel-lucas6\\Pictures\\test.jpg";

// The message does not have to be a file
byte[] message = File.ReadAllBytes(filePath);

// The key can be randomly generated using a CSPRNG or derived using a KDF (e.g. Argon2, HKDF, etc)
byte[] key = SodiumCore.GetRandomBytes(XChaCha20BLAKE2bSIV.KeySize);

// The additional data can be null, used as a nonce, and/or used for file headers, version numbers, timestamps, etc
byte[] additionalData = SodiumCore.GetRandomBytes(XChaCha20BLAKE2bSIV.KeySize / 2);

// Encrypt the message and use the default authentication tag length (256-bit)
byte[] ciphertext = XChaCha20BLAKE2bSIV.Encrypt(message, key, additionalData);

// Decrypt the ciphertext
byte[] plaintext = XChaCha20BLAKE2bSIV.Decrypt(ciphertext, key, additionalData);
```

## Benchmarks
The following benchmarks were done using [BenchmarkDotNet](https://benchmarkdotnet.org/) in a .NET 6 console application with 16 bytes of additional data and the default 256-bit tag size.

In sum, (X)ChaCha20-BLAKE2b is almost identical in speed to (X)ChaCha20-Poly1305 for 16-64 KiB messages, which is perfect since these are ideal chunk sizes for performing [chunked encryption](https://www.imperialviolet.org/2014/06/27/streamingencryption.html).

Chunked encryption should be preferred over encrypting large messages in one go because it reduces memory usage, allows earlier detection of corrupted chunks, may help reduce data loss, and reduces wiggle room for attacks in the case of popular AEADs (e.g. ChaCha20-Poly1305 and AES-GCM).

However, (X)ChaCha20-BLAKE2b is slower than (X)ChaCha20-Poly1305 for small and large messages. With that said, you should perform chunked encryption on large messages anyway, rendering that finding unimportant, and I would argue that the additional security makes this trade-off worthwhile in the case of small messages.

#### 512-byte file
|                        Method |     Mean |     Error |    StdDev |
|:----------------------------: |:--------:|:---------:|:---------:|
|      ChaCha20-BLAKE2b.Encrypt | 1.404 us | 0.0047 us | 0.0044 us |
|      ChaCha20-BLAKE2b.Decrypt | 1.453 us | 0.0024 us | 0.0022 us |
|     XChaCha20-BLAKE2b.Encrypt | 1.494 us | 0.0073 us | 0.0068 us |
|     XChaCha20-BLAKE2b.Decrypt | 1.548 us | 0.0050 us | 0.0044 us |
| XChaCha20-BLAKE2b-SIV.Encrypt | 1.474 us | 0.0045 us | 0.0038 us |
| XChaCha20-BLAKE2b-SIV.Decrypt | 1.538 us | 0.0034 us | 0.0030 us |
|  ChaCha20-Poly1305.Encrypt | 779.1 ns | 1.01 ns | 0.94 ns |
|  ChaCha20-Poly1305.Decrypt | 795.3 ns | 0.92 ns | 0.77 ns |
| XChaCha20-Poly1305.Encrypt | 865.5 ns | 1.24 ns | 1.10 ns |
| XChaCha20-Poly1305.Decrypt | 883.8 ns | 1.25 ns | 1.11 ns |

#### 16.1 KiB file
|                        Method |     Mean |    Error |   StdDev |
|:----------------------------: |:--------:|:--------:|:--------:|
|      ChaCha20-BLAKE2b.Encrypt | 17.39 us | 0.071 us | 0.063 us |
|      ChaCha20-BLAKE2b.Decrypt | 17.44 us | 0.026 us | 0.022 us |
|     XChaCha20-BLAKE2b.Encrypt | 17.46 us | 0.132 us | 0.124 us |
|     XChaCha20-BLAKE2b.Decrypt | 17.58 us | 0.125 us | 0.117 us |
| XChaCha20-BLAKE2b-SIV.Encrypt | 17.43 us | 0.148 us | 0.138 us |
| XChaCha20-BLAKE2b-SIV.Decrypt | 17.42 us | 0.024 us | 0.018 us |
|  ChaCha20-Poly1305.Encrypt | 16.97 us | 0.107 us | 0.100 us |
|  ChaCha20-Poly1305.Decrypt | 16.97 us | 0.042 us | 0.035 us |
| XChaCha20-Poly1305.Encrypt | 16.94 us | 0.029 us | 0.023 us |
| XChaCha20-Poly1305.Decrypt | 17.04 us | 0.024 us | 0.018 us |

#### 31.6 KiB file
|                        Method |     Mean |    Error |   StdDev |
|:----------------------------: |:--------:|:--------:|:--------:|
|      ChaCha20-BLAKE2b.Encrypt | 33.12 us | 0.314 us | 0.293 us |
|      ChaCha20-BLAKE2b.Decrypt | 33.14 us | 0.040 us | 0.031 us |
|     XChaCha20-BLAKE2b.Encrypt | 32.96 us | 0.042 us | 0.035 us |
|     XChaCha20-BLAKE2b.Decrypt | 33.34 us | 0.194 us | 0.181 us |
| XChaCha20-BLAKE2b-SIV.Encrypt | 33.13 us | 0.069 us | 0.057 us |
| XChaCha20-BLAKE2b-SIV.Decrypt | 33.31 us | 0.207 us | 0.193 us |
|  ChaCha20-Poly1305.Encrypt | 32.50 us | 0.093 us | 0.078 us |
|  ChaCha20-Poly1305.Decrypt | 32.48 us | 0.153 us | 0.143 us |
| XChaCha20-Poly1305.Encrypt | 32.48 us | 0.019 us | 0.016 us |
| XChaCha20-Poly1305.Decrypt | 32.68 us | 0.149 us | 0.140 us |

#### 64.7 KiB file
|                        Method |     Mean |    Error |   StdDev |
|:----------------------------: |:--------:|:--------:|:--------:|
|      ChaCha20-BLAKE2b.Encrypt | 66.14 us | 0.179 us | 0.139 us |
|      ChaCha20-BLAKE2b.Decrypt | 67.12 us | 0.570 us | 0.533 us |
|     XChaCha20-BLAKE2b.Encrypt | 66.80 us | 0.575 us | 0.538 us |
|     XChaCha20-BLAKE2b.Decrypt | 66.87 us | 0.104 us | 0.081 us |
| XChaCha20-BLAKE2b-SIV.Encrypt | 66.32 us | 0.104 us | 0.087 us |
| XChaCha20-BLAKE2b-SIV.Decrypt | 66.56 us | 0.177 us | 0.147 us |
|  ChaCha20-Poly1305.Encrypt | 66.32 us | 0.273 us | 0.256 us |
|  ChaCha20-Poly1305.Decrypt | 66.01 us | 0.079 us | 0.062 us |
| XChaCha20-Poly1305.Encrypt | 66.32 us | 0.438 us | 0.410 us |
| XChaCha20-Poly1305.Decrypt | 66.24 us | 0.244 us | 0.204 us |

#### 129 KiB file
|                        Method |     Mean |   Error |  StdDev |
|:----------------------------: |:--------:|:-------:|:-------:|
|      ChaCha20-BLAKE2b.Encrypt | 252.9 us | 0.45 us | 0.37 us |
|      ChaCha20-BLAKE2b.Decrypt | 245.1 us | 0.50 us | 0.39 us |
|     XChaCha20-BLAKE2b.Encrypt | 251.5 us | 1.65 us | 1.54 us |
|     XChaCha20-BLAKE2b.Decrypt | 245.0 us | 0.51 us | 0.40 us |
| XChaCha20-BLAKE2b-SIV.Encrypt | 251.7 us | 1.24 us | 1.16 us |
| XChaCha20-BLAKE2b-SIV.Decrypt | 244.6 us | 1.23 us | 1.09 us |
|  ChaCha20-Poly1305.Encrypt | 184.4 us | 0.29 us | 0.23 us |
|  ChaCha20-Poly1305.Decrypt | 184.8 us | 1.08 us | 0.96 us |
| XChaCha20-Poly1305.Encrypt | 185.0 us | 1.07 us | 1.00 us |
| XChaCha20-Poly1305.Decrypt | 184.9 us | 0.35 us | 0.31 us |

#### 34.1 MiB file
|                        Method |     Mean |    Error |   StdDev |
|:----------------------------: |:--------:|:--------:|:--------:|
|      ChaCha20-BLAKE2b.Encrypt | 58.14 ms | 0.340 ms | 0.284 ms |
|      ChaCha20-BLAKE2b.Decrypt | 65.02 ms | 0.314 ms | 0.294 ms |
|     XChaCha20-BLAKE2b.Encrypt | 66.56 ms | 0.270 ms | 0.253 ms |
|     XChaCha20-BLAKE2b.Decrypt | 65.25 ms | 0.254 ms | 0.237 ms |
| XChaCha20-BLAKE2b-SIV.Encrypt | 66.27 ms | 0.719 ms | 0.600 ms |
| XChaCha20-BLAKE2b-SIV.Decrypt | 64.03 ms | 0.149 ms | 0.116 ms |
|  ChaCha20-Poly1305.Encrypt | 52.99 ms | 0.169 ms | 0.141 ms |
|  ChaCha20-Poly1305.Decrypt | 52.74 ms | 0.104 ms | 0.093 ms |
| XChaCha20-Poly1305.Encrypt | 52.96 ms | 0.111 ms | 0.093 ms |
| XChaCha20-Poly1305.Decrypt | 52.65 ms | 0.183 ms | 0.153 ms |
