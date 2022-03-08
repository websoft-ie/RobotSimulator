#include "rcp_api/rcp_api.h"

#include <mutex>

std::mutex **rcp_mutex = new std::mutex*[RCP_MUTEX_COUNT];

extern "C"
{
	__declspec(dllexport) int Add(int a, int b)
	{
		return a + b;
	}
}