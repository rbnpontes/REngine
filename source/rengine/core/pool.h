#pragma once
#include <rengine/types.h>
#include <rengine/exceptions.h>
#include <rengine/strings.h>

#include <fmt/format.h>

namespace rengine {
    namespace core {
        template<typename T, typename Entity>
        struct pool_entry {
            Entity id;
            T value;
        };

        template<typename T, u32 N, typename Entity = u16>
        class array_pool {
        public:
            constexpr array_pool(c_str identifier = strings::g_pool_id): 
                count_(0), 
                magic_(0), 
                identifier_(identifier) {
                for (u32 i = 0; i < N; ++i) {
                    entries_[i].id = invalid_id;
                    available_idx_[(N - 1) - i] = i;
                }
            }

            constexpr Entity store(const T& value) {
                if (is_full())
                    throw pool_exception(
                        fmt::format(strings::exceptions::g_pool_is_full, N).c_str()
                    );

                pool_entry<T, Entity> entry;
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

            constexpr T* data() {
                return entries_;
            }
            constexpr T* data() const {
                return entries_;
            }

            constexpr u32 count() const { return count_; }
            constexpr u32 size() const { return (u32)N; }
            constexpr u32 max_size() const { return (u32)N; }

            constexpr bool empty() const { return count_ == 0u; }
            constexpr bool is_full() const { return count_ == N; }
            constexpr bool is_valid(const Entity& id) const {
                const auto idx = decode_id(id);
                if (idx >= N)
                    return;

                auto& entry = entries_[idx];
                return entry.id == id;
            }

            constexpr T& front() {
                return &entries_[0];
            }
            constexpr T& front() const {
                return &entries_[0];
            }
            constexpr T& back() {
                return &entries_[count_ - 1];
            }
            constexpr T& back() const {
                return &entries_[count_ - 1];
            }

            constexpr T& operator[](Entity id) {
                if (!is_valid(id))
                    throw pool_exception(
                        fmt::format(strings::exceptions::g_pool_invalid_id, id).c_str()
                    );

                const auto& idx = decode_id(id);
                return entries_[idx];
            }
            constexpr T& operator[](Entity id) const {
                if (!is_valid(id))
                    throw pool_exception(
                        fmt::format(strings::exceptions::g_pool_invalid_id, id).c_str()
                    );

                const auto& idx = decode_id(id);
                return entries_[idx];
            }

            static constexpr Entity invalid_id = (Entity)0xFFFFFFFF;
        private:
            static constexpr Entity encode_id(u32 idx, u8 magic) {
                constexpr u8 shift_value = (sizeof(Entity) * 2) - 8;
                return (idx << shift_value) | magic;
            }

            static constexpr u32 decode_id(Entity id) {
                constexpr u8 shift_value = (sizeof(Entity) * 2) - 8;
                return id >> shift_value;
            }

            c_str identifier_;

            pool_entry<T, Entity> entries_[N];
            Entity available_idx_[N];

            u32 count_;
            u8 magic_;
        };
    }
}