<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import Card from '@/components/ui/Card.vue'
import CardHeader from '@/components/ui/CardHeader.vue'
import CardTitle from '@/components/ui/CardTitle.vue'
import CardContent from '@/components/ui/CardContent.vue'
import Button from '@/components/ui/Button.vue'
import Badge from '@/components/ui/Badge.vue'
import DataTable from '@/components/ui/DataTable.vue'
import Pagination from '@/components/ui/Pagination.vue'
import Input from '@/components/ui/Input.vue'
import { orderApi } from '@/api'
import { useToastStore } from '@/stores/toast'
import type { Order, OrderStatus } from '@/types'
import { 
  Search, 
  Package, 
  Truck, 
  Clock, 
  CheckCircle, 
  XCircle,
  CreditCard,
  DollarSign
} from 'lucide-vue-next'

const toastStore = useToastStore()

const orders = ref<Order[]>([])
const loading = ref(true)
const currentPage = ref(1)
const totalPages = ref(1)
const totalItems = ref(0)
const pageSize = ref(15)
const searchQuery = ref('')
const activeTab = ref<'all' | 'pending' | 'paid' | 'shipped'>('all')

type BadgeVariant = 'default' | 'destructive' | 'outline' | 'secondary' | 'success' | 'warning' | 'info'

const statusConfig: Record<OrderStatus, { label: string; variant: BadgeVariant; icon: any }> = {
  Pending: { label: '待支付', variant: 'warning', icon: Clock },
  Paid: { label: '已支付', variant: 'info', icon: CreditCard },
  Shipped: { label: '已发货', variant: 'info', icon: Truck },
  Delivered: { label: '已送达', variant: 'success', icon: Package },
  Completed: { label: '已完成', variant: 'success', icon: CheckCircle },
  Cancelled: { label: '已取消', variant: 'secondary', icon: XCircle },
}

const tabs = [
  { key: 'all', label: '全部订单' },
  { key: 'pending', label: '待支付' },
  { key: 'paid', label: '待发货' },
  { key: 'shipped', label: '已发货' },
]

const columns = [
  { key: 'id', label: '订单号', width: '80px' },
  { key: 'userName', label: '用户' },
  { key: 'items', label: '商品' },
  { key: 'totalAmount', label: '金额', width: '100px' },
  { key: 'status', label: '状态', width: '100px' },
  { key: 'createTime', label: '下单时间', width: '160px' },
]

const filteredOrders = computed(() => {
  if (activeTab.value === 'all') return orders.value
  if (activeTab.value === 'pending') return orders.value.filter(o => o.status === 'Pending')
  if (activeTab.value === 'paid') return orders.value.filter(o => o.status === 'Paid')
  if (activeTab.value === 'shipped') return orders.value.filter(o => o.status === 'Shipped' || o.status === 'Delivered')
  return orders.value
})

async function fetchOrders() {
  loading.value = true
  try {
    const response = await orderApi.getAllOrders({
      pageIndex: currentPage.value - 1,
      pageSize: pageSize.value,
      filterQuery: searchQuery.value || undefined
    })
    
    if (response.data.code === 0) {
      orders.value = response.data.data || []
      totalItems.value = response.data.recordCount
      totalPages.value = Math.ceil(response.data.recordCount / pageSize.value)
    }
  } catch (error) {
    console.error('Failed to fetch orders:', error)
  } finally {
    loading.value = false
  }
}

async function handleShip(orderId: number) {
  try {
    const response = await orderApi.shipOrder(orderId)
    if (response.data.code === 0) {
      toastStore.success('订单已发货')
      fetchOrders()
    } else {
      toastStore.error(response.data.message)
    }
  } catch {
    toastStore.error('操作失败')
  }
}

function handleSearch() {
  currentPage.value = 1
  fetchOrders()
}

function formatItems(items: Order['items']) {
  if (items.length === 1) {
    return `${items[0].title} × ${items[0].quantity}`
  }
  return `${items[0].title} 等${items.length}件商品`
}

onMounted(() => {
  fetchOrders()
})
</script>

<template>
  <div class="space-y-6">
    <!-- 页面标题 -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold">订单管理</h1>
        <p class="text-muted-foreground">管理用户购买订单</p>
      </div>
      <div class="flex items-center gap-2 text-sm text-muted-foreground">
        <DollarSign class="h-4 w-4" />
        共 {{ totalItems }} 笔订单
      </div>
    </div>
    
    <!-- 标签页 -->
    <div class="flex gap-2">
      <Button
        v-for="tab in tabs"
        :key="tab.key"
        :variant="activeTab === tab.key ? 'default' : 'outline'"
        size="sm"
        @click="activeTab = tab.key as typeof activeTab"
      >
        {{ tab.label }}
      </Button>
    </div>
    
    <!-- 搜索栏 -->
    <Card>
      <CardContent class="p-4">
        <div class="flex gap-4">
          <div class="relative flex-1">
            <Search class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              v-model="searchQuery"
              placeholder="搜索订单号、用户名..."
              class="pl-10"
              @keyup.enter="handleSearch"
            />
          </div>
          <Button @click="handleSearch">搜索</Button>
        </div>
      </CardContent>
    </Card>
    
    <!-- 订单列表 -->
    <Card>
      <CardHeader>
        <CardTitle>订单列表</CardTitle>
      </CardHeader>
      <CardContent class="p-0">
        <DataTable
          :columns="columns"
          :data="filteredOrders"
          :loading="loading"
        >
          <template #cell-items="{ row }">
            <span class="text-sm">
              {{ formatItems((row as unknown as Order).items) }}
            </span>
          </template>
          
          <template #cell-totalAmount="{ value }">
            <span class="font-medium text-primary">
              ¥{{ (value as number).toFixed(2) }}
            </span>
          </template>
          
          <template #cell-status="{ row }">
            <Badge :variant="statusConfig[(row as unknown as Order).status]?.variant || 'secondary'">
              <component 
                :is="statusConfig[(row as unknown as Order).status]?.icon" 
                class="h-3 w-3 mr-1"
              />
              {{ statusConfig[(row as unknown as Order).status]?.label }}
            </Badge>
          </template>
          
          <template #actions="{ row }">
            <Button
              v-if="(row as unknown as Order).status === 'Paid'"
              size="sm"
              @click.stop="handleShip((row as unknown as Order).id)"
            >
              <Truck class="h-3 w-3 mr-1" />
              发货
            </Button>
          </template>
        </DataTable>
        
        <Pagination
          v-if="totalPages > 1"
          v-model:current-page="currentPage"
          :total-pages="totalPages"
          :total-items="totalItems"
          :page-size="pageSize"
          @update:current-page="fetchOrders"
        />
      </CardContent>
    </Card>
  </div>
</template>
