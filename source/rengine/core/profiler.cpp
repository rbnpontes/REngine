#include "profiler.h"
#include "./profiler_private.h"

namespace rengine {
	namespace core {
		void profiler_entry_push(profiler_entry_info* entry)
		{
			profiler__entry_push(entry);
		}

		void profiler_entry_pop()
		{
			profiler__entry_pop();
		}

		void profiler_log(c_str str)
		{
			profiler__log(str);
		}

		bool profiler_connected()
		{
			return g_profiler_state.connected;
		}
	}
}
