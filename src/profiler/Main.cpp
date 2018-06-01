//
// Created by liuhaoyang on 2018/6/1.
//
#include "core/CorProfilerFactory.h"

// {bc160b08-ba38-4401-8c26-85d11ff07fa7}
const CLSID CLSID_DemoProfiler = {
        0xbc160b08, 0xba38, 0x4401, { 0x8c, 0x26, 0x85, 0xd1, 0x1f, 0xf0, 0x7f, 0xa7 }};

extern "C" {
    HRESULT STDMETHODCALLTYPE DllGetClassObject(REFCLSID rclsid, REFIID riid, void **ppv) {
        if (ppv == NULL || rclsid != CLSID_DemoProfiler)
            return E_FAIL;

        *ppv = new CorProfilerFactory();

        return S_OK;
    }
}
