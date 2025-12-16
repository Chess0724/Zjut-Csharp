<script setup lang="ts">
import { computed } from 'vue'
import Button from './Button.vue'
import { ChevronLeft, ChevronRight, ChevronsLeft, ChevronsRight } from 'lucide-vue-next'

interface Props {
  currentPage: number
  totalPages: number
  totalItems: number
  pageSize: number
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'update:currentPage': [page: number]
}>()

const displayPages = computed(() => {
  const pages: (number | '...')[] = []
  const total = props.totalPages
  const current = props.currentPage
  
  if (total <= 7) {
    for (let i = 1; i <= total; i++) {
      pages.push(i)
    }
  } else {
    pages.push(1)
    
    if (current > 3) {
      pages.push('...')
    }
    
    const start = Math.max(2, current - 1)
    const end = Math.min(total - 1, current + 1)
    
    for (let i = start; i <= end; i++) {
      pages.push(i)
    }
    
    if (current < total - 2) {
      pages.push('...')
    }
    
    pages.push(total)
  }
  
  return pages
})

function goToPage(page: number) {
  if (page >= 1 && page <= props.totalPages) {
    emit('update:currentPage', page)
  }
}
</script>

<template>
  <div class="flex items-center justify-between px-2 py-4">
    <div class="text-sm text-muted-foreground">
      共 {{ totalItems }} 条记录，第 {{ currentPage }} / {{ totalPages }} 页
    </div>
    
    <div class="flex items-center gap-1">
      <Button
        variant="outline"
        size="icon"
        :disabled="currentPage <= 1"
        @click="goToPage(1)"
      >
        <ChevronsLeft class="h-4 w-4" />
      </Button>
      
      <Button
        variant="outline"
        size="icon"
        :disabled="currentPage <= 1"
        @click="goToPage(currentPage - 1)"
      >
        <ChevronLeft class="h-4 w-4" />
      </Button>
      
      <template v-for="page in displayPages" :key="page">
        <Button
          v-if="page !== '...'"
          :variant="page === currentPage ? 'default' : 'outline'"
          size="sm"
          @click="goToPage(page)"
        >
          {{ page }}
        </Button>
        <span v-else class="px-2 text-muted-foreground">...</span>
      </template>
      
      <Button
        variant="outline"
        size="icon"
        :disabled="currentPage >= totalPages"
        @click="goToPage(currentPage + 1)"
      >
        <ChevronRight class="h-4 w-4" />
      </Button>
      
      <Button
        variant="outline"
        size="icon"
        :disabled="currentPage >= totalPages"
        @click="goToPage(totalPages)"
      >
        <ChevronsRight class="h-4 w-4" />
      </Button>
    </div>
  </div>
</template>
