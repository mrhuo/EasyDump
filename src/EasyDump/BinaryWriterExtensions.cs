﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace EasyDump
{
    internal static class BinaryWriterExtensions
    {
        public static void WriteString(this BinaryWriter @this, string value)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            @this.Write(value != null ? (value.Length + 1) : 0);
            if (value != null)
                @this.Write(Encoding.Unicode.GetBytes(value + '\0'));
        }

    }
}