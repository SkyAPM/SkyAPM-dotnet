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

#pragma once

#include <string>

namespace miniutf {

/*
 * Character-at-a-time encoding. Convert pt to UTF-8/16 and append to out.
 *
 * If pt is invalid (greater than U+10FFFF), U+FFFD will be encoded instead.
 */
void utf8_encode(char32_t pt, std::string & out);
void utf16_encode(char32_t pt, std::u16string & out);

/*
 * Character-at-a-time decoding. Decodes and returns the codepoint starting at str[pos],
 * and then advance pos by the appropriate amount.
 *
 * If an invalid codepoint is found, return U+FFFD, add 1 to pos, and (if replacement_flag is
 * non-null) set *replacement_flag to true.
 */
char32_t utf8_decode(const std::string & str,
                     std::string::size_type & pos,
                     bool * replacement_flag = nullptr);
char32_t utf16_decode(const std::u16string & str,
                      std::u16string::size_type & pos,
                      bool * replacement_flag = nullptr);

/*
 * Return true if str is valid UTF-8, -16, or -32.
 *
 * - UTF-8 is valid if it contains no misplaced or missing continuation bytes, no overlong
 *   encodings, and no codepoints above U+10FFFF.
 *
 * - UTF-16 is valid if it contains no unpaired surrogates. (There's no way to attempt
 *   to represent codepoints above U+10FFFF in UTF-16.)
 *
 * - UTF-32 is valid if it contains no codepoints above U+10FFFF.
 */
bool utf8_check(const std::string & str);
bool utf16_check(const std::string & str);
bool utf32_check(const std::string & str);

/*
 * Convert back and forth between UTF-8 and UTF-16 or UTF-32.
 *
 * These functions replace invalid sections of input with U+FFFD. If this is not desired,
 * use utf8_check (above) first to check that the input is valid.
 */
std::u32string to_utf32(const std::string & str);
std::u16string to_utf16(const std::string & str);
std::string to_utf8(const std::u16string & str);
std::string to_utf8(const std::u32string & str);

/*
 * Convert str to lowercase, per the built-in Unicode lowercasing map (codepoint-by-codepoint).
 */
std::string lowercase(const std::string & str);

/*
 * Decompose str. Then, if compose is set, recompose it.
 *
 * If replacement characters are used during decoding (i.e. str contains invalid UTF-8), and
 * replacement_flag is specified, it will be set to true.
 */
std::string normalize8(const std::string & str,
                       bool compose,
                       bool * replacement_flag = nullptr);
std::u32string normalize32(const std::string & str,
                           bool compose,
                           bool * replacement_flag = nullptr);

/*
 * Convert str to Normalization Form C. Equivalent to normalize8(str, true, replacement_flag).
 *
 * If replacement characters are used during decoding (i.e. str contains invalid UTF-8), and
 * replacement_flag is specified, *replacement_flag will be set to true.
 */
std::string nfc(const std::string & str, bool * replacement_flag = nullptr);

/*
 * Convert str to Normalization Form D. Equivalent to normalize8(in, false, replacement_flag).
 *
 * If replacement characters are used during decoding (i.e. str contains invalid UTF-8), and
 * replacement_flag is specified, *replacement_flag will be set to true.
 */
std::string nfd(const std::string & str, bool * replacement_flag = nullptr);

} // namespace miniutf
