using System;

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
    internal static class ParameterValidation
    {
        internal static void Message(byte[] message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message), "Message cannot be null.");
            }
        }

        internal static void Nonce(byte[] nonce, int validNonceLength)
        {
            if (nonce == null || nonce.Length != validNonceLength)
            {
                throw new ArgumentOutOfRangeException(nameof(nonce), (nonce == null) ? 0 : nonce.Length, $"Nonce must be {validNonceLength} bytes in length.");
            }
        }

        internal static void Key(byte[] key, int validKeyLength)
        {
            if (key == null || key.Length != validKeyLength)
            {
                throw new ArgumentOutOfRangeException(nameof(key), (key == null) ? 0 : key.Length, $"Key must be {validKeyLength} bytes in length.");
            }
        }

        internal static byte[] AdditionalData(byte[] additionalData)
        {
            // Additional data can be null
            return additionalData ?? (Array.Empty<byte>());
        }
    }
}
