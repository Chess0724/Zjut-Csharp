<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import ReaderLayout from '@/layouts/ReaderLayout.vue'
import Button from '@/components/ui/Button.vue'
import Badge from '@/components/ui/Badge.vue'
import Card from '@/components/ui/Card.vue'
import CardContent from '@/components/ui/CardContent.vue'
import { orderApi } from '@/api'
import { useToastStore } from '@/stores/toast'
import type { Order, OrderStatus } from '@/types'
import { 
  Package, 
  Clock, 
  Truck, 
  CheckCircle, 
  XCircle,
  CreditCard,
  ChevronRight,
  ShoppingBag
} from 'lucide-vue-next'

const router = useRouter()
const toastStore = useToastStore()

const orders = ref<Order[]>([])
const loading = ref(true)

type BadgeVariant = 'default' | 'destructive' | 'outline' | 'secondary' | 'success' | 'warning' | 'info'

const statusConfig: Record<OrderStatus, { label: string; variant: BadgeVariant; icon: any }> = {
  Pending: { label: '待支付', variant: 'warning', icon: Clock },
  Paid: { label: '已支付', variant: 'info', icon: CreditCard },
  Shipped: { label: '已发货', variant: 'info', icon: Truck },
  Delivered: { label: '已送达', variant: 'success', icon: Package },
  Completed: { label: '已完成', variant: 'success', icon: CheckCircle },
  Cancelled: { label: '已取消', variant: 'secondary', icon: XCircle },
}

async function fetchOrders() {
  loading.value = true
  try {
    const response = await orderApi.getMyOrders({ pageSize: 50 })
    if (response.data.code === 0) {
      orders.value = response.data.data || []
    }
  } catch (error) {
    console.error('Failed to fetch orders:', error)
  } finally {
    loading.value = false
  }
}

async function handlePay(orderId: number) {
  try {
    const response = await orderApi.payOrder(orderId)
    if (response.data.code === 0) {
      toastStore.success('支付成功！')
      fetchOrders()
    } else {
      toastStore.error(response.data.message)
    }
  } catch {
    toastStore.error('支付失败')
  }
}

async function handleCancel(orderId: number) {
  try {
    const response = await orderApi.cancelOrder(orderId)
    if (response.data.code === 0) {
      toastStore.success('订单已取消')
      fetchOrders()
    } else {
      toastStore.error(response.data.message)
    }
  } catch {
    toastStore.error('取消失败')
  }
}

async function handleConfirm(orderId: number) {
  try {
    const response = await orderApi.confirmOrder(orderId)
    if (response.data.code === 0) {
      toastStore.success('已确认收货')
      fetchOrders()
    } else {
      toastStore.error(response.data.message)
    }
  } catch {
    toastStore.error('确认失败')
  }
}

function goToBooks() {
  router.push('/books')
}

onMounted(() => {
  fetchOrders()
})
</script>

<template>
  <ReaderLayout>
    <div class="container px-4 py-8">
      <h1 class="text-3xl font-bold mb-8">
        <ShoppingBag class="inline h-8 w-8 mr-3" />
        我的订单
      </h1>
      
      <!-- 加载状态 -->
      <div v-if="loading" class="space-y-4">
        <div v-for="i in 3" :key="i" class="animate-pulse">
          <div class="h-32 bg-muted rounded-xl" />
        </div>
      </div>
      
      <!-- 空状态 -->
      <div v-else-if="orders.length === 0" class="text-center py-16">
        <ShoppingBag class="h-24 w-24 mx-auto text-muted-foreground/50 mb-4" />
        <h2 class="text-xl font-semibold text-muted-foreground mb-2">暂无订单</h2>
        <p class="text-muted-foreground mb-6">快去挑选心仪的图书吧！</p>
        <Button @click="goToBooks">
          去选购
          <ChevronRight class="h-4 w-4 ml-2" />
        </Button>
      </div>
      
      <!-- 订单列表 -->
      <div v-else class="space-y-4">
        <Card v-for="order in orders" :key="order.id">
          <CardContent class="p-6">
            <!-- 订单头部 -->
            <div class="flex items-center justify-between mb-4 pb-4 border-b">
              <div class="flex items-center gap-4">
                <span class="text-sm text-muted-foreground">
                  订单号: {{ order.id }}
                </span>
                <span class="text-sm text-muted-foreground">
                  {{ order.createTime }}
                </span>
              </div>
              <Badge :variant="statusConfig[order.status]?.variant || 'secondary'">
                <component 
                  :is="statusConfig[order.status]?.icon" 
                  class="h-3 w-3 mr-1"
                />
                {{ statusConfig[order.status]?.label || order.status }}
              </Badge>
            </div>
            
            <!-- 订单商品 -->
            <div class="space-y-3 mb-4">
              <div 
                v-for="item in order.items" 
                :key="item.bookId"
                class="flex items-center justify-between"
              >
                <div class="flex-1">
                  <p class="font-medium">{{ item.title }}</p>
                  <p class="text-sm text-muted-foreground">{{ item.author }}</p>
                </div>
                <div class="text-right">
                  <p class="text-sm">¥{{ item.price.toFixed(2) }} × {{ item.quantity }}</p>
                  <p class="font-medium">¥{{ (item.price * item.quantity).toFixed(2) }}</p>
                </div>
              </div>
            </div>
            
            <!-- 订单底部 -->
            <div class="flex items-center justify-between pt-4 border-t">
              <div class="text-sm text-muted-foreground">
                <p>收货地址: {{ order.shippingAddress }}</p>
                <p>联系电话: {{ order.contactPhone }}</p>
              </div>
              
              <div class="flex items-center gap-4">
                <div class="text-right">
                  <span class="text-sm text-muted-foreground">合计: </span>
                  <span class="text-xl font-bold text-primary">
                    ¥{{ order.totalAmount.toFixed(2) }}
                  </span>
                </div>
                
                <!-- 操作按钮 -->
                <div class="flex gap-2">
                  <Button 
                    v-if="order.status === 'Pending'"
                    size="sm"
                    @click="handlePay(order.id)"
                  >
                    立即支付
                  </Button>
                  <Button 
                    v-if="order.status === 'Pending'"
                    variant="outline"
                    size="sm"
                    @click="handleCancel(order.id)"
                  >
                    取消订单
                  </Button>
                  <Button 
                    v-if="order.status === 'Delivered'"
                    size="sm"
                    @click="handleConfirm(order.id)"
                  >
                    确认收货
                  </Button>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  </ReaderLayout>
</template>
