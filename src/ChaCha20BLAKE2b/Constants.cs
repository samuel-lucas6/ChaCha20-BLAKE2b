﻿using System.Text;

/*
    ChaCha20-BLAKE2b: A committing AEAD implementation.
    Copyright (c) 2021 Samuel Lucas

    Permission is hereby granted, free of charge, to any person obtaining a copy of
    this software and associated documentation files (the "Software"), to deal in
    the Software without restriction, including without limitation the rights to
    use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
    the Software, and to permit persons to whom the Software is furnished to do so,
    subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

namespace ChaCha20BLAKE2
{
    internal static class Constants
    {
        internal const int SaltLength = 16;
        internal const int EncryptionKeyLength = 32;
        internal const int MacKeyLength = 64;
        internal const int ChaChaNonceLength = 8;
        internal const int XChaChaNonceLength = 24;
        internal static readonly byte[] EncryptionPersonal = Encoding.UTF8.GetBytes("ChaCha20.Encrypt");
        internal static readonly byte[] AuthenticationPersonal = Encoding.UTF8.GetBytes("BLAKE2b.KeyedMAC");
    }
}
