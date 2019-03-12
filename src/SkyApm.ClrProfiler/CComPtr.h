// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#ifndef CLR_PROFILER_COM_PTR_H_
#define CLR_PROFILER_COM_PTR_H_

#include <cassert>

// https://msdn.microsoft.com/en-us/magazine/dn904668.aspx

template <typename Interface>
class RemoveAddRefRelease : public Interface {
    ULONG __stdcall AddRef();
    ULONG __stdcall Release();
};

template <typename Interface>
class CComPtr {
public:
    CComPtr() noexcept = default;

    CComPtr(CComPtr const& other) noexcept : m_ptr(other.m_ptr) {
        InternalAddRef();
    }

    template <typename T>
    friend class CComPtr;

    template <typename T>
    CComPtr(CComPtr<T> const& other) noexcept : m_ptr(other.m_ptr) {
        InternalAddRef();
    }

    template <typename T>
    CComPtr(CComPtr<T>&& other) noexcept : m_ptr(other.m_ptr) {
        other.m_ptr = nullptr;
    }

    ~CComPtr() noexcept { InternalRelease(); }

    void Swap(CComPtr& other) noexcept {
        Interface* temp = m_ptr;
        m_ptr = other.m_ptr;
        other.m_ptr = temp;
    }

    void Reset() noexcept { InternalRelease(); }

    Interface* Get() const noexcept { return m_ptr; }

    Interface* Detach() noexcept {
        Interface* temp = m_ptr;
        m_ptr = nullptr;
        return temp;
    }

    void Copy(Interface* other) noexcept { InternalCopy(other); }

    void Attach(Interface* other) noexcept {
        InternalRelease();
        m_ptr = other;
    }

    Interface** GetAddressOf() noexcept {
        assert(m_ptr == nullptr);
        return &m_ptr;
    }

    void CopyTo(Interface** other) const noexcept {
        InternalAddRef();
        *other = m_ptr;
    }

    template <typename T>
    CComPtr<T> As(IID iid) const noexcept {
        CComPtr<T> temp;
        m_ptr->QueryInterface(iid, reinterpret_cast<void**>(temp.GetAddressOf()));
        return temp;
    }

    bool IsNull() const noexcept { return nullptr == m_ptr; }

    CComPtr& operator=(CComPtr const& other) noexcept {
        InternalCopy(other.m_ptr);
        return *this;
    }

    template <typename T>
    CComPtr& operator=(CComPtr<T> const& other) noexcept {
        InternalCopy(other.m_ptr);
        return *this;
    }

    template <typename T>
    CComPtr& operator=(CComPtr<T>&& other) noexcept {
        InternalMove(other);
        return *this;
    }

    RemoveAddRefRelease<Interface>* operator->() const noexcept {
        return static_cast<RemoveAddRefRelease<Interface>*>(m_ptr);
    }

    explicit operator bool() const noexcept { return nullptr != m_ptr; }

private:
    Interface* m_ptr = nullptr;

    void InternalAddRef() const noexcept {
        if (m_ptr) {
            m_ptr->AddRef();
        }
    }

    void InternalRelease() noexcept {
        Interface* temp = m_ptr;
        if (temp) {
            m_ptr = nullptr;
            temp->Release();
        }
    }

    void InternalCopy(Interface* other) noexcept {
        if (m_ptr != other) {
            InternalRelease();
            m_ptr = other;
            InternalAddRef();
        }
    }

    template <typename T>
    void InternalMove(CComPtr<T>& other) noexcept {
        if (m_ptr != other.m_ptr) {
            InternalRelease();
            m_ptr = other.m_ptr;
            other.m_ptr = nullptr;
        }
    }
};

template <typename Interface>
void swap(CComPtr<Interface>& left, CComPtr<Interface>& right) noexcept {
    left.Swap(right);
}

#endif  // CLR_PROFILER_COM_PTR_H_
