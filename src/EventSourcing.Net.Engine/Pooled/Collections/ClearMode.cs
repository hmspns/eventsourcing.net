/*
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

/// <summary>
/// This enum allows control over how data is treated when internal
/// arrays are returned to the ArrayPool. Be careful to understand 
/// what each option does before using anything other than the default
/// of Auto.
/// </summary>
public enum ClearMode
{
    /// <summary>
    /// <para><code>Auto</code> has different behavior depending on the host project's target framework.</para>
    /// <para>.NET Core 2.1: Reference types and value types that contain reference types are cleared
    /// when the internal arrays are returned to the pool. Value types that do not contain reference
    /// types are not cleared when returned to the pool.</para>
    /// <para>.NET Standard 2.0: All user types are cleared before returning to the pool, in case they
    /// contain reference types.
    /// For .NET Standard, Auto and Always have the same behavior.</para>
    /// </summary>
    Auto = 0,

    /// <summary>
    /// The <para><code>Always</code> setting has the effect of always clearing user types before returning to the pool.
    /// This is the default behavior on .NET Standard.</para><para>You might want to turn this on in a .NET Core project
    /// if you were concerned about sensitive data stored in value types leaking to other pars of your application.</para> 
    /// </summary>
    Always = 1,

    /// <summary>
    /// <para><code>Never</code> will cause pooled collections to never clear user types before returning them to the pool.</para>
    /// <para>You might want to use this setting in a .NET Standard project when you know that a particular collection stores
    /// only value types and you want the performance benefit of not taking time to reset array items to their default value.</para>
    /// <para>Be careful with this setting: if used for a collection that contains reference types, or value types that contain
    /// reference types, this setting could cause memory issues by making the garbage collector unable to clean up instances
    /// that are still being referenced by arrays sitting in the ArrayPool.</para>
    /// </summary>
    Never = 2
}