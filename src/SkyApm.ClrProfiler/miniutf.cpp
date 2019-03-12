/* Copyright (c) 2013 Dropbox, Inc.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

#include "miniutf.hpp"

#include <algorithm>

namespace miniutf {

#include "miniutfdata.h"

/* * * * * * * * * *
 * Encoding
 * * * * * * * * * */

void utf8_encode(char32_t pt, std::string& out) {
  if (pt < 0x80) {
    out += static_cast<char>(pt);
  } else if (pt < 0x800) {
    out += {static_cast<char>((pt >> 6) | 0xC0),
            static_cast<char>((pt & 0x3F) | 0x80)};
  } else if (pt < 0x10000) {
    out += {static_cast<char>((pt >> 12) | 0xE0),
            static_cast<char>(((pt >> 6) & 0x3F) | 0x80),
            static_cast<char>((pt & 0x3F) | 0x80)};
  } else if (pt < 0x110000) {
    out += {static_cast<char>((pt >> 18) | 0xF0),
            static_cast<char>(((pt >> 12) & 0x3F) | 0x80),
            static_cast<char>(((pt >> 6) & 0x3F) | 0x80),
            static_cast<char>((pt & 0x3F) | 0x80)};
  } else {
#pragma warning(disable : 4309)
    out += {static_cast<char>(0xEF), static_cast<char>(0xBF),
            static_cast<char>(0xBD)};  // U+FFFD
#pragma warning(default : 4309)
  }
}

void utf16_encode(char32_t pt, std::u16string& out) {
  if (pt < 0x10000) {
    out += static_cast<char16_t>(pt);
  } else if (pt < 0x110000) {
    out += {static_cast<char16_t>(((pt - 0x10000) >> 10) + 0xD800),
            static_cast<char16_t>((pt & 0x3FF) + 0xDC00)};
  } else {
    out += 0xFFFD;
  }
}

/* * * * * * * * * *
 * Decoding logic
 * * * * * * * * * */

struct offset_pt {
  int offset;
  char32_t pt;
};

static constexpr const offset_pt invalid_pt = {-1, 0};

/*
 * Decode a codepoint starting at str[i], and return the number of code units
 * (bytes, for UTF-8) consumed and the result. If no valid codepoint is at
 * str[i], return invalid_pt.
 */
static offset_pt utf8_decode_check(const std::string& str,
                                   std::string::size_type i) {
  uint32_t b0, b1, b2, b3;

  b0 = static_cast<unsigned char>(str[i]);

  if (b0 < 0x80) {
    // 1-byte character
    return {1, b0};
  } else if (b0 < 0xC0) {
    // Unexpected continuation byte
    return invalid_pt;
  } else if (b0 < 0xE0) {
    // 2-byte character
    if (((b1 = str[i + 1]) & 0xC0) != 0x80) return invalid_pt;

    char32_t pt = (b0 & 0x1F) << 6 | (b1 & 0x3F);
    if (pt < 0x80) return invalid_pt;

    return {2, pt};
  } else if (b0 < 0xF0) {
    // 3-byte character
    if (((b1 = str[i + 1]) & 0xC0) != 0x80) return invalid_pt;
    if (((b2 = str[i + 2]) & 0xC0) != 0x80) return invalid_pt;

    char32_t pt = (b0 & 0x0F) << 12 | (b1 & 0x3F) << 6 | (b2 & 0x3F);
    if (pt < 0x800) return invalid_pt;

    return {3, pt};
  } else if (b0 < 0xF8) {
    // 4-byte character
    if (((b1 = str[i + 1]) & 0xC0) != 0x80) return invalid_pt;
    if (((b2 = str[i + 2]) & 0xC0) != 0x80) return invalid_pt;
    if (((b3 = str[i + 3]) & 0xC0) != 0x80) return invalid_pt;

    char32_t pt =
        (b0 & 0x0F) << 18 | (b1 & 0x3F) << 12 | (b2 & 0x3F) << 6 | (b3 & 0x3F);
    if (pt < 0x10000 || pt >= 0x110000) return invalid_pt;

    return {4, pt};
  } else {
    // Codepoint out of range
    return invalid_pt;
  }
}

