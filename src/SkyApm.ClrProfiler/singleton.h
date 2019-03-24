#ifndef CLR_PROFILER_SINGLETON_H_
#define CLR_PROFILER_SINGLETON_H_

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

#endif

