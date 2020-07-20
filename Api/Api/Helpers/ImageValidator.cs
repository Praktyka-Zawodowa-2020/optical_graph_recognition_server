using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Api.Services
{
    public class ImageValidator
    {
        private static readonly Dictionary<byte[], string> _fileSignatures = new Dictionary<byte[], string>(new StructuralEqualityComparer())
        {
            { new byte[] { 0xFF, 0xD8, 0xFF, 0xDB }, ".jpeg" },
            { new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, ".jpeg" },
            { new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 }, ".jpeg" },
            { new byte[] { 0xFF, 0xD8, 0xFF, 0xEE }, ".jpeg" },
            { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },".png" },
            { new byte[] { 0x42, 0x4D }, ".bmp" }
        };


        public string GetValidExtension(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                int[] sygSizes = { 2, 4, 8 };
                var headerBytes = reader.ReadBytes(sygSizes.Max());
                foreach (var size in sygSizes)
                {
                    var bytes = headerBytes.Take(size).ToArray();

                    if (!_fileSignatures.ContainsKey(bytes))
                        continue;

                    var extension = _fileSignatures[bytes];

                    return extension;
                }
                return String.Empty;
            }
        }

        [Serializable]
        internal class StructuralEqualityComparer : IEqualityComparer, IEqualityComparer<object>
        {
            public new bool Equals(object x, object y)
            {
                var s = x as IStructuralEquatable;
                return s == null ? object.Equals(x, y) : s.Equals(y, this);
            }

            public int GetHashCode(object obj)
            {
                var s = obj as IStructuralEquatable;
                return s == null ? EqualityComparer<object>.Default.GetHashCode(obj) : s.GetHashCode(this);
            }
        }
    }
}
