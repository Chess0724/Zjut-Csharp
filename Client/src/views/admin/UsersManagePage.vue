<script setup lang="ts">
import { ref, onMounted } from 'vue'
import Card from '@/components/ui/Card.vue'
import CardContent from '@/components/ui/CardContent.vue'
import DataTable from '@/components/ui/DataTable.vue'
import Pagination from '@/components/ui/Pagination.vue'
import Input from '@/components/ui/Input.vue'
import Button from '@/components/ui/Button.vue'
import Badge from '@/components/ui/Badge.vue'
import Avatar from '@/components/ui/Avatar.vue'
import { adminApi } from '@/api'
import { useToastStore } from '@/stores/toast'
import type { User } from '@/types'
import { Search, Trash2, Shield, ShieldCheck, ShieldAlert } from 'lucide-vue-next'

const toastStore = useToastStore()

const users = ref<User[]>([])
const loading = ref(true)
const currentPage = ref(1)
const totalPages = ref(1)
const totalItems = ref(0)
const pageSize = ref(15)
const searchQuery = ref('')
const sortColumn = ref('UserName')
const sortOrder = ref<'ASC' | 'DESC'>('ASC')

const columns = [
  { key: 'avatar', label: '', width: '50px' },
  { key: 'userName', label: '用户名', sortable: true },
  { key: 'email', label: '邮箱', sortable: true },
  { key: 'phone', label: '电话' },
  { key: 'roles', label: '角色' },
]

async function fetchUsers() {
  loading.value = true
  try {
    const response = await adminApi.getUsers({
      pageIndex: currentPage.value - 1,
      pageSize: pageSize.value,
      sortColumn: sortColumn.value,
      sortOrder: sortOrder.value,
      filterQuery: searchQuery.value || undefined
    })
    
    if (response.data.code === 0) {
      users.value = response.data.data || []
      totalItems.value = response.data.recordCount
      totalPages.value = Math.ceil(response.data.recordCount / pageSize.value)
    }
  } catch (error) {
    console.error('Failed to fetch users:', error)
  } finally {
    loading.value = false
  }
}

function handleSort(column: string) {
  if (sortColumn.value === column) {
    sortOrder.value = sortOrder.value === 'ASC' ? 'DESC' : 'ASC'
  } else {
    sortColumn.value = column
    sortOrder.value = 'ASC'
  }
  fetchUsers()
}

function handleSearch() {
  currentPage.value = 1
  fetchUsers()
}

async function handleDelete(user: User) {
  if (!confirm(`确定要删除用户 ${user.userName} 吗？此操作不可撤销。`)) {
    return
  }
  
  try {
    const response = await adminApi.removeUser(user.id)
    if (response.data.code === 0) {
      toastStore.success('用户删除成功')
      fetchUsers()
    } else {
      toastStore.error(response.data.message)
    }
  } catch {
    toastStore.error('删除失败')
  }
}

function getRoleBadgeVariant(role: string) {
  switch (role) {
    case 'Admin':
      return 'destructive'
    case 'Moderator':
      return 'default'
    default:
      return 'secondary'
  }
}

function getRoleIcon(role: string) {
  switch (role) {
    case 'Admin':
      return ShieldAlert
    case 'Moderator':
      return ShieldCheck
    default:
      return Shield
  }
}

onMounted(() => {
  fetchUsers()
})
</script>

<template>
  <div class="space-y-6">
    <!-- 页面标题 -->
    <div>
      <h1 class="text-2xl font-bold">用户管理</h1>
      <p class="text-muted-foreground">管理系统用户</p>
    </div>
    
    <!-- 搜索栏 -->
    <Card>
      <CardContent class="p-4">
        <div class="flex gap-4">
          <div class="relative flex-1">
            <Search class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              v-model="searchQuery"
              placeholder="搜索用户名或邮箱..."
              class="pl-10"
              @keyup.enter="handleSearch"
            />
          </div>
          <Button @click="handleSearch">搜索</Button>
        </div>
      </CardContent>
    </Card>
    
    <!-- 数据表格 -->
    <Card>
      <CardContent class="p-0">
        <DataTable
          :columns="columns"
          :data="users"
          :loading="loading"
          :sort-column="sortColumn"
          :sort-order="sortOrder"
          @sort="handleSort"
        >
          <template #cell-avatar="{ row }">
            <Avatar 
              :src="(row as User).avatar" 
              :fallback="(row as User).userName"
              size="sm"
            />
          </template>
          
          <template #cell-roles="{ value }">
            <div class="flex flex-wrap gap-1">
              <Badge 
                v-for="role in (value as string[])" 
                :key="role"
                :variant="getRoleBadgeVariant(role)"
                class="gap-1"
              >
                <component :is="getRoleIcon(role)" class="h-3 w-3" />
                {{ role }}
              </Badge>
            </div>
          </template>
          
          <template #actions="{ row }">
            <Button
              v-if="!(row as User).roles?.includes('Admin')"
              variant="ghost"
              size="icon"
              class="text-destructive hover:text-destructive"
              @click.stop="handleDelete(row as User)"
            >
              <Trash2 class="h-4 w-4" />
            </Button>
          </template>
        </DataTable>
        
        <Pagination
          v-if="totalPages > 1"
          v-model:current-page="currentPage"
          :total-pages="totalPages"
          :total-items="totalItems"
          :page-size="pageSize"
          @update:current-page="fetchUsers"
        />
      </CardContent>
    </Card>
  </div>
</template>
