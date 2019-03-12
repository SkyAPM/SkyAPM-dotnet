// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#ifndef CLR_PROFILER_MACROS_H_
#define CLR_PROFILER_MACROS_H_

#include <corhlpr.h>
#include <fstream>

#define RETURN_IF_FAILED(EXPR) \
  do {                         \
    hr = (EXPR);               \
    if (FAILED(hr)) {          \
      return (hr);             \
    }                          \
  } while (0)
#define RETURN_OK_IF_FAILED(EXPR) \
  do {                            \
    hr = (EXPR);                  \
    if (FAILED(hr)) {             \
      return S_OK;                \
    }                             \
  } while (0)

#endif  // CLR_PROFILER_MACROS_H_
