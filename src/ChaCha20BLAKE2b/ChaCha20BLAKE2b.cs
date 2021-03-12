using System.Security.Cryptography;
using Sodium;

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
    public static class ChaCha20BLAKE2b
    {
        /// <summary>Encrypts a message using ChaCha20-BLAKE2b.</summary>
        /// <param name="message">The message to encrypt.</param>
        /// <param name="nonce">The 8 byte nonce.</param>
        /// <param name="key">The 32 byte key.</param>
        /// <param name="additionalData">Optional additional data to authenticate.</param>
        /// <param name="tagLength">The length of the authentication tag. The default length is 32 bytes.</param>
        /// <remarks>Never reuse a nonce with the same key. A counter nonce is strongly recommended.</remarks>
        /// <returns>The ciphertext and tag.</returns>
        public static byte[] Encrypt(byte[] message, byte[] nonce, byte[] key, byte[] additionalData = null, TagLength tagLength = TagLength.Medium)
        {
            ParameterValidation.Message(message);
            ParameterValidation.Nonce(nonce, Constants.ChaChaNonceLength);
            ParameterValidation.Key(key, Constants.EncryptionKeyLength);
            additionalData = ParameterValidation.AdditionalData(additionalData);
            (byte[] encryptionKey, byte[] macKey) = KeyDerivation.DeriveKeys(nonce, key);
            byte[] ciphertext = StreamEncryption.EncryptChaCha20(message, nonce, encryptionKey);
            byte[] tagMessage = Arrays.Concat(additionalData, ciphertext, Arrays.ConvertLength(additionalData.Length), Arrays.ConvertLength(ciphertext.Length));
            byte[] tag = GenericHash.Hash(tagMessage, macKey, (int)tagLength);
            return Arrays.Concat(ciphertext, tag);
        }

        /// <summary>Decrypts a ciphertext message using ChaCha20-BLAKE2b.</summary>
        /// <param name="ciphertext">The ciphertext to decrypt.</param>
        /// <param name="nonce">The 8 byte nonce.</param>
        /// <param name="key">The 32 byte key.</param>
        /// <param name="additionalData">Optional additional data to authenticate.</param>
        /// <param name="tagLength">The length of the authentication tag. The default length is 32 bytes.</param>
        /// <returns>The decrypted message.</returns>
        public static byte[] Decrypt(byte[] ciphertext, byte[] nonce, byte[] key, byte[] additionalData = null, TagLength tagLength = TagLength.Medium)
        {
            ParameterValidation.Ciphertext(ciphertext);
            ParameterValidation.Nonce(nonce, Constants.ChaChaNonceLength);
            ParameterValidation.Key(key, Constants.EncryptionKeyLength);
            additionalData = ParameterValidation.AdditionalData(additionalData);
            int tagSize = (int)tagLength;
            (byte[] encryptionKey, byte[] macKey) = KeyDerivation.DeriveKeys(nonce, key);
            byte[] tag = Tag.Read(ciphertext, tagSize);
            ciphertext = Tag.Remove(ciphertext, tagSize);
            byte[] tagMessage = Arrays.Concat(additionalData, ciphertext, Arrays.ConvertLength(additionalData.Length), Arrays.ConvertLength(ciphertext.Length));
            byte[] computedTag = GenericHash.Hash(tagMessage, macKey, tagSize);
            bool validTag = Utilities.Compare(tag, computedTag);
            return !validTag ? throw new CryptographicException() : StreamEncryption.DecryptChaCha20(ciphertext, nonce, encryptionKey);
        }
    }
}
