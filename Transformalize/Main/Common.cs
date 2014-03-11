﻿#region License

// /*
// Transformalize - Replicate, Transform, and Denormalize Your Data...
// Copyright (C) 2013 Dale Newman
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// */

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Transformalize.Configuration;
using Transformalize.Extensions;
using Transformalize.Libs.EnterpriseLibrary.Validation.Validators;

namespace Transformalize.Main {
    public static class Common {
        private const StringComparison IC = StringComparison.OrdinalIgnoreCase;
        private const string APPLICATION_FOLDER = @"\Tfl\";
        private static readonly char[] Slash = new[] { '\\' };

        public static Dictionary<string, byte> Validators = new Dictionary<string, byte> {
            {"containscharacters", 1},
            {"datetimerange", 1},
            {"domain", 1},
            {"isjson", 1},
            {"notnull", 1},
            {"fieldcomparison", 1},
            {"range", 1},
            {"regex", 1},
            {"relativedatetime", 1},
            {"stringlength", 1},
            {"typeconversion", 1}
        };

        public static bool IsValidator(string method) {
            return Validators.ContainsKey(method.ToLower());
        }

        public static Dictionary<string, Func<string, object>> ConversionMap = new Dictionary<string, Func<string, object>> {
            {"string", (x => x)},
            {"xml", (x => x)},
            {"int16", (x => Convert.ToInt16(x))},
            {"int32", (x => Convert.ToInt32(x))},
            {"int", (x => Convert.ToInt32(x))},
            {"int64", (x => Convert.ToInt64(x))},
            {"long", (x => Convert.ToInt64(x))},
            {"double", (x => Convert.ToDouble(x))},
            {"decimal", (x => decimal.Parse(x, NumberStyles.Float | NumberStyles.AllowThousands | NumberStyles.AllowCurrencySymbol, (IFormatProvider)CultureInfo.CurrentCulture.GetFormat(typeof(NumberFormatInfo))))},
            {"char", (x => Convert.ToChar(x))},
            {"datetime", (x => Convert.ToDateTime(x))},
            {"boolean", (x => Convert.ToBoolean(x))},
            {"single", (x => Convert.ToSingle(x))},
            {"guid", (x => Guid.Parse(x))},
            {"byte", (x => Convert.ToByte(x))},
            {"byte[]", (HexStringToByteArray)},
            {"rowversion", (HexStringToByteArray)}
        };

        public static Dictionary<ComparisonOperator, Func<object, object, bool>> CompareMap = new Dictionary<ComparisonOperator, Func<object, object, bool>>() {
            {ComparisonOperator.Equal, ((x, y) => x.Equals(y))},
            {ComparisonOperator.NotEqual, ((x, y) => !x.Equals(y))},
            {ComparisonOperator.GreaterThan, ((x, y) => ((IComparable) x).CompareTo(y) > 0)},
            {ComparisonOperator.GreaterThanEqual, ((x, y) => x.Equals(y) || ((IComparable) x).CompareTo(y) > 0)},
            {ComparisonOperator.LessThan, ((x, y) => ((IComparable) x).CompareTo(y) < 0)},
            {ComparisonOperator.LessThanEqual, ((x, y) => x.Equals(y) || ((IComparable) x).CompareTo(y) < 0)}
        };

        public static Dictionary<string, Func<object, object>> GetObjectConversionMap() {
            return new Dictionary<string, Func<object, object>> {
                {"string", (x => x)},
                {"xml", (x => x)},
                {"guid", (x => Guid.Parse(x.ToString()))},
                {"int16", (x => Convert.ToInt16(x))},
                {"int", (x => Convert.ToInt32(x))},
                {"int32", (x => Convert.ToInt32(x))},
                {"int64", (x => Convert.ToInt64(x))},
                {"long", (x => Convert.ToInt64(x))},
                {"double", (x => Convert.ToDouble(x))},
                {"decimal", (x => decimal.Parse(x.ToString(), NumberStyles.Float | NumberStyles.AllowThousands | NumberStyles.AllowCurrencySymbol, (IFormatProvider)CultureInfo.CurrentCulture.GetFormat(typeof(NumberFormatInfo))))},
                {"char", (x => Convert.ToChar(x))},
                {"datetime", (x => Convert.ToDateTime(x))},
                {"boolean", (x => Convert.ToBoolean(x))},
                {"bool", (x => Convert.ToBoolean(x))},
                {"single", (x => Convert.ToSingle(x))},
                {"byte", (x => Convert.ToByte(x))},
                {"byte[]", (x => HexStringToByteArray(x.ToString()))},
                {"rowversion", (x => HexStringToByteArray(x.ToString()))}
            };
        }

