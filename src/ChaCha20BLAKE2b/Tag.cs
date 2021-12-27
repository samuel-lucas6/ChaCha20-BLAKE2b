﻿using System;
using Sodium;

/*
    ChaCha20-BLAKE2b: Committing ChaCha20-BLAKE2b, XChaCha20-BLAKE2b, and XChaCha20-BLAKE2b-SIV AEAD implementations.
    Copyright (c) 2021-2022 Samuel Lucas

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
    internal static class Tag
    {
        internal static byte[] Calculate(byte[] ciphertext, byte[] additionalData, byte[] macKey, int tagLength)
        {
            byte[] tagMessage = Arrays.Concat(additionalData, ciphertext, BitConversion.GetBytes(additionalData.Length), BitConversion.GetBytes(ciphertext.Length));
            return GenericHash.Hash(tagMessage, macKey, tagLength);
        }

        internal static byte[] Read(byte[] ciphertext, int tagLength)
        {
            var tag = new byte[tagLength];
            Array.Copy(ciphertext, ciphertext.Length - tag.Length, tag, destinationIndex: 0, tag.Length);
            return tag;
        }

        internal static byte[] Remove(byte[] ciphertextWithTag, int tagLength)
        {
            var ciphertext = new byte[ciphertextWithTag.Length - tagLength];
            Array.Copy(ciphertextWithTag, sourceIndex: 0, ciphertext, destinationIndex: 0, ciphertext.Length);
            return ciphertext;
        }

        internal static byte[] GetNonce(byte[] tag)
        {
            var nonce = new byte[Constants.XChaCha20NonceSize];
            Array.Copy(tag, nonce, nonce.Length);
            return nonce;
        }
    }
}
