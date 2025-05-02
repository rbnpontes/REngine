#pragma once
#include <rengine/math/math-operations.h>

#if ENGINE_SSE
	#include <emmintrin.h>
	#ifdef HIGH_DEFINITION_PRECISION
		#define sse_load_number(ptr) _mm_loadu_pd(ptr)
		#define sse_store_number(ptr, val) _mm_storeu_pd(ptr, val)
		#define sse_cmpeq_number(first, second) _mm_cmpeq_pd(first, second)
		#define sse_and_number(first, second) _mm_and_pd(first, second)
		#define sse_movehl_number(first, second) _mm_movehl_pd(first, second)
		#define sse_shuffle_number(first, second, imm) _mm_shuffle_pd(first, second, imm)
		#define sse_cast_int_number(first) _mm_castpd_si128(first)
		#define sse_set_number(w, z, y, x) _mm_set_pd(w, z, y, x)
		#define sse_set_int(w, z, y, x) _mm_set_epi32(w, z, y, x)
		#define sse_set_single_number(x) _mm_set1_pd(x)
		#define sse_set_single_int(x) _mm_set1_epi32(x)
		#define sse_add_number(first, second) _mm_add_pd(first, second)
		#define sse_sub_number(first, second) _mm_sub_pd(first, second)
		#define sse_mul_number(first, second) _mm_mul_pd(first, second)
		#define sse_div_number(first, second) _mm_div_pd(first, second)
		#define sse_unpacklo_number(first, second) _mm_unpacklo_pd(first, second)
		#define sse_unpackhi_number(first, second) _mm_unpackhi_pd(first, second)
		#define sse_movelh_number(first, second) _mm_movelh_pd(first, second)
		#define sse_movehl_number(first, second) _mm_movehl_pd(first, second)
		#define sse_cvtss_number(x) _mm_cvtsd_f64(x)
	#else
		#define sse_load_number(ptr) _mm_loadu_ps(ptr)
		#define sse_store_number(ptr, val) _mm_storeu_ps(ptr, val)
		#define sse_cmpeq_number(first, second) _mm_cmpeq_ps(first, second)
		#define sse_and_number(first, second) _mm_and_ps(first, second)
		#define sse_movehl_number(first, second) _mm_movehl_ps(first, second)	
		#define sse_shuffle_number(first, second, imm) _mm_shuffle_ps(first, second, imm)
		#define sse_cast_int_number(first) _mm_castps_si128(first)
		#define sse_set_number(w, z, y, x) _mm_set_ps(w, z, y, x)
		#define sse_set_int(w, z, y, x) _mm_set_epi32(w, z, y, x)
		#define sse_set_single_number(x) _mm_set1_ps(x)
		#define sse_set_single_int(x) _mm_set1_epi32(x)
		#define sse_add_number(first, second) _mm_add_ps(first, second)
		#define sse_sub_number(first, second) _mm_sub_ps(first, second)
		#define sse_mul_number(first, second) _mm_mul_ps(first, second)
		#define sse_div_number(first, second) _mm_div_ps(first, second)
		#define sse_unpacklo_number(first, second) _mm_unpacklo_ps(first, second)
		#define sse_unpackhi_number(first, second) _mm_unpackhi_ps(first, second)
		#define sse_movelh_number(first, second) _mm_movelh_ps(first, second)
		#define sse_movehl_number(first, second) _mm_movehl_ps(first, second)
		#define sse_cvtss_number(x) _mm_cvtss_f32(x)
	#endif
	#define sse_cvtsi128_int(x) _mm_cvtsi128_si32(x)

	#define sse_m128_t __m128
#else
	#define sse_load_number(ptr) rengine::math::fake_sse::load(ptr)
	#define sse_store_number(ptr, val) rengine::math::fake_sse::store(ptr, val)
	#define sse_cmpeq_number(first, second) rengine::math::fake_sse::cmpeq(first, second)
    #define sse_and_number(first, second) rengine::math::fake_sse::_and(first, second)
	#define sse_movehl_number(first, second) rengine::math::fake_sse::movehl(first, second)
	#define sse_shuffle_number(first, second, imm) rengine::math::fake_sse::shuffle(first, second, imm)
    #define sse_cast_int_number(first) rengine::math::fake_sse::cast_int(first)
	#define sse_cvtsi128_int(x) x.i[0]
	#define sse_set_number(w, z, y, x) rengine::math::fake_sse::set(w, z, y, x)
	#define sse_set_int(w, z, y, x) rengine::math::fake_sse::set(w, z, y, x)
	#define sse_set_single_number(x) rengine::math::fake_sse::set(x, x, x, x)
	#define sse_set_int_single(x) rengine::math::fake_sse::set(x, x, x, x)
	#define sse_add_number(first, second) rengine::math::fake_sse::add(first, second)
	#define sse_sub_number(first, second) rengine::math::fake_sse::sub(first, second)
	#define sse_mul_number(first, second) rengine::math::fake_sse::mul(first, second)
	#define sse_div_number(first, second) rengine::math::fake_sse::div(first, second)
	#define sse_unpacklo_number(first, second) rengine::math::fake_sse::unpacklo(first, second)
	#define sse_unpackhi_number(first, second) rengine::math::fake_sse::unpackhi(first, second)
	#define sse_movelh_number(first, second) rengine::math::fake_sse::movelh(first, second)
	#define sse_movehl_number(first, second) rengine::math::fake_sse::movehl(first, second)
	#define sse_cvtss_number(x) x.n[0]

	#define sse_m128_t rengine::math::fake_sse::m128_t
