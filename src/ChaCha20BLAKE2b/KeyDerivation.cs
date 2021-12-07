﻿using System;
using Sodium;

/*
    ChaCha20-BLAKE2b: Committing ChaCha20-BLAKE2b, XChaCha20-BLAKE2b, and XChaCha20-BLAKE2b-SIV AEAD implementations.
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
    internal static class KeyDerivation
    {
        internal static (byte[] encryptionKey, byte[] macKey) DeriveKeys(byte[] inputKeyingMaterial, byte[] nonce, byte[] encryptionContext, byte[] authenticationContext)
        {
            byte[] encryptionKey = BLAKE2bKDF(inputKeyingMaterial, encryptionContext, Constants.EncryptionKeySize);
            byte[] macKey = BLAKE2bKDF(inputKeyingMaterial, authenticationContext, Constants.MacKeySize, nonce);
            return (encryptionKey, macKey);
        }

        private static byte[] BLAKE2bKDF(byte[] inputKeyingMaterial, byte[] context, int outputLength, byte[] salt = null)
        {
            if (salt == null) { salt = Array.Empty<byte>(); }
            byte[] message = Arrays.Concat(salt, context, BitConversion.GetBytes(salt.Length), BitConversion.GetBytes(context.Length));
            return GenericHash.Hash(message, inputKeyingMaterial, outputLength);
        }
    }
}
