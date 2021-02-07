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
    internal static class Arrays
    {
        private const int _index = 0;

        internal static byte[] Concat(byte[] a, byte[] b)
        {
            var concat = new byte[a.Length + b.Length];
            Array.Copy(a, _index, concat, _index, a.Length);
            Array.Copy(b, _index, concat, a.Length, b.Length);
            return concat;
        }

        internal static byte[] Concat(byte[] a, byte[] b, byte[] c, byte[] d)
        {
            var concat = new byte[a.Length + b.Length + c.Length + d.Length];
            Array.Copy(a, _index, concat, _index, a.Length);
            Array.Copy(b, _index, concat, a.Length, b.Length);
            Array.Copy(c, _index, concat, a.Length + b.Length, c.Length);
            Array.Copy(d, _index, concat, a.Length + b.Length + c.Length, d.Length);
            return concat;
        }
    }
}