// UTF-16 decode helpers.
static inline bool is_high_surrogate(char16_t c) {
  return (c >= 0xD800) && (c < 0xDC00);
}
static inline bool is_low_surrogate(char16_t c) {
  return (c >= 0xDC00) && (c < 0xE000);
}

/*
 * Like utf8_decode_check, but for UTF-16.
 */
static offset_pt utf16_decode_check(const std::u16string& str,
                                    std::u16string::size_type i) {
  if (is_high_surrogate(str[i]) && is_low_surrogate(str[i + 1])) {
    // High surrogate followed by low surrogate
    char32_t pt = (((str[i] - 0xD800) << 10) | (str[i + 1] - 0xDC00)) + 0x10000;
    return {2, pt};
  } else if (is_high_surrogate(str[i]) || is_low_surrogate(str[i])) {
    // High surrogate *not* followed by low surrogate, or unpaired low surrogate
    return invalid_pt;
  } else {
    return {1, str[i]};
  }
}

/*
 * UTF-32 is very easy to check.
 */
static offset_pt utf32_decode_check(const std::u32string& str,
                                    std::u32string::size_type i) {
  if (str[i] < 0x110000) {
    return {1, str[i]};
  } else {
    return invalid_pt;
  }
}

/* * * * * * * * * *
 * Decoding wrappers
 * * * * * * * * * */

char32_t utf8_decode(const std::string& str, std::string::size_type& i,
                     bool* replacement_flag) {
  offset_pt res = utf8_decode_check(str, i);
  if (res.offset < 0) {
    if (replacement_flag) *replacement_flag = true;
    i += 1;
    return 0xFFFD;
  } else {
    i += res.offset;
    return res.pt;
  }
}

char32_t utf16_decode(const std::u16string& str, std::u16string::size_type& i,
                      bool* replacement_flag) {
  offset_pt res = utf16_decode_check(str, i);
  if (res.offset < 0) {
    if (replacement_flag) *replacement_flag = true;
    i += 1;
    return 0xFFFD;
  } else {
    i += res.offset;
    return res.pt;
  }
}

/* * * * * * * * * *
 * Checking
 * * * * * * * * * */

template <typename Tfunc, typename Tstring>
bool check_helper(const Tfunc& func, const Tstring& str) {
  for (typename Tstring::size_type i = 0; i < str.length();) {
    offset_pt res = func(str, i);
    if (res.offset < 0) return false;
    i += res.offset;
  }
  return true;
}

bool utf8_check(const std::string& str) {
  return check_helper(utf8_decode_check, str);
}
bool utf16_check(const std::u16string& str) {
  return check_helper(utf16_decode_check, str);
}
bool utf32_check(const std::u32string& str) {
  return check_helper(utf32_decode_check, str);
}

/* * * * * * * * * *
 * Conversion
 * * * * * * * * * */

std::u32string to_utf32(const std::string& str) {
  std::u32string out;
  out.reserve(str.length());  // likely overallocate
  for (std::string::size_type i = 0; i < str.length();)
    out += utf8_decode(str, i);
  return out;
}

std::u16string to_utf16(const std::string& str) {
  std::u16string out;
  out.reserve(str.length());  // likely overallocate
  for (std::string::size_type i = 0; i < str.length();)
    utf16_encode(utf8_decode(str, i), out);
  return out;
}

std::string to_utf8(const std::u16string& str) {
  std::string out;
  out.reserve(str.length() * 3 / 2);  // estimate
  for (std::u16string::size_type i = 0; i < str.length();)
    utf8_encode(utf16_decode(str, i), out);
  return out;
}

std::string to_utf8(const std::u32string& str) {
  std::string out;
  out.reserve(str.length() * 3 / 2);  // estimate
  for (char32_t pt : str) utf8_encode(pt, out);
  return out;
}

/* * * * * * * * * *
 * Lowercase
 * * * * * * * * * */

std::string lowercase(const std::string& str) {
  std::string out;
  out.reserve(str.size());
  for (size_t i = 0; i < str.length();) {
    int32_t pt = utf8_decode(str, i);
    utf8_encode(pt + lowercase_offset(pt), out);
  }
  return out;
}

/* * * * * * * * * *
 * Composition
 * * * * * * * * * */

