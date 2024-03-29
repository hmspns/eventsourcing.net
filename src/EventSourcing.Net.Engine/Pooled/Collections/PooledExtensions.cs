﻿/*
 * The MIT License (MIT)
 *
 * Copyright (c) Joel Mueller
 *
 * All rights reserved.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

namespace EventSourcing.Net.Engine.Pooled.Collections;

using System;
using System.Collections.Generic;

internal static class PooledExtensions
{
    internal static PooledList<T> ToPooledList<T>(this IEnumerable<T> items)
    {
        return new PooledList<T>(items);
    }

    internal static PooledList<T> ToPooledList<T>(this IEnumerable<T> items, int suggestCapacity)
    {
        return new PooledList<T>(items, suggestCapacity);
    }

    internal static PooledList<T> ToPooledList<T>(this T[] array)
    {
        return new PooledList<T>(array.AsSpan());
    }

    internal static PooledList<T> ToPooledList<T>(this ReadOnlySpan<T> span)
    {
        return new PooledList<T>(span);
    }

    internal static PooledList<T> ToPooledList<T>(this Span<T> span)
    {
        return new PooledList<T>(span);
    }

    internal static PooledList<T> ToPooledList<T>(this ReadOnlyMemory<T> memory)
    {
        return new PooledList<T>(memory.Span);
    }

    internal static PooledList<T> ToPooledList<T>(this Memory<T> memory)
    {
        return new PooledList<T>(memory.Span);
    }
}