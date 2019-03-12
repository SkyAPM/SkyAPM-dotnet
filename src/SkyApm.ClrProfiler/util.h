// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#ifndef CLR_PROFILER_UTIL_H_
#define CLR_PROFILER_UTIL_H_

#include <string>
#include <vector>
#include "string.h" // NOLINT

#ifdef _WIN32

#include <io.h>
#include <direct.h>  
#include <process.h>

#else

#include <unistd.h> 
#include <sys/types.h>  
#include <sys/stat.h>
#include <fstream>

#endif

namespace trace {

    const WSTRING CORECLR_PROFILER_HOME = "CORECLR_PROFILER_HOME"_W;
    const WSTRING COR_PROFILER_HOME = "COR_PROFILER_HOME"_W;

    void SetClrProfilerFlag(bool flag);
    WSTRING GetClrProfilerHome();

#ifdef _WIN32
    const auto PathSeparator = "\\"_W;
#else
    const auto PathSeparator = "/"_W;
#endif

    inline int GetPID() {
#ifdef _WIN32
        return _getpid();
#else
        return getpid();
#endif
    } 

    static bool CheckDir(const char* dir)
    {
#ifdef _WIN32  
        if (_access(dir, 0) == -1)
#else 
        if (access(dir, 0) == -1)
#endif
        {
#ifdef _WIN32  
            int flag = _mkdir(dir);
#else 
            int flag = mkdir(dir, 0777);
#endif  
            return (flag == 0);
        }
        return true;
    };

    class UnCopyable
    {
    protected:
        UnCopyable() {};
        ~UnCopyable() {};

    private:
        UnCopyable(const UnCopyable&) = delete;
        UnCopyable(const UnCopyable&&) = delete;
        UnCopyable& operator = (const UnCopyable&) = delete;
        UnCopyable& operator = (const UnCopyable&&) = delete;
    };

    template <typename T>
    class Singleton : public UnCopyable
    {
    public:
        static T* Instance()
        {
            static T instance_obj;
            return &instance_obj;
        }
    };

    template <typename Out>
    void Split(const WSTRING &s, wchar_t delim, Out result);

    // Split splits a string by the given delimiter.
    std::vector<WSTRING> Split(const WSTRING &s, wchar_t delim);

    // Trim removes space from the beginning and end of a string.
    WSTRING Trim(const WSTRING &str);

    // GetEnvironmentValue returns the environment variable value for the given
    // name. Space is trimmed.
    WSTRING GetEnvironmentValue(const WSTRING &name);

    // GetEnvironmentValues returns environment variable values for the given name
    // split by the delimiter. Space is trimmed and empty values are ignored.
    std::vector<WSTRING> GetEnvironmentValues(const WSTRING &name,
        const wchar_t delim);

    // GetEnvironmentValues calls GetEnvironmentValues with a semicolon delimiter.
    std::vector<WSTRING> GetEnvironmentValues(const WSTRING &name);

    //HexStr
    WSTRING HexStr(const unsigned char *data, int len);

    //HexToBytes
    std::vector<BYTE> HexToBytes(const std::string& hex);
}  // namespace trace
#endif  // CLR_PROFILER_UTIL_H_
