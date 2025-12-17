<script setup lang="ts">
import { computed, ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import ReaderLayout from '@/layouts/ReaderLayout.vue'
import Button from '@/components/ui/Button.vue'
import Card from '@/components/ui/Card.vue'
import CardHeader from '@/components/ui/CardHeader.vue'
import CardTitle from '@/components/ui/CardTitle.vue'
import CardContent from '@/components/ui/CardContent.vue'
import Input from '@/components/ui/Input.vue'
import { useCartStore } from '@/stores/cart'
import { useToastStore } from '@/stores/toast'
import { useAuthStore } from '@/stores/auth'
import { orderApi } from '@/api'
import { getBookCoverColor } from '@/lib/utils'
import type { CartItem, CartItemDto } from '@/types'
import { 
  ShoppingCart, 
  Trash2, 
  Plus, 
  Minus, 
  ArrowRight,
  PackageOpen
} from 'lucide-vue-next'

const router = useRouter()
const cartStore = useCartStore()
const toastStore = useToastStore()
const authStore = useAuthStore()

const shippingAddress = ref('')
const contactPhone = ref('')
const submitting = ref(false)
const showCheckout = ref(false)

const isEmpty = computed(() => {
  if (cartStore.useServerCart) {
    return cartStore.serverItems.length === 0
  }
  return cartStore.localItems.length === 0
})

// 获取商品的bookId（兼容两种数据结构）
function getBookId(item: CartItem | CartItemDto): number {
  if ('bookId' in item) {
    return item.bookId
  }
  return item.book.id
}

// 获取商品标题
function getTitle(item: CartItem | CartItemDto): string {
  if ('title' in item && typeof item.title === 'string') {
    return item.title
  }
  return (item as CartItem).book.title
}

// 获取商品作者
function getAuthor(item: CartItem | CartItemDto): string {
  if ('author' in item && typeof item.author === 'string') {
    return item.author
  }
  return (item as CartItem).book.author
}

// 获取商品价格
function getPrice(item: CartItem | CartItemDto): number {
  if ('price' in item && typeof item.price === 'number') {
    return item.price
  }
  return (item as CartItem).book.price || 39.9
}

// 获取可用库存
function getAvailableInventory(item: CartItem | CartItemDto): number {
  if ('inventory' in item) {
    return item.inventory
  }
  const cartItem = item as CartItem
  return cartItem.book.inventory - cartItem.book.borrowed
}

function updateQuantity(item: CartItem | CartItemDto, delta: number) {
  const bookId = getBookId(item)
  cartStore.updateQuantity(bookId, item.quantity + delta)
}

function removeItem(item: CartItem | CartItemDto) {
  cartStore.removeItem(getBookId(item))
}

function goToBooks() {
  router.push('/books')
}

function goToBookDetail(item: CartItem | CartItemDto) {
  router.push(`/book/${getBookId(item)}`)
}

async function handleCheckout() {
  if (!authStore.isLoggedIn) {
    authStore.openLoginModal()
    return
  }
  
  if (!shippingAddress.value.trim()) {
    toastStore.warning('请填写收货地址')
    return
  }
  
  if (!contactPhone.value.trim()) {
    toastStore.warning('请填写联系电话')
    return
  }
  
  submitting.value = true
  try {
    // 构建订单项
    const items = cartStore.useServerCart 
      ? undefined  // 使用服务端购物车时，后端会自动读取购物车
      : cartStore.localItems.map(item => ({
          bookId: item.book.id,
          quantity: item.quantity
        }))
    
    const response = await orderApi.createOrder({
      items,
      shippingAddress: shippingAddress.value,
      receiverPhone: contactPhone.value
    })
    
    if (response.data.code === 0) {
      toastStore.success('订单创建成功！')
      cartStore.clearCart()
      router.push('/orders')
    } else {
      toastStore.error(response.data.message)
    }
  } catch {
    toastStore.error('创建订单失败，请稍后再试')
  } finally {
    submitting.value = false
  }
}

onMounted(() => {
  if (cartStore.useServerCart) {
    cartStore.fetchCart()
  }
})
</script>

<template>
  <ReaderLayout>
    <div class="container px-4 py-8">
      <h1 class="text-3xl font-bold mb-8">
        <ShoppingCart class="inline h-8 w-8 mr-3" />
        购物车
      </h1>
      
      <!-- 空购物车 -->
      <div v-if="isEmpty" class="text-center py-16">
        <PackageOpen class="h-24 w-24 mx-auto text-muted-foreground/50 mb-4" />
        <h2 class="text-xl font-semibold text-muted-foreground mb-2">购物车是空的</h2>
        <p class="text-muted-foreground mb-6">快去挑选心仪的图书吧！</p>
        <Button @click="goToBooks">
          去选购
          <ArrowRight class="h-4 w-4 ml-2" />
        </Button>
      </div>
      
      <!-- 购物车内容 -->
      <div v-else class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <!-- 商品列表 -->
        <div class="lg:col-span-2 space-y-4">
          <Card v-for="item in cartStore.items" :key="getBookId(item)">
            <CardContent class="p-4">
              <div class="flex gap-4">
                <!-- 封面 -->
                <div 
                  :class="[
                    'w-20 h-28 rounded-lg flex-shrink-0 flex items-center justify-center cursor-pointer',
                    getBookCoverColor(getTitle(item))
                  ]"
                  @click="goToBookDetail(item)"
                >
                  <span class="text-white text-xs text-center px-2 line-clamp-3">
                    {{ getTitle(item) }}
                  </span>
                </div>
                
                <!-- 信息 -->
                <div class="flex-1 min-w-0">
                  <h3 
                    class="font-semibold truncate cursor-pointer hover:text-primary"
                    @click="goToBookDetail(item)"
                  >
                    {{ getTitle(item) }}
                  </h3>
                  <p class="text-sm text-muted-foreground">{{ getAuthor(item) }}</p>
                  <p class="text-lg font-bold text-primary mt-2">
                    ¥{{ getPrice(item).toFixed(2) }}
                  </p>
                </div>
                
                <!-- 数量控制 -->
                <div class="flex flex-col items-end justify-between">
                  <Button 
                    variant="ghost" 
                    size="sm"
                    @click="removeItem(item)"
                  >
                    <Trash2 class="h-4 w-4 text-destructive" />
                  </Button>
                  
                  <div class="flex items-center gap-2">
                    <Button 
                      variant="outline" 
                      size="sm"
                      :disabled="item.quantity <= 1"
                      @click="updateQuantity(item, -1)"
                    >
                      <Minus class="h-3 w-3" />
                    </Button>
                    <span class="w-8 text-center">{{ item.quantity }}</span>
                    <Button 
                      variant="outline" 
                      size="sm"
                      :disabled="item.quantity >= getAvailableInventory(item)"
                      @click="updateQuantity(item, 1)"
                    >
                      <Plus class="h-3 w-3" />
                    </Button>
                  </div>
                  
                  <p class="text-sm font-medium">
                    小计: ¥{{ (getPrice(item) * item.quantity).toFixed(2) }}
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
        
        <!-- 结算区 -->
        <div class="lg:col-span-1">
          <Card class="sticky top-24">
            <CardHeader>
              <CardTitle>订单结算</CardTitle>
            </CardHeader>
            <CardContent class="space-y-4">
              <div class="flex justify-between text-sm">
                <span class="text-muted-foreground">商品数量</span>
                <span>{{ cartStore.itemCount }} 件</span>
              </div>
              
              <div class="flex justify-between text-lg font-bold">
                <span>合计</span>
                <span class="text-primary">¥{{ cartStore.totalAmount.toFixed(2) }}</span>
              </div>
              
              <hr class="my-4" />
              
              <!-- 收货信息 -->
              <div v-if="showCheckout" class="space-y-4">
                <div class="space-y-2">
                  <label class="text-sm font-medium">收货地址</label>
                  <Input 
                    v-model="shippingAddress" 
                    placeholder="请输入详细收货地址"
                  />
                </div>
                
                <div class="space-y-2">
                  <label class="text-sm font-medium">联系电话</label>
                  <Input 
                    v-model="contactPhone" 
                    placeholder="请输入联系电话"
                  />
                </div>
                
                <Button 
                  class="w-full" 
                  size="lg"
                  :loading="submitting"
                  @click="handleCheckout"
                >
                  确认下单
                </Button>
                
                <Button 
                  variant="ghost" 
                  class="w-full"
                  @click="showCheckout = false"
                >
                  返回修改
                </Button>
              </div>
              
              <Button 
                v-else
                class="w-full" 
                size="lg"
                @click="showCheckout = true"
              >
                去结算
                <ArrowRight class="h-4 w-4 ml-2" />
              </Button>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  </ReaderLayout>
</template>
