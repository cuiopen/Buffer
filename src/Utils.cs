// Copyright (c) 2016 Yamool. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

namespace Yamool.Buffers
{
    using System;

    internal static class Utils
    {
        internal static int Log2(int value)
        {
            var num = 0;
            while ((value >>= 1) > 0)
            {
                num++;
            }
            return num;
        }
    }
}
