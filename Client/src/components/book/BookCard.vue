<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import type { Book } from '@/types'
import { getBookCoverColor, formatCurrency } from '@/lib/utils'
import Badge from '@/components/ui/Badge.vue'
import Button from '@/components/ui/Button.vue'
import { BookOpen, User, Building2, ShoppingCart } from 'lucide-vue-next'
import { useCartStore } from '@/stores/cart'

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
const cartStore = useCartStore()

const coverColor = computed(() => getBookCoverColor(props.book.title))
// 借阅可用库存 = 借阅库存 - 已借出，最小为 0
const availableBorrowStock = computed(() => Math.max(0, (props.book.inventory || 0) - (props.book.borrowed || 0)))
// 销售库存
const availableSaleStock = computed(() => props.book.saleInventory || 0)
// 是否可借阅
const canBorrow = computed(() => availableBorrowStock.value > 0)
// 是否可购买
const canBuy = computed(() => availableSaleStock.value > 0)
const bookPrice = computed(() => props.book.price ?? 39.9)

function goToDetail() {
  router.push({ name: 'BookDetail', params: { id: props.book.id } })
}

function handleAddToCart() {
  cartStore.addItem(props.book, 1)
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
        :variant="canBorrow || canBuy ? 'success' : 'destructive'"
        class="absolute top-3 right-3"
      >
        {{ canBorrow ? `可借 ${availableBorrowStock}` : (canBuy ? '仅售' : '暂无库存') }}
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
      
      <!-- 价格和库存 -->
      <div class="flex items-center justify-between pt-2 border-t">
        <span class="text-lg font-bold text-primary">{{ formatCurrency(bookPrice) }}</span>
        <div class="text-xs text-muted-foreground text-right">
          <div>可借 {{ availableBorrowStock }}</div>
          <div>可购 {{ availableSaleStock }}</div>
        </div>
      </div>
      
      <!-- 操作按钮 -->
      <div v-if="showActions" class="flex gap-2">
        <Button
          v-if="canBorrow"
          class="flex-1"
          size="sm"
          @click.stop="emit('borrow', book)"
        >
          借阅
        </Button>
        <Button
          v-if="canBuy"
          variant="outline"
          size="sm"
          class="flex items-center gap-1"
          @click.stop="handleAddToCart"
        >
          <ShoppingCart class="h-4 w-4" />
          购买
        </Button>
        <Button
          v-if="!canBorrow && !canBuy"
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
