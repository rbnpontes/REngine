#pragma once
#include <rengine/types.h>

namespace rengine {
	namespace core {
		struct profiler_entry_info {
			c_str name{ null };
			c_str function{ null };
			c_str file{ null };
			u32 line{ 0 };
			u32 padding{ 0 };
		};

		void profiler_entry_push(profiler_entry_info* entry);
		void profiler_entry_pop();
		void profiler_log(c_str str);
		bool profiler_connected();
		
		struct profiler_scope {
			~profiler_scope() {
				profiler_entry_pop();
			}
		};

#define profiler__concat_indirect(x, y) x##y
#define profiler__concat(x, y) profiler__concat_indirect(x, y)
#define profiler__line __LINE__
#define profiler__entry_def(line) profiler__concat(___profiler_entry, line)
#define profiler__entry(name, line) \
	rengine::core::profiler_entry_info profiler__entry_def(line) = {\
		name, __func__, __FILE__, line, 0 \
	};
#define profile_begin() \
	profiler__entry(null, profiler__line) \
	rengine::core::profiler_entry_push(&profiler__entry_def(profiler__line))
#define profile_begin_name(name) \
	profiler__entry(name, profiler__line) \
	rengine::core::profiler_entry_push(&profiler__entry_def(profiler__line))
#define profile_end() rengine::core::profiler_entry_pop()
#define profile_scoped_end() \
	rengine::core::profiler_scope profiler__concat(___profiler_scoped_end, profiler__line) = {}

#define profile() \
	profile_begin(); \
	rengine::core::profiler_scope profiler__concat(___profiler_scope, profiler__line) = {}

#define profile_name(name) \
	profile_begin_name(name); \
	rengine::core::profiler_scope profiler__concat(___profiler_scope, profiler__line) = {}

	}
}