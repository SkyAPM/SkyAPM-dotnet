//
// Created by liuhaoyang on 2018/6/1.
//

#include "CorProfiler.h"

CorProfiler::CorProfiler() {
}

CorProfiler::~CorProfiler() {
}

HRESULT STDMETHODCALLTYPE CorProfiler::AssemblyLoadStarted(AssemblyID assemblyId)
{
    printf("AssemblyLoad..\n");
    return S_OK;
}