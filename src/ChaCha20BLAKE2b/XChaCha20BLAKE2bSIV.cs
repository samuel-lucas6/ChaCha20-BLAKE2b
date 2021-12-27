using System.Text;
using System.Security.Cryptography;
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
    public static class XChaCha20BLAKE2bSIV
    {
        public const int KeySize = Constants.EncryptionKeySize;
        private static readonly byte[] EncryptionContext = Encoding.UTF8.GetBytes("XChaCha20-BLAKE2b-SIV 07/12/2021 20:51 XChaCha20.Encrypt()");
        private static readonly byte[] AuthenticationContext = Encoding.UTF8.GetBytes("XChaCha20-BLAKE2b-SIV 07/12/2021 20:52 BLAKE2b.KeyedHash()");

        public static byte[] Encrypt(byte[] message, byte[] key, byte[] additionalData = null, TagLength tagLength = TagLength.BLAKE2b256)
        {
            ParameterValidation.Message(message);
            ParameterValidation.Key(key, Constants.EncryptionKeySize);
            additionalData = ParameterValidation.AdditionalData(additionalData);
            (byte[] encryptionKey, byte[] macKey) = KeyDerivation.DeriveKeys(key, nonce: null, EncryptionContext, AuthenticationContext);
            byte[] tag = Tag.Calculate(message, additionalData, macKey, (int)tagLength);
            byte[] nonce = Tag.GetNonce(tag);
            byte[] ciphertext = StreamEncryption.EncryptXChaCha20(message, nonce, encryptionKey);
            return Arrays.Concat(ciphertext, tag);
        }

        public static byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] additionalData = null, TagLength tagLength = TagLength.BLAKE2b256)
        {
            ParameterValidation.Ciphertext(ciphertext, (int)tagLength);
            ParameterValidation.Key(key, Constants.EncryptionKeySize);
            additionalData = ParameterValidation.AdditionalData(additionalData);
            (byte[] encryptionKey, byte[] macKey) = KeyDerivation.DeriveKeys(key, nonce: null, EncryptionContext, AuthenticationContext);
            byte[] tag = Tag.Read(ciphertext, (int)tagLength);
            byte[] ciphertextWithoutTag = Tag.Remove(ciphertext, (int)tagLength);
            byte[] nonce = Tag.GetNonce(tag);
            byte[] message = StreamEncryption.DecryptXChaCha20(ciphertextWithoutTag, nonce, encryptionKey);
            byte[] computedTag = Tag.Calculate(message, additionalData, macKey, (int)tagLength);
            bool validTag = Utilities.Compare(tag, computedTag);
            if (validTag) { return message; }
            Arrays.ZeroMemory(message);
            Arrays.ZeroMemory(computedTag);
            throw new CryptographicException();
        }
    }
}
