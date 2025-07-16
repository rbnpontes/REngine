#include "arena.h"
#include "allocator.h"
#include "./arena_private.h"

#include "../exceptions.h"

namespace rengine {
	namespace core {
		class DefaultArena : public IArena {
		public:
			DefaultArena(): IArena() {}
			~DefaultArena() override {}

			ptr alloc(const size_t size) override {
				if (!size)
					return null;

				usage_ += size;
				return core::alloc(size);
			}

			ptr realloc(ptr mem, const size_t new_size) override {
				if (!mem)
					return null;

				usage_ -= alloc_get_pointer_size(mem);
				usage_ += new_size;
				return core::alloc_realloc(mem, new_size);
			}

			void free(ptr mem) override {
				if (!mem)
					return;

				usage_ -= alloc_get_pointer_size(mem);
				return core::alloc_free(mem);
			}

			size_t usage() const override { return usage_; }
			size_t size() const override { return usage_; }
		private:
			size_t usage_{ 0 };
		};
		
		class FrameArena : public IFrameArena {
		public:
			struct untrack_mem_link_t {
				untrack_mem_link_t* prev;
				untrack_mem_link_t* next;
			};

			FrameArena() {}
			~FrameArena() override {
				destroy_all_blocks();
				core::alloc_free(bucket_mem_);
			}

			void init_bucket_mem(const size_t size) {
				bucket_mem_ = (byte*)core::alloc(size);
				bucket_mem_size_ = size;
				curr_bucket_mem_size_ = 0;
			}

			ptr alloc(const size_t size) override {
				ptr result = null;
				const auto available_bucket_size = bucket_mem_size_ - curr_bucket_mem_size_;

				if (0 == size)
					return result;

				usage_ += size;
				if (size <= available_bucket_size) {
					result = bucket_mem_ + curr_bucket_mem_size_;
					curr_bucket_mem_size_ += size;
					return result;
				}

				byte* untrack_mem = (byte*)core::alloc(sizeof(untrack_mem_link_t) + size);
				untrack_mem_link_t* link = (untrack_mem_link_t*)untrack_mem;
				link->prev = links_;
				link->next = null;
				if (links_)
					links_->next = link;
				links_ = link;

				++num_blocks_;
				return untrack_mem + sizeof(untrack_mem_link_t);
			}

			ptr realloc(ptr mem, const size_t new_size) override {
				return alloc(new_size);
			}

			void free(ptr mem) override {}

			void reset() override {
				if (usage_ <= bucket_mem_size_) {
					usage_ = curr_bucket_mem_size_ = 0;
					return;
				}

				if (bucket_mem_)
					core::alloc_free(bucket_mem_);
				init_bucket_mem(usage_);
				usage_ = 0;
			}

			void destroy_block() override {
				if (!links_)
					return;
				auto* curr_link = links_;
				links_ = curr_link->prev;

				if (links_)
					links_->next = null;

				if (links_ == curr_link)
					links_ = null;

				--num_blocks_;
				core::alloc_free(curr_link);
			}

			void destroy_all_blocks() override {
				while (links_)
					destroy_block();
			}

			size_t usage() const override { return usage_; }
			size_t size() const override { return usage_; }
			size_t get_blocks_count() const override { return num_blocks_; }
		protected:
			byte* bucket_mem_{null};
			size_t bucket_mem_size_{ 0 };
			size_t curr_bucket_mem_size_{ 0 };
			size_t num_blocks_{ 0 };
			size_t usage_{ 0 };

			untrack_mem_link_t* links_{ null };
		};

		class FixedArena : public FrameArena {
		public:
			FixedArena() : FrameArena() {}
			void set_capacity(const size_t capacity) {
				capacity_ = capacity;
			}

			ptr alloc(const size_t size) override {
				const auto available_space = capacity_ - usage_;
				if (size > available_space)
					throw out_of_memory_exception();

				return FrameArena::alloc(size);
			}
		private:
			size_t capacity_{ 0 };
		};

		class ScratchArena : public IScratchArena {
		public:
			ScratchArena() : IScratchArena() {}
			~ScratchArena() override {
				core::alloc_free(buffer_);
			}

			void resize(const size_t scratch_size) override {
				if (buffer_)
					core::alloc_free(buffer_);
				buffer_ = (byte*)core::alloc(scratch_size);
				size_ = scratch_size;
				usage_ = 0;
			}

			ptr alloc(const size_t size) override {
				if (0 == size)
					return null;

				const auto available_space = size_ - usage_;
				if (size > available_space)
					throw out_of_memory_exception();

				ptr result = buffer_ + usage_;
				usage_ += size;
				return result;
			}

			ptr realloc(ptr mem, const size_t new_size) override {
				if (!mem)
					return null;
				usage_ -= alloc_get_pointer_size(mem);
				return alloc(new_size);
			}

			void free(ptr mem) {
				if (!mem)
					return;
				usage_ -= alloc_get_pointer_size(mem);
			}
		

			size_t usage() const override {
				return usage_;
			}

			size_t size() const override {
				return size_;
			}
		private:
			byte* buffer_{ null };
			size_t usage_{ 0 };
			size_t size_{ 0 };
		};

		IArena* arena_create_default()
		{
			auto arena = arena__alloc<DefaultArena>(arena_kind::normal);
			arena__push(arena);
			return arena;
		}

		IFrameArena* arena_create_frame(const size_t initial_size)
		{
			auto arena = arena__alloc<FrameArena>(arena_kind::frame);
			arena->init_bucket_mem(initial_size);
			arena__push(arena);
			return arena;
		}

		IFrameArena* arena_create_fixed(const size_t max_size)
		{
			auto arena = arena__alloc<FixedArena>(arena_kind::fixed);
			arena->init_bucket_mem(max_size);
			arena->set_capacity(max_size);
			arena__push(arena);
			return arena;
		}

		IScratchArena* arena_create_scratch(const size_t scratch_size)
		{ 
			auto arena = arena__alloc<ScratchArena>(arena_kind::scratch);
			arena->resize(scratch_size);
			return arena;
		}

		void arena_destroy(IArena* arena)
		{
			arena__destroy(arena);
		}

		IArena* arena_get_default()
		{
			return g_arena_state.default_arena;
		}

		IScratchArena* arena_get_scratch()
		{
			return g_arena_state.scratch_arena;
		}

	}
}