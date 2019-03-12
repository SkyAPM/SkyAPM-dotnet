// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#include "util.h"

#include <cwctype>
#include <iterator>
#include <string>
#include <vector>

namespace trace {

template <typename Out>
void Split(const WSTRING &s, wchar_t delim, Out result) {
  size_t lpos = 0;
  for (size_t i = 0; i < s.length(); i++) {
    if (s[i] == delim) {
      *(result++) = s.substr(lpos, (i - lpos));
      lpos = i + 1;
    }
  }
  *(result++) = s.substr(lpos);
}

std::vector<WSTRING> Split(const WSTRING &s, wchar_t delim) {
  std::vector<WSTRING> elems;
  Split(s, delim, std::back_inserter(elems));
  return elems;
}

WSTRING Trim(const WSTRING &str) {
  if (str.length() == 0) {
    return ""_W;
  }

  WSTRING trimmed = str;

  auto lpos = trimmed.find_first_not_of(" \t"_W);
  if (lpos != WSTRING::npos && lpos > 0) {
    trimmed = trimmed.substr(lpos);
  }

  auto rpos = trimmed.find_last_not_of(" \t"_W);
  if (rpos != WSTRING::npos) {
    trimmed = trimmed.substr(0, rpos + 1);
  }

  return trimmed;
}

WSTRING GetEnvironmentValue(const WSTRING &name) {
#ifdef _WIN32
  const size_t max_buf_size = 4096;
  WCHAR buf[max_buf_size];
  const auto len = GetEnvironmentVariable(name.data(), buf, max_buf_size);
  return Trim(WSTRING(buf).substr(0, len));
#else
  auto cstr = std::getenv(ToString(name).c_str());
  if (cstr == nullptr) {
    return ""_W;
  }
  std::string str(cstr);
  auto wstr = ToWSTRING(str);
  return Trim(wstr);
#endif
}

std::vector<WSTRING> GetEnvironmentValues(const WSTRING &name,
                                          const wchar_t delim) {
  std::vector<WSTRING> values;
  for (auto s : Split(GetEnvironmentValue(name), delim)) {
    s = Trim(s);
    if (!s.empty()) {
      values.push_back(s);
    }
  }
  return values;
}

std::vector<WSTRING> GetEnvironmentValues(const WSTRING &name) {
  return GetEnvironmentValues(name, L';');
}

constexpr char HexMap[] = { '0', '1', '2', '3', '4', '5', '6', '7',
               '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

WSTRING HexStr(const unsigned char *data, int len) {
    WSTRING s(len * 2, ' ');
    for (int i = 0; i < len; ++i) {
        s[2 * i] = HexMap[(data[i] & 0xF0) >> 4];
        s[2 * i + 1] = HexMap[data[i] & 0x0F];
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
}  // namespace trace
