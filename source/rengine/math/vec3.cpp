#include "./vec3.h"

namespace rengine {
	namespace math {
        template<typename T> const base_vec3<T> base_vec3<T>::zero = base_vec3<T>((T)0, (T)0, (T)0);
        template<typename T> const base_vec3<T> base_vec3<T>::one = base_vec3<T>((T)1, (T)1, (T)1);
        template<typename T> const base_vec3<T> base_vec3<T>::left = base_vec3<T>((T)-1, (T)0, (T)0);
        template<typename T> const base_vec3<T> base_vec3<T>::right = base_vec3<T>((T)1, (T)0, (T)0);
        template<typename T> const base_vec3<T> base_vec3<T>::up = base_vec3<T>((T)0, (T)1, (T)0);
        template<typename T> const base_vec3<T> base_vec3<T>::down = base_vec3<T>((T)0, (T)-1, (T)0);
        template<typename T> const base_vec3<T> base_vec3<T>::forward = base_vec3<T>((T)0, (T)0, (T)1);
        template<typename T> const base_vec3<T> base_vec3<T>::backward = base_vec3<T>((T)0, (T)0, (T)-1);

        template struct base_vec3<number_t>;
        template struct base_vec3<int_t>;
        template struct base_vec3<uint_t>;
	}
}