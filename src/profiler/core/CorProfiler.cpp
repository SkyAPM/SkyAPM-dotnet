//
// Created by liuhaoyang on 2018/6/1.
//

#include "CorProfiler.h"
#include <iostream>
#include <codecvt>

using namespace std;

CorProfiler::CorProfiler() {
}

CorProfiler::~CorProfiler() {
}

HRESULT STDMETHODCALLTYPE CorProfiler::AssemblyLoadStarted(AssemblyID assemblyId) {

    cout << "AssemblyLoad.." << endl;

    ICorProfilerInfo3 *info = this->GetCorProfilerInfo3();

    WCHAR assemblyName[256];
    HRESULT hr = info->GetAssemblyInfo(assemblyId, 256, nullptr, assemblyName, nullptr, nullptr);
    if(hr == S_OK){
        wstring_convert<std::codecvt_utf8<char16_t>, char16_t> convert;
        cout<< convert.to_bytes(assemblyName) <<endl;
    }

    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfiler::JITCompilationStarted(FunctionID functionId, BOOL fIsSafeToBlock)
{

    return S_OK;
}