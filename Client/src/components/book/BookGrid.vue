<script setup lang="ts">
import type { Book } from '@/types'
import BookCard from './BookCard.vue'
import BookCardSkeleton from './BookCardSkeleton.vue'

interface Props {
  books: Book[]
  loading?: boolean
  skeletonCount?: number
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  skeletonCount: 8
})

const emit = defineEmits<{
  'borrow': [book: Book]
}>()
</script>

<template>
  <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
    <!-- 加载中骨架屏 -->
    <template v-if="loading">
      <BookCardSkeleton v-for="i in skeletonCount" :key="i" />
    </template>
    
    <!-- 书籍卡片 -->
    <template v-else>
      <BookCard
        v-for="book in books"
        :key="book.id"
        :book="book"
        @borrow="emit('borrow', $event)"
      />
    </template>
    
    <!-- 空状态 -->
    <div
      v-if="!loading && books.length === 0"
      class="col-span-full flex flex-col items-center justify-center py-16 text-muted-foreground"
    >
      <svg
        class="h-16 w-16 mb-4 opacity-50"
        xmlns="http://www.w3.org/2000/svg"
        fill="none"
        viewBox="0 0 24 24"
        stroke="currentColor"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="1.5"
          d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"
        />
      </svg>
      <p class="text-lg font-medium">暂无图书</p>
      <p class="text-sm">尝试更换搜索关键词</p>
    </div>
  </div>
</template>
