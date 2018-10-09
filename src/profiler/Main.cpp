//
// Created by liuhaoyang on 2018/6/1.
//
#include "core/CorProfilerFactory.h"

// {bc160b08-ba38-4401-8c26-85d11ff07fa7}
const CLSID CLSID_DemoProfiler = {
        0xb62bd191, 0x587d, 0x4924, { 0x99, 0x8f,  0x92, 0xdd, 0x56, 0xf2, 0x58, 0x4c }};

extern "C" {
    HRESULT STDMETHODCALLTYPE DllGetClassObject(REFCLSID rclsid, REFIID riid, void **ppv) {
        if (rclsid != CLSID_DemoProfiler)
            return E_FAIL;

        *ppv = new CorProfilerFactory();

        return S_OK;
    }
}
