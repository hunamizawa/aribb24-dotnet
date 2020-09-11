using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace AribB24.DotNet.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void SimpleDecodeTest()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var encoder = new B24Decoder();

            // TODO
            // �Ȃ񂩃o�C�g���^����
            // �f�R�[�h�ł���΂���
            var bytes = new byte[] { 0x0E, 0x1B, 0x7C, 0xA2, 0xA4, 0x89, 0xA6, 0xA8, 0x41, 0x42, 0x8A, 0x43, 0x44 };
            var expected = "�A�C��AB�b�c";

            var actual = encoder.GetString(bytes);
            Assert.Equal(expected, actual);

            // �G���[�ɂȂ�p�^�[��������
        }

        [Fact]
        public void Equals_HalfToFullTable_And_FullToHalfTable()
        {
            foreach (var (half, full) in B24Decoder.halfToFullTable)
                Assert.Equal(half, B24Decoder.fullToHalfTable[full]);
            foreach (var (full, half) in B24Decoder.fullToHalfTable)
                Assert.Equal(full, B24Decoder.halfToFullTable[half]);
        }

        [Fact]
        public void Decodes_JIS_X_0213_2004_Collectly()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var (jisCodes, fullToHalf) = ReadJisX0213File(@"jisx0213-2004-8bit-std.txt");
            ReadZen2HanFile(@"zen2han.txt", fullToHalf);

            var encoder = new B24Decoder();

            var bs = new byte[] {
                0x1b, 0x28, 0x39, // (GL <-) G0 <- JIS�݊�����1��
                0x1b, 0x2a, 0x3a, // (GR <-) G2 <- JIS�݊�����2��
                0x00, 0x00,       // 1������
                0x89,             // NSZ
                0x00, 0x00        // 2������
            };

            foreach (var (kuTen, codePoints) in jisCodes)
            {
                var expected_full = CodepointsToString(codePoints);
                var expected_half = fullToHalf.GetValueOrDefault(expected_full) ?? expected_full;

                // ���ʃp�^�[��
                var expected = expected_full switch
                {
                    "�`" => "�`~",
                    _ => expected_full + expected_half,
                };

                var ku = (byte)(kuTen >> 8);
                var ten = (byte)(kuTen & 0xFF);
                bs[6] = bs[9] = ku;
                bs[7] = bs[10] = ten;

                var actual = encoder.GetString(bs);
                //Assert.AreEqual(expected, actual, "Expected:<{0}>. Actual:<{1}>. 0x{2:X4} ({2}��{3}�_)", AsUnicodeLiteral(expected), AsUnicodeLiteral(actual), kuTen, ku - 0x20, ten - 0x20);
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Decodes_Katakana_Hiragana_Collectly()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var patterns = new Dictionary<int, (string, string, string)>
            {
                [0x21] = ("�@", "�", "��"),
                [0x22] = ("�A", "�", "��"),
                [0x23] = ("�B", "�", "��"),
                [0x24] = ("�C", "�", "��"),
                [0x25] = ("�D", "�", "��"),
                [0x26] = ("�E", "�", "��"),
                [0x27] = ("�F", "�", "��"),
                [0x28] = ("�G", "�", "��"),
                [0x29] = ("�H", "�", "��"),
                [0x2A] = ("�I", "�", "��"),
                [0x2B] = ("�J", "�", "��"),
                [0x2C] = ("�K", "��", "��"),
                [0x2D] = ("�L", "�", "��"),
                [0x2E] = ("�M", "��", "��"),
                [0x2F] = ("�N", "�", "��"),
                [0x30] = ("�O", "��", "��"),
                [0x31] = ("�P", "�", "��"),
                [0x32] = ("�Q", "��", "��"),
                [0x33] = ("�R", "�", "��"),
                [0x34] = ("�S", "��", "��"),
                [0x35] = ("�T", "�", "��"),
                [0x36] = ("�U", "��", "��"),
                [0x37] = ("�V", "�", "��"),
                [0x38] = ("�W", "��", "��"),
                [0x39] = ("�X", "�", "��"),
                [0x3A] = ("�Y", "��", "��"),
                [0x3B] = ("�Z", "�", "��"),
                [0x3C] = ("�[", "��", "��"),
                [0x3D] = ("�\", "�", "��"),
                [0x3E] = ("�]", "��", "��"),
                [0x3F] = ("�^", "�", "��"),
                [0x40] = ("�_", "��", "��"),
                [0x41] = ("�`", "�", "��"),
                [0x42] = ("�a", "��", "��"),
                [0x43] = ("�b", "�", "��"),
                [0x44] = ("�c", "�", "��"),
                [0x45] = ("�d", "��", "��"),
                [0x46] = ("�e", "�", "��"),
                [0x47] = ("�f", "��", "��"),
                [0x48] = ("�g", "�", "��"),
                [0x49] = ("�h", "��", "��"),
                [0x4A] = ("�i", "�", "��"),
                [0x4B] = ("�j", "�", "��"),
                [0x4C] = ("�k", "�", "��"),
                [0x4D] = ("�l", "�", "��"),
                [0x4E] = ("�m", "�", "��"),
                [0x4F] = ("�n", "�", "��"),
                [0x50] = ("�o", "��", "��"),
                [0x51] = ("�p", "��", "��"),
                [0x52] = ("�q", "�", "��"),
                [0x53] = ("�r", "��", "��"),
                [0x54] = ("�s", "��", "��"),
                [0x55] = ("�t", "�", "��"),
                [0x56] = ("�u", "��", "��"),
                [0x57] = ("�v", "��", "��"),
                [0x58] = ("�w", "�", "��"),
                [0x59] = ("�x", "��", "��"),
                [0x5A] = ("�y", "��", "��"),
                [0x5B] = ("�z", "�", "��"),
                [0x5C] = ("�{", "��", "��"),
                [0x5D] = ("�|", "��", "��"),
                [0x5E] = ("�}", "�", "��"),
                [0x5F] = ("�~", "�", "��"),
                [0x60] = ("��", "�", "��"),
                [0x61] = ("��", "�", "��"),
                [0x62] = ("��", "�", "��"),
                [0x63] = ("��", "�", "��"),
                [0x64] = ("��", "�", "��"),
                [0x65] = ("��", "�", "��"),
                [0x66] = ("��", "�", "��"),
                [0x67] = ("��", "�", "��"),
                [0x68] = ("��", "�", "��"),
                [0x69] = ("��", "�", "��"),
                [0x6A] = ("��", "�", "��"),
                [0x6B] = ("��", "�", "��"),
                [0x6C] = ("��", "�", "��"),
                [0x6D] = ("��", "�", "��"),
                [0x6E] = ("��", "��", "��"),
                [0x6F] = ("��", "�", "��"),
                [0x70] = ("��", "��", "��"),
                [0x71] = ("��", "��", "��"),
                [0x72] = ("��", "�", "��"),
                [0x73] = ("��", "�", "��"),
                [0x74] = ("��", "��", null),
                [0x75] = ("��", "��", null),
                [0x76] = ("��", "��", null),
                [0x77] = ("�R", "�R", "�T"),
                [0x78] = ("�S", "�S", "�U"),
                [0x79] = ("�[", "�", "�["),
                [0x7A] = ("�B", "�", "�B"),
                [0x7B] = ("�u", "�", "�u"),
                [0x7C] = ("�v", "�", "�v"),
                [0x7D] = ("�A", "�", "�A"),
                [0x7E] = ("�E", "�", "�E"),
            };
            var encoder = new B24Decoder();

            var bs = new byte[] {
                0x1b, 0x28, 0x31, // (GL <-) G0 <- �J�^�J�i�W��
                0x1b, 0x2a, 0x30, // (GR <-) G2 <- �Ђ炪�ȏW��
                0x00,             // 1������
                0x00,             // 2������
                0x89,             // NSZ
                0x00,             // 3�����ځi���p�J�i�j
            };

            foreach (var (code, v) in patterns)
            {
                var expected = (v.Item3 ?? v.Item1) + v.Item1 + v.Item2;
                bs[6] = (byte)(v.Item3 is null ? code : (code + 0x80));
                bs[7] = bs[9] = (byte)code;
                var actual = encoder.GetString(bs);
                //Assert.AreEqual(expected, actual, "Expected:<{0}>. Actual:<{1}>. 0x{2:X2}", AsUnicodeLiteral(expected), AsUnicodeLiteral(actual), code);
                Assert.Equal(expected, actual);
            }
        }

        /// <summary>
        /// "jisx0213-2004-8bit-std.txt" ���p�[�X����
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static (IReadOnlyDictionary<int, int[]> jisCodes, Dictionary<string, string> fullToHalf) ReadJisX0213File(string file)
        {
            var jisCodes = new Dictionary<int, int[]>();
            var fullToHalf = new Dictionary<string, string>();

            foreach (var line in File.ReadLines(file, Encoding.UTF8))
            {
                if (!line.Any() || line[0] == '#')
                    continue;

                var cols = line.Split('\t');

                if (cols[1].Length == 0)
                    continue;

                var kuTen = int.Parse(cols[0].Substring(2), NumberStyles.HexNumber);
                var windows = cols.FirstOrDefault(p => p.StartsWith("Windows: U+"));
                var fullwidth = cols.FirstOrDefault(p => p.StartsWith("Fullwidth: U+"));
                var codePoints = ParseCodePoints(windows ?? fullwidth ?? cols[1]);

                jisCodes.Add(kuTen, codePoints);
                if (fullwidth != null)
                {
                    var full = CodepointsToString(codePoints);
                    var half = CodepointsToString(ParseCodePoints(cols[1]));
                    if (fullToHalf.ContainsKey(full))
                        fullToHalf[full] = half;
                    else
                        fullToHalf.Add(full, half);
                }
            }

            return (jisCodes, fullToHalf);
        }

        /// <summary>
        /// "zen2han.txt" ���p�[�X����
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fullToHalf"></param>
        private void ReadZen2HanFile(string file, Dictionary<string, string> fullToHalf)
        {
            foreach (var line in File.ReadLines(file, Encoding.UTF8))
            {
                if (!line.Any() || line[0] == '#')
                    continue;

                var cols = line.Split('\t');

                if (cols.Length < 2)
                    continue;

                var full = cols[0];
                var half = cols[1];
                if (fullToHalf.ContainsKey(full))
                    fullToHalf[full] = half;
                else
                    fullToHalf.Add(full, half);
            }
        }

        /// <summary>
        /// "U+aaaa+bbbb" �̂悤�ɕ\������Ă���R�[�h�|�C���g����Aint[] { 0xaaaa, 0xbbbb } �ɕϊ�
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        private static int[] ParseCodePoints(string cp)
        {
            return cp.Split('+')
                .Skip(1)
                .Select(p => int.Parse(p, NumberStyles.HexNumber))
                .ToArray();
        }

        /// <summary>
        /// int �^�̃R�[�h�|�C���g��� string �ɕϊ�
        /// </summary>
        /// <param name="codePoints"></param>
        /// <returns></returns>
        private static string CodepointsToString(IEnumerable<int> codePoints)
        {
            return string.Join("", codePoints.Select(p => char.ConvertFromUtf32(p)));
        }

        private static string AsUnicodeLiteral(string str)
        {
            return string.Join("", str.Select(p => $"\\u{(int)p:X4}"));
        }
    }
}
