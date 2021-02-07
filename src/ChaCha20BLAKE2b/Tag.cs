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
    internal static class Tag
    {
        internal static byte[] Read(byte[] ciphertext)
        {
            byte[] tag = new byte[Constants.TagLength];
            Array.Copy(ciphertext, ciphertext.Length - tag.Length, tag, destinationIndex: 0, tag.Length);
            return tag;
        }

        internal static byte[] Remove(byte[] ciphertextWithTag)
        {
            byte[] ciphertext = new byte[ciphertextWithTag.Length - Constants.TagLength];
            Array.Copy(ciphertextWithTag, sourceIndex: 0, ciphertext, destinationIndex: 0, ciphertext.Length);
            return ciphertext;
        }
    }
}
