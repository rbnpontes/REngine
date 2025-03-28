#pragma once
#define ALLOC_DEFAULT_LIMIT 128 * 1000000 // default size is 128mb
//#define HIGH_DEFINITION_PRECISION // enable high precision math types
#define MAX_ALLOWED_WINDOWS 4



#if MAX_ALLOWED_WINDOWS < 1
	#error "MAX_ALLOWED_WINDOWS must be greater than 0"
#endif
#if MAX_ALLOWED_WINDOWS > 254
	#error "MAX_ALLOWED_WINDOWS must be less than 254"
#endif