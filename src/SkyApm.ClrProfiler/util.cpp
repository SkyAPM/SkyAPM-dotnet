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

#include "util.h"

#include <iterator>
#include <string>
#include <vector>
#include <locale>
#include <functional>
#include <algorithm>
#include <sstream>
#include <codecvt>
#include <fmt/format.h>

namespace clrprofiler {

#if _MSC_VER >= 1900
	std::string utf16_to_utf8(std::u16string u16str)
	{
		std::wstring_convert<std::codecvt_utf8_utf16<int16_t>, int16_t> convert;
		const auto p = reinterpret_cast<const int16_t *>(u16str.data());
		return convert.to_bytes(p, p + u16str.size());
	}

	std::u16string utf8_to_utf16(std::string u8str)
	{
		std::wstring_convert<std::codecvt_utf8_utf16<int16_t>, int16_t> convert;
		const auto p = reinterpret_cast<const char *>(u8str.data());
		auto str = convert.from_bytes(p, p + u8str.size());
		std::u16string u16_str(str.begin(), str.end());
		return u16_str;
	}
#else
	std::string utf16_to_utf8(std::u16string utf16_string)
	{
		std::wstring_convert<std::codecvt_utf8_utf16<char16_t>, char16_t> convert;
		return convert.to_bytes(utf16_string);
	}

	std::u16string utf8_to_utf16(std::string utf8_string)
	{
		std::wstring_convert<std::codecvt_utf8_utf16<char16_t>, char16_t> convert;
		return convert.from_bytes(utf8_string);
	}
#endif

	std::string ToString(const WSTRING& wstr) 
	{
		const std::u16string u16str(reinterpret_cast<const char16_t*>(wstr.c_str()));
		return utf16_to_utf8(u16str);
	}

	WSTRING ToWString(const std::string& str)
	{
		std::u16string u16str = utf8_to_utf16(str);
		const std::basic_string<WCHAR> ptr = reinterpret_cast<const WCHAR*>(u16str.c_str());
		return std::basic_string<WCHAR>(ptr);
	}

	std::vector<WSTRING> Split(const WSTRING &s, const WCHAR wchar) {
		std::vector<WSTRING> str_array;
		WSTRINGSTREAM wss(s);
		WSTRING temp;
		while (std::getline(wss, temp, wchar)) {
			str_array.push_back(temp);
		}
		return str_array;
	}

	WSTRING Trim(const WSTRING &str) {
		if (str.length() == 0) {
			return W("");
		}

		const auto front = std::find_if_not(str.begin(), str.end(), [](int c) {return ::isspace(c); });
		const auto back = std::find_if_not(str.rbegin(), str.rend(), [](int c) {return ::isspace(c); }).base();
		return back <= front ? WSTRING() : WSTRING(front, back);
	}

	WSTRING GetEnvironmentValue(const WSTRING &name) {
#ifdef _WIN32
		const size_t max_env_size = 1024;
		WCHAR buf[max_env_size];
		GetEnvironmentVariable(name.data(), buf, max_env_size);
		return Trim(WSTRING(buf));
#else
		auto cstr = std::getenv(ToString(name).c_str());
		if (cstr == nullptr) {
			return W("");
		}
		std::string str(cstr);
		auto wstr = ToWString(str);
		return Trim(wstr);
#endif
	}

	constexpr char hex_map[] = { '0', '1', '2', '3', '4', '5', '6', '7',
				   '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

	WSTRING HexStr(const unsigned char *data, int len) {
		WSTRING s(len * 2, (char)' ');
		for (int i = 0; i < len; ++i) {
			s[2 * i] = hex_map[(data[i] & 0xF0) >> 4];
			s[2 * i + 1] = hex_map[data[i] & 0x0F];
		}
		return s;
	}

	std::vector<BYTE> HexToBytes(const std::string& hex) {
		std::vector<BYTE> bytes;
		for (unsigned int i = 0; i < hex.length(); i += 2) {
			std::string byteString = hex.substr(i, 2);
			auto byte = BYTE(strtol(byteString.c_str(), NULL, 16));
			bytes.push_back(byte);
		}
		return bytes;
	}

	static bool PROFILER_FLAG;
	void SetClrProfilerFlag(bool flag) {
		PROFILER_FLAG = flag;
	}

	WSTRING GetClrProfilerHome() {
		return  PROFILER_FLAG ? CORECLR_PROFILER_HOME : COR_PROFILER_HOME;
	}
}  // namespace clrprofiler