/*
 * Write the canonical decomposition of pt to out.
 */
static void unicode_decompose(char32_t pt, std::u32string& out) {
  // Special-case: Hangul decomposition
  if (pt >= 0xAC00 && pt < 0xD7A4) {
    out += 0x1100 + (pt - 0xAC00) / 588;
    out += 0x1161 + ((pt - 0xAC00) % 588) / 28;
    if ((pt - 0xAC00) % 28) out += 0x11A7 + (pt - 0xAC00) % 28;
    return;
  }

  // Otherwise, look up in the decomposition table
  int32_t decomp_start_idx = decomp_idx(pt);
  if (!decomp_start_idx) {
    out += pt;
    return;
  }

  size_t length = (decomp_start_idx >> 14) + 1;
  decomp_start_idx &= (1 << 14) - 1;

  for (size_t i = 0; i < length; i++) {
    out += xref[decomp_seq[decomp_start_idx + i]];
  }
}

/*
 * If there is a Primary Composite equivalent to <L, C>, return it. Otherwise
 * return 0.
 */
static uint32_t unicode_compose(uint32_t L, uint32_t C) {
  int comp_seq_idx;

  /* Algorithmic Hangul composition */
  if (L >= 0x1100 && L < 0x1113 && C >= 0x1161 && C < 0x1176)
    return ((L - 0x1100) * 21 + C - 0x1161) * 28 + 0xAC00;

  if (L >= 0xAC00 && L < 0xD7A4 && !((L - 0xAC00) % 28) && C >= 0x11A8 &&
      C < 0x11C3)
    return L + C - 0x11A7;

  /* Predefined composition mapping */
  comp_seq_idx = comp_idx(L);
  do {
    if (xref[comp_seq[comp_seq_idx * 2] & ~0x8000] == C)
      return xref[comp_seq[comp_seq_idx * 2 + 1]];
  } while (!(comp_seq[(comp_seq_idx++) * 2] & 0x8000));

  return 0;
}

std::u32string normalize32(const std::string& str, bool compose,
                           bool* replacement_flag) {
  if (str.empty()) return {};

  // Decode and decompose
  std::u32string codepoints;
  codepoints.reserve(str.size());
  for (size_t i = 0; i < str.length();) {
    uint32_t pt = utf8_decode(str, i, replacement_flag);
    unicode_decompose(pt, codepoints);
  }

  // Canonical Ordering Algorithm: sort all runs of characters with nonzero
  // combining class.
  size_t start = 0;
  while (start < codepoints.length()) {
    if (!ccc(codepoints[start])) {
      start++;
      continue;
    }

    size_t end = start + 1;
    while (end < codepoints.length() && ccc(codepoints[end])) {
      end++;
    }

    if (end - start > 1) {
      std::stable_sort(codepoints.begin() + start, codepoints.begin() + end,
                       [](char32_t a, char32_t b) { return ccc(a) < ccc(b); });
    }

    start = end + 1;
  }

  if (compose) {
    size_t i = 1;
    int last_class = -1, starter_pos = 0, target_pos = 1;
    char32_t starter = codepoints[0];

    while (i < codepoints.length()) {
      char32_t ch = codepoints[i];
      int ch_class = ccc(ch);

      uint32_t composite = unicode_compose(starter, ch);
      if (composite && last_class < ch_class) {
        codepoints[starter_pos] = composite;
        starter = composite;
      } else if (ch_class == 0) {
        starter_pos = target_pos;
        starter = ch;
        last_class = -1;
        codepoints[target_pos] = ch;
        target_pos++;
      } else {
        last_class = ch_class;
        codepoints[target_pos] = ch;
        target_pos++;
      }

      i++;
    }

    codepoints.resize(target_pos);
  }

  return codepoints;
}

std::string normalize8(const std::string& str, bool compose,
                       bool* replacement_flag) {
  std::u32string codepoints = normalize32(str, compose, replacement_flag);
  return to_utf8(codepoints);
}

std::string nfc(const std::string& str, bool* replacement_flag) {
  return normalize8(str, true, replacement_flag);
}

std::string nfd(const std::string& str, bool* replacement_flag) {
  return normalize8(str, false, replacement_flag);
}

}  // namespace miniutf
