using System;
using System.Security.Cryptography;
using Sodium;

/*
    ChaCha20-BLAKE2b: An AEAD implementation using libsodium.
    Copyright(C) 2021 Samuel Lucas

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program. If not, see https://www.gnu.org/licenses/.
*/

namespace ChaCha20BLAKE2
{
    public static class XChaCha20BLAKE2b
    {
        /// <summary>Encrypts a message using XChaCha20-BLAKE2b.</summary>
        /// <param name="message">The message to encrypt.</param>
        /// <param name="nonce">The 24 byte nonce.</param>
        /// <param name="key">The 32 byte key.</param>
        /// <param name="additionalData">Optional additional data to authenticate.</param>
        /// <remarks>Never reuse a nonce with the same key. A random nonce is recommended.</remarks>
        /// <returns>The ciphertext and tag.</returns>
        public static byte[] Encrypt(byte[] message, byte[] nonce, byte[] key, byte[] additionalData = null)
        {
            ParameterValidation.Message(message);
            ParameterValidation.Nonce(nonce, Constants.XChaChaNonceLength);
            ParameterValidation.Key(key, Constants.EncryptionKeyLength);
            additionalData = ParameterValidation.AdditionalData(additionalData);
            (byte[] encryptionKey, byte[] macKey) = KeyDerivation.DeriveKeys(nonce, key);
            byte[] ciphertext = StreamEncryption.EncryptXChaCha20(message, nonce, encryptionKey);
            byte[] tagMessage = Arrays.Concat(additionalData, ciphertext, BitConverter.GetBytes(additionalData.Length), BitConverter.GetBytes(ciphertext.Length));
            byte[] tag = GenericHash.Hash(tagMessage, macKey, Constants.TagLength);
            return Arrays.Concat(ciphertext, tag);
        }

        /// <summary>Decrypts a ciphertext message using XChaCha20-BLAKE2b.</summary>
        /// <param name="ciphertext">The ciphertext to decrypt.</param>
        /// <param name="nonce">The 24 byte nonce.</param>
        /// <param name="key">The 32 byte key.</param>
        /// <param name="additionalData">Optional additional data to authenticate.</param>
        /// <remarks>Never reuse a nonce with the same key. A random nonce is recommended.</remarks>
        /// <returns>The decrypted message.</returns>
        public static byte[] Decrypt(byte[] ciphertext, byte[] nonce, byte[] key, byte[] additionalData = null)
        {
            ParameterValidation.Message(ciphertext);
            ParameterValidation.Nonce(nonce, Constants.XChaChaNonceLength);
            ParameterValidation.Key(key, Constants.EncryptionKeyLength);
            additionalData = ParameterValidation.AdditionalData(additionalData);
            (byte[] encryptionKey, byte[] macKey) = KeyDerivation.DeriveKeys(nonce, key);
            byte[] tag = Tag.Read(ciphertext);
            ciphertext = Tag.Remove(ciphertext);
            byte[] tagMessage = Arrays.Concat(additionalData, ciphertext, BitConverter.GetBytes(additionalData.Length), BitConverter.GetBytes(ciphertext.Length));
            byte[] computedTag = GenericHash.Hash(tagMessage, macKey, Constants.TagLength);
            bool validTag = Utilities.Compare(tag, computedTag);
            return !validTag ? throw new CryptographicException() : StreamEncryption.DecryptXChaCha20(ciphertext, nonce, encryptionKey);
        }
    }
}
