using System.Text;

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
    internal static class Constants
    {
        internal const int SaltLength = 16;
        internal const int EncryptionKeyLength = 32;
        internal const int MacKeyLength = 64;
        internal const int ChaChaNonceLength = 8;
        internal const int XChaChaNonceLength = 24;
        internal const int TagLength = 64;
        internal static readonly byte[] EncryptionPersonal = Encoding.UTF8.GetBytes("ChaCha20.Encrypt");
        internal static readonly byte[] AuthenticationPersonal = Encoding.UTF8.GetBytes("BLAKE2b.KeyedMAC");
    }
}