#endif
#define sse_shuffle(fp3, fp2, fp1, fp0) ( (fp3 << 6) | (fp2 << 4) | (fp1 << 2) | fp0 )
#define sse_transpose_number(row0, row1, row2, row3) { \
	sse_m128_t _tmp3, _tmp2, _tmp1, _tmp0; \
		\
		_tmp0 = sse_shuffle_number(row0, row1, 0x44); \
		_tmp2 = sse_shuffle_number(row0, row1, 0xEE); \
		_tmp1 = sse_shuffle_number(row2, row3, 0x44); \
		_tmp3 = sse_shuffle_number(row2, row3, 0xEE); \
		\
		row0 = sse_shuffle_number(_tmp0, _tmp1, 0x88); \
		row1 = sse_shuffle_number(_tmp0, _tmp1, 0xDD); \
		row2 = sse_shuffle_number(_tmp2, _tmp3, 0x88); \
		row3 = sse_shuffle_number(_tmp2, _tmp3, 0xDD); \
	}

namespace rengine {
	namespace math {
		namespace fake_sse {
			typedef union m128_t {
				number_t n[4];
				uint_t u[4];
				int_t i[4];
			};

			typedef struct m128i_t {
				int_t i[4];
			};

			// simulate SSE operations for non-SSE platforms
			constexpr m128_t load(const number_t* ptr) {
				m128_t result;
				for (size_t i = 0; i < 4; ++i)
					result.n[i] = ptr[i];
				return result;
			}

			constexpr void store(number_t* ptr, const m128_t& val) {
				for (size_t i = 0; i < 4; ++i)
					ptr[i] = val.n[i];
			}

			constexpr m128_t cmpeq(const m128_t& first, const m128_t& second) {
				m128_t result;
				for (size_t i = 0; i < 4; ++i)
					result.n[i] = equals(first.n[i], second.n[i]) ? 0xFFFFFFFF : 0x0;
				return result;
			}

			constexpr m128_t _and(const m128_t& first, const m128_t& second) {
				m128_t result;
				for (size_t i = 0; i < 4; ++i)
					result.u[i] = first.u[i] & second.u[i];
				return result;
			}

			constexpr m128_t shuffle(const m128_t& first, const m128_t& second, const u8& imm) {
				m128_t result;
				u8 a0 = (imm >> 0) & 0x3;
				u8 a1 = (imm >> 2) & 0x3;
				u8 b0 = (imm >> 4) & 0x3;
				u8 b1 = (imm >> 6) & 0x3;

				result.n[0] = first.n[a0];
				result.n[1] = first.n[a1];
				result.n[2] = second.n[b0];
				result.n[3] = second.n[b1];
				return result;
			}
		
			constexpr m128i_t cast_int(const m128_t& x) {
				m128i_t ret;
				for(u8 i = 0; i < 4; ++i)
					ret.i[i] = x.i[i];
				return ret;
			}

			constexpr m128_t set(const number_t& w, const number_t& z, const number_t& y, const number_t& x) {
				m128_t result;
				result.n[0] = x;
				result.n[1] = y;
				result.n[2] = z;
				result.n[3] = w;
				return result;
			}

			constexpr m128_t set(const int_t& w, const int_t& z, const int_t& y, const int_t& x) {
				m128_t result;
				result.i[0] = x;
				result.i[1] = y;
				result.i[2] = z;
				result.i[3] = w;
				return result;
			}

			constexpr m128_t add(const m128_t& first, const m128_t& second) {
				m128_t result;
				for (size_t i = 0; i < 4; ++i)
					result.n[i] = first.n[i] + second.n[i];
				return result;
			}

			constexpr m128_t sub(const m128_t& first, const m128_t& second) {
				m128_t result;
				for (size_t i = 0; i < 4; ++i)
					result.n[i] = first.n[i] - second.n[i];
				return result;
			}

			constexpr m128_t mul(const m128_t& first, const m128_t& second) {
				m128_t result;
				for (size_t i = 0; i < 4; ++i)
					result.n[i] = first.n[i] * second.n[i];
				return result;
			}

			constexpr m128_t div(const m128_t& first, const m128_t& second) {
				m128_t result;
				for (size_t i = 0; i < 4; ++i) {
					if (result.u[i] == 0)
						result.n[i] = 0;
					else
						result.n[i] = first.n[i] / second.n[i];
				}
				return result;
			}

			constexpr m128_t unpacklo(const m128_t& first, const m128_t& second) {
				m128_t result;
				result.n[0] = first.n[0];
				result.n[1] = second.n[0];
				result.n[2] = first.n[1];
				result.n[3] = second.n[1];
				return result;
			}

			constexpr m128_t unpackhi(const m128_t& first, const m128_t& second) {
				m128_t result;
				result.n[0] = first.n[2];
				result.n[1] = second.n[2];
				result.n[2] = first.n[3];
				result.n[3] = second.n[3];
				return result;
			}
			
			constexpr m128_t movelh(const m128_t& first, const m128_t& second) {
				m128_t result;
				result.n[0] = first.n[0];
				result.n[1] = first.n[1];
				result.n[2] = second.n[0];
				result.n[3] = second.n[1];
				return result;
			}

			constexpr m128_t movehl(const m128_t& first, const m128_t& second) {
				m128_t result;
				result.n[0] = first.n[2];
				result.n[1] = first.n[3];
				result.n[2] = second.n[2];
				result.n[3] = second.n[3];
				return result;
			}
		}
	}
}