cmake_minimum_required (VERSION 3.5)

SET (CMAKE_TOOLCHAIN_FILE         "~/vcpkg/scripts/buildsystems/vcpkg.cmake")

SET (CMAKE_C_COMPILER             "/usr/bin/clang-3.9")
SET (CMAKE_CXX_COMPILER           "/usr/bin/clang++-3.9")

project("SkyApm.ClrProfiler")

add_compile_options(-std=c++11 -fPIC -fms-extensions)
add_compile_options(-DBIT64 -DPAL_STDCPP_COMPAT -DPLATFORM_UNIX -DUNICODE)
add_compile_options(-Wno-invalid-noreturn -Wno-macro-redefined)

find_package(spdlog CONFIG REQUIRED)

include_directories("SkyApm.ClrProfiler"
    ~/coreclr/src/pal/inc/rt
    ~/coreclr/src/pal/prebuilt/inc
    ~/coreclr/src/pal/inc
    ~/coreclr/src/inc
    ~/coreclr/src/bin/Product/Linux.x64.Debug/inc
)

add_library("SkyApm.ClrProfiler" SHARED
    util.cpp
    config_loader.cpp
    il_rewriter.cpp
    il_rewriter_helper.cpp 
    clr_helpers.cpp
    profiler.cpp 
    class_factory.cpp
    dllmain.cpp
)

set_target_properties("SkyApm.ClrProfiler" PROPERTIES PREFIX "")

target_link_libraries("SkyApm.ClrProfiler" PRIVATE spdlog::spdlog)

