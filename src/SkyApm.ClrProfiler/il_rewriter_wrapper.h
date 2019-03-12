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

#ifndef CLR_PROFILER_IL_REWRITER_WRAPPER_H_
#define CLR_PROFILER_IL_REWRITER_WRAPPER_H_

#include "il_rewriter.h"

class ILRewriterWrapper {
 private:
  ILRewriter* const m_ILRewriter;
  ILInstr* m_ILInstr;

 public:
  ILRewriterWrapper(ILRewriter* const il_rewriter)
      : m_ILRewriter(il_rewriter), m_ILInstr(nullptr) {}

  ILRewriter* GetILRewriter() const;
  void SetILPosition(ILInstr* pILInstr);
  void Pop() const;
  void LoadNull() const;
  void LoadStr(mdToken token) const;
  void LoadInt64(INT64 value) const;
  void LoadInt32(INT32 value) const;
  void LoadArgument(UINT16 index) const;
  void LoadIND(unsigned elementType) const;
  void LoadToken(mdToken token) const;
  void StLocal(unsigned index) const;
  void LoadLocal(unsigned index) const;
  void Cast(mdTypeRef type_ref) const;
  void Box(mdTypeRef type_ref) const;
  void UnboxAny(mdTypeRef type_ref) const;
  void CreateArray(mdTypeRef type_ref, INT32 size) const;
  void CallMember(const mdMemberRef& member_ref, bool is_virtual) const;
  void Duplicate() const;
  void BeginLoadValueIntoArray(INT32 arrayIndex) const;
  void EndLoadValueIntoArray() const;
  void Return() const;
  ILInstr* Rethrow() const;
  ILInstr* EndFinally() const;
  ILInstr* CallMember0(const mdMemberRef& member_ref, bool is_virtual) const;
};

#endif  // CLR_PROFILER_IL_REWRITER_WRAPPER_H_
