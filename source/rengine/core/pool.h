#pragma once
#include <rengine/types.h>
#include <rengine/exceptions.h>
#include <rengine/strings.h>
#include <rengine/core/entity-utils.h>

#include <fmt/format.h>

namespace rengine {
    namespace core {
        template<typename T, typename Entity = u16>
        struct pool_entry {
            Entity id;
            T value;
        };

        template<typename T, u32 N, typename Entity = u16, Entity InvalidId = 0xFFFFFFFF>
        class array_pool {
        public:
            constexpr array_pool(c_str identifier = strings::g_pool_id): 
                count_(0), 
                magic_(0), 
                identifier_(identifier) {
                for (u32 i = 0; i < N; ++i) {
                    entries_[i].id = InvalidId;
                    available_idx_[(N - 1) - i] = i;
                }
            }
            
            class Iterator {
            public:
				Iterator() : pool_(nullptr), index_(0) {}
				Iterator(array_pool<T, N>* pool, u32 index) : pool_(pool), index_(index) {}
				const pool_entry<T, Entity>& operator*() {
                    if (index_ == N)
                        return pool_entry<T, Entity>{};
					return pool_->entries_[index_];
				}
                const pool_entry<T, Entity>& operator*() const {
                    if (index_ == N)
                        return pool_entry<T, Entity>{};
                    return pool_->entries_[index_];
                }

				Iterator& operator++() {
                    index_ = find_next_valid_idx();
					return *this;
				}

				bool operator!=(const Iterator& other) const {
					return index_ != other.index_;
				}
            private:
                u32 find_next_valid_idx() {
					for (u32 i = index_ + 1; i < N; ++i) {
                        if (pool_->entries_[i].id == InvalidId)
                            continue;
						return i;
					}
					return N;
                }

				array_pool<T, N>* pool_;
				u16 index_;
            };

            constexpr Entity push_back(const T& value) {
                if (is_full())
                    throw pool_exception(
                        fmt::format(strings::exceptions::g_pool_is_full, N).c_str()
                    );

                pool_entry<T> entry;
                const auto idx = available_idx_[(N - count_) - 1];
                entry.id = encode_id(idx, ++magic_);
				entry.value = value;
                entries_[idx] = entry;

                ++count_;
                return entry.id;
            }

            constexpr void erase(const Entity& id) {
                if (empty())
                    return;

                const auto idx = decode_id(id);
                auto& entry = entries_[idx];
                // must remove only if the whole id matches with the current entry
                if (entry.id != id)
                    return;

                entry.id = invalid_id;
                --count_;
                available_idx_[(N - count_) - 1] = idx;
            }

			constexpr void overwrite(const Entity& id, const T& value) {
				if (!is_valid(id))
					throw pool_exception(
						fmt::format(strings::exceptions::g_pool_invalid_id, id).c_str()
					);
				const auto& idx = decode_id(id);
				auto& entry = entries_[idx];
				entry.value = value;
			}

            constexpr Entity replace(const Entity& id, const T& value) {
                if (!is_valid(id))
                    throw pool_exception(
                        fmt::format(strings::exceptions::g_pool_invalid_id, id).c_str()
                    );

                const auto& idx = decode_id(id);
                auto& entry = entries_[idx];
                entry.id = encode_id(idx, ++magic_);
                entry.value = value;

                return entry.id;
            }

            constexpr Entity regenerate_id(const Entity& id) {
                if (!is_valid(id))
                    throw pool_exception(
                        fmt::format(strings::exceptions::g_pool_invalid_id, id).c_str()
                    );

                const auto& idx = decode_id(idx);
                auto& entry = entries_[idx];
                entry.id = encode_id(idx, ++magic_);
                return entry.id;
            }

            constexpr void clear() {
                for(u32 i = 0; i < N; ++i)
					entries_[i].id = InvalidId;
				count_ = magic_ = 0;
            }

