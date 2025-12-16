<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import type { Book } from '@/types'
import { getBookCoverColor, truncate } from '@/lib/utils'
import Badge from '@/components/ui/Badge.vue'
import Button from '@/components/ui/Button.vue'
import { BookOpen, User, Building2 } from 'lucide-vue-next'

interface Props {
  book: Book
  showActions?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  showActions: true
})

const emit = defineEmits<{
  'borrow': [book: Book]
}>()

const router = useRouter()

const coverColor = computed(() => getBookCoverColor(props.book.title))
const isAvailable = computed(() => props.book.inventory > 0)

function goToDetail() {
  router.push({ name: 'BookDetail', params: { id: props.book.id } })
}
</script>

<template>
  <div 
    class="group relative bg-card rounded-xl border shadow-sm overflow-hidden transition-all duration-300 hover:shadow-lg hover:-translate-y-1 cursor-pointer"
    @click="goToDetail"
  >
    <!-- 书籍封面 -->
    <div 
      :class="[
        'relative h-48 bg-gradient-to-br flex items-center justify-center p-6',
        coverColor
      ]"
    >
      <!-- 装饰线条 -->
      <div class="absolute inset-0 opacity-10">
        <div class="absolute top-4 left-4 right-4 h-px bg-white" />
        <div class="absolute bottom-4 left-4 right-4 h-px bg-white" />
        <div class="absolute top-4 bottom-4 left-4 w-px bg-white" />
        <div class="absolute top-4 bottom-4 right-4 w-px bg-white" />
      </div>
      
      <!-- 书名 -->
      <h3 class="relative text-white text-center font-serif text-xl font-bold leading-tight line-clamp-3">
        {{ book.title }}
      </h3>
      
      <!-- 库存状态徽标 -->
      <Badge 
        :variant="isAvailable ? 'success' : 'destructive'"
        class="absolute top-3 right-3"
      >
        {{ isAvailable ? `库存 ${book.inventory}` : '暂无库存' }}
      </Badge>
    </div>
    
    <!-- 书籍信息 -->
    <div class="p-4 space-y-3">
      <div class="space-y-2">
        <div class="flex items-center gap-2 text-sm text-muted-foreground">
          <User class="h-4 w-4" />
          <span class="truncate">{{ book.author }}</span>
        </div>
        <div class="flex items-center gap-2 text-sm text-muted-foreground">
          <Building2 class="h-4 w-4" />
          <span class="truncate">{{ book.publisher }}</span>
        </div>
        <div class="flex items-center gap-2 text-sm text-muted-foreground">
          <BookOpen class="h-4 w-4" />
          <span>{{ book.identifier }}</span>
        </div>
      </div>
      
      <!-- 操作按钮 -->
      <div v-if="showActions" class="pt-2">
        <Button
          v-if="isAvailable"
          class="w-full"
          size="sm"
          @click.stop="emit('borrow', book)"
        >
          立即借阅
        </Button>
        <Button
          v-else
          variant="outline"
          class="w-full"
          size="sm"
          disabled
        >
          暂无库存
        </Button>
      </div>
    </div>
  </div>
</template>
