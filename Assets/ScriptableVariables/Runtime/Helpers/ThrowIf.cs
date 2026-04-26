using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OpenUtility.Exceptions
{
    public static class ThrowIf
    {
        public static void NotInt(string str, out int result)
        {
            if (!int.TryParse(str, out result))
                throw new FormatException($"The provided string '{str}' is not a valid integer.");
        }
        
        public static void NotFloat(string str, out float result)
        {
            if (!float.TryParse(str, out result))
                throw new FormatException($"The provided string '{str}' is not a valid float.");
        }
        
        public static void NullOrEmpty(string str)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentException("string is null or empty.");
        }

        public static void Empty(string str)
        {
            if (str is { Length: 0 })
                throw new ArgumentException("string is empty.");
        }

        public static void SystemObjectNull(Object obj)
        {
            if (obj == null)
                throw new NullReferenceException("System.Object reference not set to an instance of an object.");
        }

        public static void UnityObjectNull(UnityEngine.Object obj)
        {
            if (obj == null)
                throw new NullReferenceException("UnityEngine.Object reference not set to an instance of an object.");
        }

        public static void Negative(Single single)
        {
            if (single < 0)
                throw new ArgumentOutOfRangeException($"Value {single} is negative.");
        }

        public static void Negative(Int32 integer)
        {
            if (integer < 0)
                throw new ArgumentOutOfRangeException($"Value {integer} is negative.");
        }

        public static void Zero(Single single)
        {
            if (single == 0)
                throw new ArgumentOutOfRangeException("Value is zero.");
        }

        public static void Zero(Int32 integer)
        {
            if (integer == 0)
                throw new ArgumentOutOfRangeException("Value is zero.");
        }

        public static void ZeroOrNegative(Single single)
        {
            Zero(single);
            Negative(single);
        }

        public static void OutOfBounds<T>(T[] array, int index)
        {
            if (index < 0 || index >= array.Length)
                throw new IndexOutOfRangeException($"Index {index} is out of bounds for array of length {array.Length}.");
        }
        
        public static void OutOfBounds<T>(ICollection<T> list, int index)
        {
            if (index < 0 || index >= list.Count)
                throw new IndexOutOfRangeException($"Index {index} is out of bounds for list of count {list.Count}.");
        }

        public static void EmptyArray<T>(T[] array)
        {
            if (array.Length == 0)
                throw new ArgumentException("Array is empty.");
        }

        public static void EmptyCollection(ICollection collection)
        {
            if (collection.Count == 0)
                throw new ArgumentException("Collection is empty.");
        }

        public static void EmptyEnumerable<T>(IEnumerable<T> collection)
        {
            if (!collection.Any())
                throw new ArgumentException("Collection is empty.");
        }

        public static void NotDerivedFrom<T>(Type type)
        {
            if (!typeof(T).IsAssignableFrom(type))
                throw new ArgumentException($"Type {type.FullName} is not derived from {typeof(T).FullName}.");
        }

        public static void NotBool(int value, out bool result)
        {
            if (value == 0)
            {
                result = false;
            }
            else if (value == 1)
            {
                result = true;
            }
            else
            {
                throw new FormatException($"The provided integer '{value}' is not a valid boolean representation. Use 0 for false and 1 for true.");
            }
        }

        public static void SmallerThen(int value, int threshold)
        {
            if (value < threshold)
                throw new Exception($"Value {value} is smaller than the threshold {threshold}.");
        }
        
        public static void SmallerThen(int value, int threshold, string message)
        {
            if (value < threshold)
                throw new Exception(message);
        }

        public static void GreaterThen(int value, int threshold)
        {
            if (value > threshold)
                throw new Exception($"Value {value} is greater than the threshold {threshold}.");
        }
        
        public static void GreaterThen(int value, int threshold, string message)
        {
            if (value > threshold)
                throw new Exception(message);
        }
    }
}
