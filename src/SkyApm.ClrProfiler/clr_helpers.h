/*
 * Licensed to the SkyAPM under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The SkyAPM licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

#ifndef CLR_PROFILER_CLRHELPER_H_
#define CLR_PROFILER_CLRHELPER_H_

#include <vector>
#include "util.h"
#include "ccom_ptr.h"
#include <corprof.h>
#include "logging.h"
#include <cor.h>
#include <corhlpr.h>
#include "config_loader.h"

#define RETURN_FAIL_IF_FALSE(EXPR)      \
  do {                                  \
    if ((EXPR) == false) return E_FAIL; \
  } while (0)

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

namespace clrprofiler {

    const size_t NameMaxSize = 1024;

    const WSTRING ProfilerAssemblyName = W("SkyApm.ClrProfiler.Trace");
    const WSTRING TraceAgentTypeName = W("SkyApm.ClrProfiler.Trace.TraceAgent");
    const WSTRING GetInstanceMethodName = W("GetInstance");
    const WSTRING BeforeMethodName = W("BeforeMethod");
    const WSTRING AfterMethodName = W("AfterMethod");
    const WSTRING MethodTraceTypeName = W("SkyApm.ClrProfiler.Trace.MethodTrace");

    const WSTRING AssemblyTypeName = W("System.Reflection.Assembly");
    const WSTRING AssemblyLoadMethodName = W("LoadFrom");

    const WSTRING SystemTypeName = W("System.Type");
    const WSTRING GetTypeFromHandleMethodName = W("GetTypeFromHandle");
    const WSTRING RuntimeTypeHandleTypeName = W("System.RuntimeTypeHandle");

    const WSTRING SystemBoolean = W("System.Boolean");
    const WSTRING SystemChar = W("System.Char");
    const WSTRING SystemByte = W("System.Byte");
    const WSTRING SystemSByte = W("System.SByte");
    const WSTRING SystemUInt16 = W("System.UInt16");
    const WSTRING SystemInt16 = W("System.Int16");
    const WSTRING SystemInt32 = W("System.Int32");
    const WSTRING SystemUInt32 = W("System.UInt32");
    const WSTRING SystemInt64 = W("System.Int64");
    const WSTRING SystemUInt64 = W("System.UInt64");
    const WSTRING SystemSingle = W("System.Single");
    const WSTRING SystemDouble = W("System.Double");
    const WSTRING SystemIntPtr = W("System.IntPtr");
    const WSTRING SystemUIntPtr = W("System.UIntPtr");
    const WSTRING SystemString = W("System.String");
    const WSTRING SystemObject = W("System.Object");
    const WSTRING SystemException = W("System.Exception");

    struct AssemblyProperty {
        const void  *ppbPublicKey;
        ULONG       pcbPublicKey;
        ULONG       pulHashAlgId;
        ASSEMBLYMETADATA pMetaData{};
        WSTRING     szName;
        DWORD assemblyFlags = 0;

        AssemblyProperty() : ppbPublicKey(nullptr), pcbPublicKey(0), pulHashAlgId(0), szName(W(""))
        {
        }
    };

    struct AssemblyInfo {
        const AssemblyID id;
        const WSTRING name;

        AssemblyInfo() : id(0), name(W("")) {}
        AssemblyInfo(AssemblyID id, WSTRING name) : id(id), name(name) {}
		
        bool IsValid() const { return id != 0; }
    };

    class ModuleMetaInfo {
    private:
    public:
        const mdToken entryPointToken;
        const WSTRING assemblyName;
		const TraceAssembly trace_assembly;

		//iLRewriteMap ,because generic method has multi functionid
		std::unordered_map<mdMethodDef, bool> iLRewriteMap{};

        ModuleMetaInfo(mdToken entry_point_token, WSTRING assembly_name, TraceAssembly trace_assembly)
            : entryPointToken(entry_point_token),
              assemblyName(assembly_name),
			  trace_assembly(trace_assembly){}

        mdToken getTypeFromHandleToken = 0;
    };

    struct ModuleInfo {
        const ModuleID id;
        const WSTRING path;
        const AssemblyInfo assembly;
        const DWORD flags;
        const LPCBYTE baseLoadAddress;

        ModuleInfo() : id(0), path(W("")), assembly({}), flags(0), baseLoadAddress(nullptr){}
        ModuleInfo(ModuleID id, WSTRING path, AssemblyInfo assembly, DWORD flags, LPCBYTE baseLoadAddress)
            : id(id), path(path), assembly(assembly), flags(flags), baseLoadAddress(baseLoadAddress) {}

        bool IsValid() const { return id != 0; }

        bool CanILRewrite() const {
            return !((flags & COR_PRF_MODULE_WINDOWS_RUNTIME) != 0);
        }

        mdToken GetEntryPointToken() const {
            if (baseLoadAddress == nullptr) {
                return  mdTokenNil;
            }

            const auto pntHeaders = baseLoadAddress + VAL32(((IMAGE_DOS_HEADER*)baseLoadAddress)->e_lfanew);
            const auto ntHeaders = (IMAGE_NT_HEADERS64*)pntHeaders;
            IMAGE_DATA_DIRECTORY directoryEntry;
            if (ntHeaders->OptionalHeader.Magic == VAL16(IMAGE_NT_OPTIONAL_HDR32_MAGIC)) 
            {
                directoryEntry = ((IMAGE_NT_HEADERS32*)pntHeaders)->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_COMHEADER];
            }
            else 
            {
                directoryEntry = ntHeaders->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_COMHEADER];
            }
            const auto corHeader = (IMAGE_COR20_HEADER*)GetRvaData(VAL32(directoryEntry.VirtualAddress), pntHeaders);
            return corHeader->EntryPointToken;
        }

    private:
        static ULONG AlignUp(ULONG value, UINT alignment)
        {
            return (value + alignment - 1)&~(alignment - 1);
        }

        LPCBYTE GetRvaData(DWORD rva, LPCBYTE pntHeaders) const
        {
            if (COR_PRF_MODULE_FLAT_LAYOUT & flags)
            {
                const auto ntHeaders = (IMAGE_NT_HEADERS*)pntHeaders;
                IMAGE_SECTION_HEADER *sectionRet = NULL;
                const auto pSection = pntHeaders + FIELD_OFFSET(IMAGE_NT_HEADERS, OptionalHeader) + VAL16(ntHeaders->FileHeader.SizeOfOptionalHeader);
                auto section = (IMAGE_SECTION_HEADER*)pSection;
                const auto sectionEnd = (IMAGE_SECTION_HEADER*)(pSection + VAL16(ntHeaders->FileHeader.NumberOfSections));
                while (section < sectionEnd)
                {
                    if (rva < VAL32(section->VirtualAddress)
                        + AlignUp((UINT)VAL32(section->Misc.VirtualSize), (UINT)VAL32(ntHeaders->OptionalHeader.SectionAlignment)))
                    {
                        if (rva < VAL32(section->VirtualAddress))
                            sectionRet = NULL;
                        else
                        {
                            sectionRet = section;
                        }
                    }
                    section++;
                }
                if (sectionRet == NULL)
                {
                    return baseLoadAddress + rva;
                }
                return baseLoadAddress + rva - VAL32(sectionRet->VirtualAddress) + VAL32(sectionRet->PointerToRawData);

            }
            return baseLoadAddress + rva;
        }
    };

    struct TypeInfo {
        const mdToken id;
        const WSTRING name;

        TypeInfo() : id(0), name(W("")) {}
        TypeInfo(mdToken id, WSTRING name) : id(id), name(name) {}

        bool IsValid() const { return id != 0; }
    };

    enum MethodArgumentTypeFlag
    {
        TypeFlagByRef = 0x01,
        TypeFlagVoid = 0x02,
        TypeFlagBoxedType = 0x04
    };

    struct MethodArgument {
        ULONG offset;
        ULONG length;
        PCCOR_SIGNATURE pbBase;
        mdToken GetTypeTok(CComPtr<IMetaDataEmit2>& pEmit, mdAssemblyRef corLibRef) const;
        WSTRING GetTypeTokName(CComPtr<IMetaDataImport2>& pImport) const;
        int GetTypeFlags(unsigned& elementType) const;
    };

    struct MethodSignature {
    private:
        PCCOR_SIGNATURE pbBase;
        unsigned len;
        ULONG numberOfTypeArguments = 0;
        ULONG numberOfArguments = 0;     
        MethodArgument ret{};
        std::vector<MethodArgument> params;
    public:
        MethodSignature(): pbBase(nullptr), len(0){}
        MethodSignature(PCCOR_SIGNATURE pb, unsigned cbBuffer) {
            pbBase = pb;
            len = cbBuffer;
        };
        ULONG NumberOfTypeArguments() const { return numberOfTypeArguments; }
        ULONG NumberOfArguments() const { return numberOfArguments; }
        WSTRING ToWString() const { return HexStr(pbBase, len); }
        MethodArgument GetRet() const { return  ret; }
        std::vector<MethodArgument> GetMethodArguments() const { return params; }
        HRESULT TryParse();
        bool operator ==(const MethodSignature& other) const {
            return memcmp(pbBase, other.pbBase, len);
        }
        CorCallingConvention CallingConvention() const {
            return CorCallingConvention(len == 0 ? 0 : pbBase[0]);
        }
        bool IsEmpty() const  {
            return len == 0;
        }
    };

    struct FunctionInfo {
        const mdToken id;
        const WSTRING name;
        const TypeInfo type;
        MethodSignature signature;

        FunctionInfo() : id(0), name(W("")), type({}), signature({}) {}
        FunctionInfo(mdToken id, WSTRING name, TypeInfo type, MethodSignature signature) : id(id), name(name), type(type), signature(signature) {}

        bool IsValid() const { return id != 0; }
    };

    ModuleInfo GetModuleInfo(ICorProfilerInfo4* info,
		const ModuleID& module_id);

	TypeInfo GetTypeInfo(const CComPtr<IMetaDataImport2>& metadata_import,
		const mdToken& token);

	FunctionInfo GetFunctionInfo(const CComPtr<IMetaDataImport2>& metadata_import,
		const mdToken& token);

    mdAssemblyRef GetCorLibAssemblyRef(CComPtr<IUnknown>& metadata_interfaces, 
		AssemblyProperty assembly_property);

    mdAssemblyRef GetProfilerAssemblyRef(CComPtr<IUnknown>& metadata_interfaces,
        ASSEMBLYMETADATA assembly_metadata, 
        std::vector<BYTE> public_key);

}

#endif  // CLR_PROFILER_CLRHELPER_H_
