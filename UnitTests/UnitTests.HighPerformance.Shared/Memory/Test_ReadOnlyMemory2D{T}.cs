﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Toolkit.HighPerformance.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.HighPerformance.Memory
{
    [TestClass]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649", Justification = "Test class for generic type")]
    public class Test_ReadOnlyMemory2DT
    {
        [TestCategory("ReadOnlyMemory2DT")]
        [TestMethod]
        public void Test_ReadOnlyMemory2DT_Empty()
        {
            ReadOnlyMemory2D<int> empty1 = default;

            Assert.IsTrue(empty1.IsEmpty);
            Assert.AreEqual(empty1.Size, 0);
            Assert.AreEqual(empty1.Width, 0);
            Assert.AreEqual(empty1.Height, 0);

            ReadOnlyMemory2D<string> empty2 = ReadOnlyMemory2D<string>.Empty;

            Assert.IsTrue(empty2.IsEmpty);
            Assert.AreEqual(empty2.Size, 0);
            Assert.AreEqual(empty2.Width, 0);
            Assert.AreEqual(empty2.Height, 0);
        }

        [TestCategory("ReadOnlyMemory2DT")]
        [TestMethod]
        public void Test_ReadOnlyMemory2DT_Array1DConstructor()
        {
            int[] array =
            {
                1, 2, 3, 4, 5, 6
            };

            ReadOnlyMemory2D<int> span2d = new ReadOnlyMemory2D<int>(array, 1, 2, 2, 1);

            Assert.IsFalse(span2d.IsEmpty);
            Assert.AreEqual(span2d.Size, 4);
            Assert.AreEqual(span2d.Width, 2);
            Assert.AreEqual(span2d.Height, 2);
            Assert.AreEqual(span2d.Span[0, 0], 2);
            Assert.AreEqual(span2d.Span[1, 1], 6);

            Assert.ThrowsException<ArrayTypeMismatchException>(() => new ReadOnlyMemory2D<object>(new string[1], 1, 1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ReadOnlyMemory2D<int>(array, -99, 1, 1, 1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ReadOnlyMemory2D<int>(array, 0, -10, 1, 1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ReadOnlyMemory2D<int>(array, 0, 1, 1, -1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ReadOnlyMemory2D<int>(array, 0, 1, -100, 1));
            Assert.ThrowsException<ArgumentException>(() => new ReadOnlyMemory2D<int>(array, 0, 10, 1, 120));
        }

        [TestCategory("ReadOnlyMemory2DT")]
        [TestMethod]
        public void Test_ReadOnlyMemory2DT_Array2DConstructor_1()
        {
            int[,] array =
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };

            ReadOnlyMemory2D<int> span2d = new ReadOnlyMemory2D<int>(array);

            Assert.IsFalse(span2d.IsEmpty);
            Assert.AreEqual(span2d.Size, 6);
            Assert.AreEqual(span2d.Width, 3);
            Assert.AreEqual(span2d.Height, 2);
            Assert.AreEqual(span2d.Span[0, 1], 2);
            Assert.AreEqual(span2d.Span[1, 2], 6);

            Assert.ThrowsException<ArrayTypeMismatchException>(() => new ReadOnlyMemory2D<object>(new string[1, 2]));
        }

        [TestCategory("ReadOnlyMemory2DT")]
        [TestMethod]
        public void Test_ReadOnlyMemory2DT_Array2DConstructor_2()
        {
            int[,] array =
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };

            ReadOnlyMemory2D<int> span2d = new ReadOnlyMemory2D<int>(array, 0, 1, 2, 2);

            Assert.IsFalse(span2d.IsEmpty);
            Assert.AreEqual(span2d.Size, 4);
            Assert.AreEqual(span2d.Width, 2);
            Assert.AreEqual(span2d.Height, 2);
            Assert.AreEqual(span2d.Span[0, 0], 2);
            Assert.AreEqual(span2d.Span[1, 1], 6);

            Assert.ThrowsException<ArrayTypeMismatchException>(() => new ReadOnlyMemory2D<object>(new string[1, 2], 0, 0, 2, 2));
        }

        [TestCategory("ReadOnlyMemory2DT")]
        [TestMethod]
        public void Test_ReadOnlyMemory2DT_Array3DConstructor_1()
        {
            int[,,] array =
            {
                {
                    { 1, 2, 3 },
                    { 4, 5, 6 }
                },
                {
                    { 10, 20, 30 },
                    { 40, 50, 60 }
                }
            };

            ReadOnlyMemory2D<int> span2d = new ReadOnlyMemory2D<int>(array, 1);

            Assert.IsFalse(span2d.IsEmpty);
            Assert.AreEqual(span2d.Size, 6);
            Assert.AreEqual(span2d.Width, 3);
            Assert.AreEqual(span2d.Height, 2);
            Assert.AreEqual(span2d.Span[0, 1], 20);
            Assert.AreEqual(span2d.Span[1, 2], 60);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ReadOnlyMemory2D<int>(array, -1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ReadOnlyMemory2D<int>(array, 20));
        }

        [TestCategory("ReadOnlyMemory2DT")]
        [TestMethod]
        public void Test_ReadOnlyMemory2DT_Array3DConstructor_2()
        {
            int[,,] array =
            {
                {
                    { 1, 2, 3 },
                    { 4, 5, 6 }
                },
                {
                    { 10, 20, 30 },
                    { 40, 50, 60 }
                }
            };

            ReadOnlyMemory2D<int> span2d = new ReadOnlyMemory2D<int>(array, 1, 0, 1, 2, 2);

            Assert.IsFalse(span2d.IsEmpty);
            Assert.AreEqual(span2d.Size, 4);
            Assert.AreEqual(span2d.Width, 2);
            Assert.AreEqual(span2d.Height, 2);
            Assert.AreEqual(span2d.Span[0, 0], 20);
            Assert.AreEqual(span2d.Span[1, 1], 60);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ReadOnlyMemory2D<int>(array, -1, 1, 1, 1, 1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ReadOnlyMemory2D<int>(array, 1, -1, 1, 1, 1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ReadOnlyMemory2D<int>(array, 1, 1, -1, 1, 1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ReadOnlyMemory2D<int>(array, 1, 1, 1, -1, 1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ReadOnlyMemory2D<int>(array, 1, 1, 1, 1, -1));
        }

#if !WINDOWS_UWP
        [TestCategory("ReadOnlyMemory2DT")]
        [TestMethod]
        public void Test_ReadOnlyMemory2DT_ReadOnlyMemoryConstructor()
        {
            ReadOnlyMemory<int> memory = new[]
            {
                1, 2, 3, 4, 5, 6
            };

            ReadOnlyMemory2D<int> span2d = new ReadOnlyMemory2D<int>(memory, 1, 2, 2, 1);

            Assert.IsFalse(span2d.IsEmpty);
            Assert.AreEqual(span2d.Size, 4);
            Assert.AreEqual(span2d.Width, 2);
            Assert.AreEqual(span2d.Height, 2);
            Assert.AreEqual(span2d.Span[0, 0], 2);
            Assert.AreEqual(span2d.Span[1, 1], 6);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ReadOnlyMemory2D<int>(memory, -99, 1, 1, 1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ReadOnlyMemory2D<int>(memory, 0, -10, 1, 1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ReadOnlyMemory2D<int>(memory, 0, 1, 1, -1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ReadOnlyMemory2D<int>(memory, 0, 1, -100, 1));
            Assert.ThrowsException<ArgumentException>(() => new ReadOnlyMemory2D<int>(memory, 0, 10, 1, 120));
        }
#endif

        [TestCategory("ReadOnlyMemory2DT")]
        [TestMethod]
        public void Test_ReadOnlyMemory2DT_Slice_1()
        {
            int[,] array =
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };

            ReadOnlyMemory2D<int> span2d = new ReadOnlyMemory2D<int>(array);

            ReadOnlyMemory2D<int> slice1 = span2d.Slice(1, 1, 1, 2);

            Assert.AreEqual(slice1.Size, 2);
            Assert.AreEqual(slice1.Height, 1);
            Assert.AreEqual(slice1.Width, 2);
            Assert.AreEqual(slice1.Span[0, 0], 5);
            Assert.AreEqual(slice1.Span[0, 1], 6);

            ReadOnlyMemory2D<int> slice2 = span2d.Slice(0, 1, 2, 2);

            Assert.AreEqual(slice2.Size, 4);
            Assert.AreEqual(slice2.Height, 2);
            Assert.AreEqual(slice2.Width, 2);
            Assert.AreEqual(slice2.Span[0, 0], 2);
            Assert.AreEqual(slice2.Span[1, 0], 5);
            Assert.AreEqual(slice2.Span[1, 1], 6);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ReadOnlyMemory2D<int>(array).Slice(-1, 1, 1, 1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ReadOnlyMemory2D<int>(array).Slice(1, -1, 1, 1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ReadOnlyMemory2D<int>(array).Slice(1, 1, 1, -1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ReadOnlyMemory2D<int>(array).Slice(1, 1, -1, 1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ReadOnlyMemory2D<int>(array).Slice(10, 1, 1, 1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ReadOnlyMemory2D<int>(array).Slice(1, 12, 1, 12));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ReadOnlyMemory2D<int>(array).Slice(1, 1, 55, 1));
        }

        [TestCategory("ReadOnlyMemory2DT")]
        [TestMethod]
        public void Test_ReadOnlyMemory2DT_Slice_2()
        {
            int[,] array =
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };

            ReadOnlyMemory2D<int> span2d = new ReadOnlyMemory2D<int>(array);

            ReadOnlyMemory2D<int> slice1 = span2d.Slice(0, 0, 2, 2);

            Assert.AreEqual(slice1.Size, 4);
            Assert.AreEqual(slice1.Height, 2);
            Assert.AreEqual(slice1.Width, 2);
            Assert.AreEqual(slice1.Span[0, 0], 1);
            Assert.AreEqual(slice1.Span[1, 1], 5);

            ReadOnlyMemory2D<int> slice2 = slice1.Slice(1, 0, 1, 2);

            Assert.AreEqual(slice2.Size, 2);
            Assert.AreEqual(slice2.Height, 1);
            Assert.AreEqual(slice2.Width, 2);
            Assert.AreEqual(slice2.Span[0, 0], 4);
            Assert.AreEqual(slice2.Span[0, 1], 5);

            ReadOnlyMemory2D<int> slice3 = slice2.Slice(0, 1, 1, 1);

            Assert.AreEqual(slice3.Size, 1);
            Assert.AreEqual(slice3.Height, 1);
            Assert.AreEqual(slice3.Width, 1);
            Assert.AreEqual(slice3.Span[0, 0], 5);
        }

        [TestCategory("ReadOnlyMemory2DT")]
        [TestMethod]
        public void Test_ReadOnlyMemory2DT_TryGetReadOnlyMemory_1()
        {
            int[,] array =
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };

            ReadOnlyMemory2D<int> span2d = new ReadOnlyMemory2D<int>(array);

            bool success = span2d.TryGetMemory(out ReadOnlyMemory<int> memory);

            Assert.IsFalse(success);
            Assert.IsTrue(memory.IsEmpty);
        }

        [TestCategory("ReadOnlyMemory2DT")]
        [TestMethod]
        public void Test_ReadOnlyMemory2DT_TryGetReadOnlyMemory_2()
        {
            int[] array = { 1, 2, 3, 4 };

            ReadOnlyMemory2D<int> span2d = new ReadOnlyMemory2D<int>(array, 2, 2);

            bool success = span2d.TryGetMemory(out ReadOnlyMemory<int> memory);

            Assert.IsTrue(success);
            Assert.AreEqual(memory.Length, array.Length);
            Assert.AreEqual(memory.Span[2], 3);
        }

#if !WINDOWS_UWP
        [TestCategory("ReadOnlyMemory2DT")]
        [TestMethod]
        public void Test_ReadOnlyMemory2DT_TryGetReadOnlyMemory_3()
        {
            ReadOnlyMemory<int> data = new[] { 1, 2, 3, 4 };

            ReadOnlyMemory2D<int> span2d = new ReadOnlyMemory2D<int>(data, 2, 2);

            bool success = span2d.TryGetMemory(out ReadOnlyMemory<int> memory);

            Assert.IsTrue(success);
            Assert.AreEqual(memory.Length, data.Length);
            Assert.AreEqual(memory.Span[2], 3);
        }
#endif

        [TestCategory("ReadOnlyMemory2DT")]
        [TestMethod]
        public unsafe void Test_ReadOnlyMemory2DT_Pin_1()
        {
            int[] array = { 1, 2, 3, 4 };

            ReadOnlyMemory2D<int> span2d = new ReadOnlyMemory2D<int>(array, 2, 2);

            using var pin = span2d.Pin();

            Assert.AreEqual(((int*)pin.Pointer)[0], 1);
            Assert.AreEqual(((int*)pin.Pointer)[3], 4);
        }

        [TestCategory("ReadOnlyMemory2DT")]
        [TestMethod]
        public unsafe void Test_ReadOnlyMemory2DT_Pin_2()
        {
            int[] array = { 1, 2, 3, 4 };

            ReadOnlyMemory2D<int> span2d = new ReadOnlyMemory2D<int>(array, 2, 2);

            using var pin = span2d.Pin();

            Assert.AreEqual(((int*)pin.Pointer)[0], 1);
            Assert.AreEqual(((int*)pin.Pointer)[3], 4);
        }

        [TestCategory("ReadOnlyMemory2DT")]
        [TestMethod]
        public void Test_ReadOnlyMemory2DT_ToArray_1()
        {
            int[,] array =
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };

            ReadOnlyMemory2D<int> span2d = new ReadOnlyMemory2D<int>(array);

            int[,] copy = span2d.ToArray();

            Assert.AreEqual(copy.GetLength(0), array.GetLength(0));
            Assert.AreEqual(copy.GetLength(1), array.GetLength(1));

            CollectionAssert.AreEqual(array, copy);
        }

        [TestCategory("ReadOnlyMemory2DT")]
        [TestMethod]
        public void Test_ReadOnlyMemory2DT_ToArray_2()
        {
            int[,] array =
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };

            ReadOnlyMemory2D<int> span2d = new ReadOnlyMemory2D<int>(array, 0, 0, 2, 2);

            int[,] copy = span2d.ToArray();

            Assert.AreEqual(copy.GetLength(0), 2);
            Assert.AreEqual(copy.GetLength(1), 2);

            int[,] expected =
            {
                { 1, 2 },
                { 4, 5 }
            };

            CollectionAssert.AreEqual(expected, copy);
        }

        [TestCategory("ReadOnlyMemory2DT")]
        [TestMethod]
        public void Test_ReadOnlyMemory2DT_Equals()
        {
            int[,] array =
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };

            ReadOnlyMemory2D<int> span2d = new ReadOnlyMemory2D<int>(array);

            Assert.IsFalse(span2d.Equals(null));
            Assert.IsFalse(span2d.Equals(new ReadOnlyMemory2D<int>(array, 0, 1, 2, 2)));
            Assert.IsTrue(span2d.Equals(new ReadOnlyMemory2D<int>(array)));
            Assert.IsTrue(span2d.Equals(span2d));
        }

        [TestCategory("ReadOnlyMemory2DT")]
        [TestMethod]
        public void Test_ReadOnlyMemory2DT_GetHashCode()
        {
            Assert.AreEqual(ReadOnlyMemory2D<int>.Empty.GetHashCode(), 0);

            int[,] array =
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };

            ReadOnlyMemory2D<int> span2d = new ReadOnlyMemory2D<int>(array);

            int a = span2d.GetHashCode(), b = span2d.GetHashCode();

            Assert.AreEqual(a, b);

            int c = new ReadOnlyMemory2D<int>(array, 0, 1, 2, 2).GetHashCode();

            Assert.AreNotEqual(a, c);
        }

        [TestCategory("ReadOnlyMemory2DT")]
        [TestMethod]
        public void Test_ReadOnlyMemory2DT_ToString()
        {
            int[,] array =
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };

            ReadOnlyMemory2D<int> span2d = new ReadOnlyMemory2D<int>(array);

            string text = span2d.ToString();

            const string expected = "Microsoft.Toolkit.HighPerformance.Memory.ReadOnlyMemory2D<System.Int32>[2, 3]";

            Assert.AreEqual(text, expected);
        }
    }
}