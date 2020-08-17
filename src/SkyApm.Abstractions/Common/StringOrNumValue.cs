/*
 * Licensed to the SkyAPM under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The SkyAPM licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using System;

namespace SkyApm.Common
{
    public struct StringOrNumValue<T> : IEquatable<StringOrNumValue<T>> where T : struct
    {
        private readonly Nullable<T> _numValue;
        private readonly string _stringValue;

        public StringOrNumValue(T value)
        {
            _numValue = value;
            _stringValue = null;
        }

        public bool HasValue => HasNumValue || HasStringValue;

        public bool HasNumValue => _numValue != null;

        public bool HasStringValue => _stringValue != null;

        public StringOrNumValue(string value)
        {
            _numValue = null;
            _stringValue = value;
        }

        public StringOrNumValue(T numValue, string stringValue)
        {
            _numValue = numValue;
            _stringValue = stringValue;
        }

        public T GetNumValue() => _numValue ?? default(T);

        public string GetStringValue() => _stringValue;

        public (string, T) GetValue()
        {
            return (_stringValue, _numValue ?? default(T));
        }

        public override string ToString()
        {
            if (HasNumValue) return _numValue.ToString();
            return _stringValue;
        }

        public bool Equals(StringOrNumValue<T> other)
        {
            return _numValue.Equals(other._numValue) && _stringValue == other._stringValue;
        }

        public override bool Equals(object obj)
        {
            return obj is StringOrNumValue<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _numValue.GetHashCode();
                if(_stringValue != null)
                    hashCode = (hashCode * 397) ^ _stringValue.GetHashCode();
                return hashCode;
            }
        }

        public static implicit operator StringOrNumValue<T>(string value) => new StringOrNumValue<T>(value);
        public static implicit operator StringOrNumValue<T>(T value) => new StringOrNumValue<T>(value);
        public static bool operator ==(StringOrNumValue<T> left, StringOrNumValue<T> right) => left.Equals(right);
        public static bool operator !=(StringOrNumValue<T> left, StringOrNumValue<T> right) => !left.Equals(right);
    }
}