            constexpr const pool_entry<T, Entity>* data() noexcept {
                return entries_;
            }
            constexpr const pool_entry<T, Entity>* data() const noexcept {
                return entries_;
            }

            constexpr const pool_entry<T, Entity>& at(u32 idx) {
                if (idx > count_)
                    throw pool_exception(
                        fmt::format(strings::exceptions::g_pool_out_of_range, idx).c_str()
                    );
                return &entries_[idx];
            }
            constexpr const pool_entry<T, Entity>& at(u32 idx) const noexcept {
                if (idx > count_)
                    throw pool_exception(
                        fmt::format(strings::exceptions::g_pool_out_of_range, idx).c_str()
                    );
                return &entries_[idx];
            }

            constexpr u32 count() const noexcept { return count_; }
            constexpr u32 size() const noexcept { return (u32)N; }
            constexpr u32 max_size() const noexcept { return (u32)N; }

            constexpr bool empty() const noexcept { return count_ == 0u; }
            constexpr bool is_full() const noexcept { return count_ == N; }
            constexpr bool is_valid(const Entity& id) const noexcept {
                const auto idx = decode_id(id);
                if (idx >= N)
                    return false;

                auto& entry = entries_[idx];
                return entry.id == id;
            }

            constexpr Iterator begin() noexcept {
                Iterator it;
				find_valid_begin_iterator(&it);
                return it;
            }
            constexpr Iterator begin() const noexcept {
                Iterator it;
                find_valid_begin_iterator(&it);
                return it;
            }
			constexpr const Iterator cbegin() const noexcept {
                return Iterator(this, 0);
			}
            constexpr const pool_entry<T, Entity>& front() {
                return &entries_[0];
            }
            constexpr const pool_entry<T, Entity>& front() const {
                return &entries_[0];
            }
			constexpr Iterator end() noexcept {
                return Iterator(this, N);
			}
			constexpr const Iterator end() const noexcept {
                return Iterator(this, N);
			}
            constexpr const Iterator cend() const noexcept {
                return Iterator(this, N);
            }

            constexpr const pool_entry<T>& operator[](Entity id) {
                if (!is_valid(id))
                    throw pool_exception(
                        fmt::format(strings::exceptions::g_pool_invalid_id, id).c_str()
                    );

                const auto& idx = decode_id(id);
                return entries_[idx];
            }
            constexpr const pool_entry<T>& operator[](Entity id) const {
                if (!is_valid(id))
                    throw pool_exception(
                        fmt::format(strings::exceptions::g_pool_invalid_id, id).c_str()
                    );

                const auto& idx = decode_id(id);
                return entries_[idx];
            }

            static constexpr Entity invalid_id = (Entity)InvalidId;
        private:
            void find_valid_begin_iterator(Iterator* it_out) {
                if (empty()) {
                    *it_out = end();
                    return;
                }

                for (u32 i = 0; i < N; ++i) {
					if (entries_[i].id == invalid_id)
						continue;
					*it_out = Iterator(this, i);
                    return;
                }
            }
            /*void fix_data_gaps() {
                pool_entry<T, Entity> new_entries[N];
                u32 next_idx = 0;

                for (u32 i = 0; i < N; ++i) {
                    auto& entry_idx = entry_indexes_[i];
                    if (entry_idx == invalid_id)
                        continue;

					new_entries[next_idx] = entries_[entry_idx];
1					entry_indexes_[i] = next_idx;
                    ++next_idx;
                }

				memcpy(entries_, new_entries, sizeof(T) * N);
            }*/

            static constexpr Entity encode_id(u32 idx, u8 magic) {
				entity_id_encoder<Entity> encoder;
				return encoder.encode(idx, magic);
            }
            static constexpr u32 decode_id(Entity id) {
				entity_id_encoder<Entity> encoder;
				return encoder.decode(id);
            }

            c_str identifier_;

            pool_entry<T, Entity> entries_[N];
            Entity available_idx_[N];

            u32 count_;
            u8 magic_;
        };
    }
}