        public static Func<KeyValuePair<string, Field>, bool> FieldFinder(ParameterConfigurationElement p) {
            if (p.Entity != string.Empty)
                return f => f.Value.Alias.Equals(p.Field, IC) && f.Value.Entity.Equals(p.Entity, IC) || f.Value.Name.Equals(p.Field, IC) && f.Value.Entity.Equals(p.Entity, IC);
            return f => f.Value.Alias.Equals(p.Field, IC) || f.Value.Name.Equals(p.Field, IC);
        }

        public static Func<Field, bool> FieldFinder(string nameOrAlias) {
            return v => v.Name.Equals(nameOrAlias, IC) || v.Alias.Equals(nameOrAlias, IC);
        }

        public static string GetAlias(FieldConfigurationElement element, bool usePrefix, string prefix) {
            return usePrefix && element.Alias.Equals(element.Name) && !string.IsNullOrEmpty(prefix) ? prefix + element.Name : element.Alias;
        }

        public static string GetTemporaryFolder(string processName) {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).TrimEnd(Slash) + APPLICATION_FOLDER + processName;

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return folder;
        }

        public static string GetTemporarySubFolder(string processName, string subFolder) {
            var f = Path.Combine(GetTemporaryFolder(processName), subFolder);
            if (!Directory.Exists(f)) {
                Directory.CreateDirectory(f);
            }
            return f;
        }

        public static byte[] HexStringToByteArray(string hex) {
            var bytes = new byte[hex.Length / 2];
            var hexValue = new[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };

            for (int x = 0, i = 0; i < hex.Length; i += 2, x += 1) {
                bytes[x] = (byte)(hexValue[Char.ToUpper(hex[i + 0]) - '0'] << 4 |
                                  hexValue[Char.ToUpper(hex[i + 1]) - '0']);
            }

            return bytes;
        }

        public static string BytesToHexString(byte[] bytes) {
            var c = new char[bytes.Length * 2];
            for (var i = 0; i < bytes.Length; i++) {
                var b = bytes[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }
            return new string(c);
        }

        public static string ToSimpleType(string type) {
            var result = type.ToLower();
            if (result == "int") {
                result = "int32";
            }
            if (result == "bool") {
                result = "boolean";
            }
            return result.Replace("system.", string.Empty);
        }

        public static Type ToSystemType(string simpleType) {
            simpleType = ToSimpleType(simpleType);
            if (simpleType == "byte[]")
                return typeof(byte[]);
            if (simpleType == "int")
                return typeof(int);
            if (simpleType == "datetime")
                return typeof(DateTime);
            if (simpleType == "rowversion")
                return typeof(byte[]);
            var fullName = "System." + simpleType[0].ToString(CultureInfo.InvariantCulture).ToUpper() + simpleType.Substring(1);
            return Type.GetType(fullName);
        }

        public static int DateTimeToInt32(DateTime date) {
            return (int)(date - new DateTime(1, 1, 1)).TotalDays + 1;
        }

        public static DateTime Int32ToDateTime(int timeKey) {
            return new DateTime(1, 1, 1).AddDays(timeKey - 1);
        }

        public static string LogLength(string value, int totalWidth = 20) {
            return value.Length > totalWidth ? value.Left(totalWidth) : value.PadRight(totalWidth, '.');
        }

        public static string EntityOutputName(Entity entity, string processName) {
            return entity.PrependProcessNameToOutputName ? string.Concat(processName.Replace("-",string.Empty) , entity.Alias).Replace(" ", string.Empty) : entity.Alias;
        }

        public static bool AreEqual(byte[] b1, byte[] b2) {
            return b1.Length == b2.Length && b1.SequenceEqual(b2);
        }

        public static byte[] Max(byte[] b1, byte[] b2) {
            var minLength = Math.Min(b1.Length, b2.Length);
            if (minLength == 0)  // return longest, when comparable are equal
            {
                return b1.Length > b2.Length ? b1 : b2;
            }

            for (var i = 0; i < minLength; i++) {
                if (b1[i] != b2[i]) {
                    return b1[i] > b2[i] ? b1 : b2;  // return first one with a bigger byte
                }
            }

            return b1.Length > b2.Length ? b1 : b2; // return longest, when comparable are equal

        }

    }
}