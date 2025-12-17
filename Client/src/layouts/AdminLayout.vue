<script setup lang="ts">
import { ref, computed } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import Button from '@/components/ui/Button.vue'
import Avatar from '@/components/ui/Avatar.vue'
import Badge from '@/components/ui/Badge.vue'
import { 
  LayoutDashboard, 
  BookOpen, 
  Users, 
  BookMarked,
  MessageSquare,
  FileText,
  ChevronLeft,
  LogOut,
  Home,
  Menu,
  X,
  ShoppingBag
} from 'lucide-vue-next'

const authStore = useAuthStore()
const route = useRoute()
const sidebarCollapsed = ref(false)
const mobileSidebarOpen = ref(false)

interface NavItem {
  name: string
  to: string
  icon: unknown
  badge?: number
  requiresRole?: string
}

const navItems = computed<NavItem[]>(() => [
  { name: '仪表盘', to: '/admin', icon: LayoutDashboard },
  { name: '图书管理', to: '/admin/books', icon: BookOpen },
  { name: '借阅管理', to: '/admin/borrows', icon: BookMarked },
  { name: '订单管理', to: '/admin/orders', icon: ShoppingBag },
  { name: '用户管理', to: '/admin/users', icon: Users, requiresRole: 'Admin' },
  { name: '荐购管理', to: '/admin/recommends', icon: MessageSquare },
  { name: '系统日志', to: '/admin/logs', icon: FileText, requiresRole: 'Admin' },
])

const filteredNavItems = computed(() => 
  navItems.value.filter(item => {
    if (!item.requiresRole) return true
    return authStore.hasRole(item.requiresRole)
  })
)

function toggleSidebar() {
  sidebarCollapsed.value = !sidebarCollapsed.value
}

function toggleMobileSidebar() {
  mobileSidebarOpen.value = !mobileSidebarOpen.value
}

function closeMobileSidebar() {
  mobileSidebarOpen.value = false
}

function isActive(path: string) {
  if (path === '/admin') {
    return route.path === '/admin'
  }
  return route.path.startsWith(path)
}
</script>

<template>
  <div class="min-h-screen bg-muted/30">
    <!-- 移动端顶部栏 -->
    <header class="lg:hidden sticky top-0 z-40 flex h-16 items-center gap-4 border-b bg-background px-4">
      <Button variant="ghost" size="icon" @click="toggleMobileSidebar">
        <Menu class="h-5 w-5" />
      </Button>
      <span class="font-semibold">管理后台</span>
    </header>
    
    <!-- 移动端侧边栏遮罩 -->
    <Transition
      enter-active-class="transition-opacity duration-200"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-opacity duration-200"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="mobileSidebarOpen"
        class="lg:hidden fixed inset-0 z-40 bg-black/50"
        @click="closeMobileSidebar"
      />
    </Transition>
    
    <!-- 侧边栏 -->
    <aside
      :class="[
        'fixed top-0 left-0 z-50 h-full bg-background border-r transition-all duration-300',
        sidebarCollapsed ? 'w-16' : 'w-64',
        mobileSidebarOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'
      ]"
    >
      <!-- 侧边栏头部 -->
      <div class="flex h-16 items-center justify-between px-4 border-b">
        <RouterLink
          v-if="!sidebarCollapsed"
          to="/admin"
          class="flex items-center gap-2 font-bold text-primary"
        >
          <LayoutDashboard class="h-5 w-5" />
          <span>管理后台</span>
        </RouterLink>
        <LayoutDashboard v-else class="h-5 w-5 text-primary mx-auto" />
        
        <!-- 折叠按钮（仅桌面端） -->
        <Button
          variant="ghost"
          size="icon"
          class="hidden lg:flex"
          @click="toggleSidebar"
        >
          <ChevronLeft :class="['h-4 w-4 transition-transform', sidebarCollapsed && 'rotate-180']" />
        </Button>
        
        <!-- 关闭按钮（仅移动端） -->
        <Button
          variant="ghost"
          size="icon"
          class="lg:hidden"
          @click="closeMobileSidebar"
        >
          <X class="h-4 w-4" />
        </Button>
      </div>
      
      <!-- 导航菜单 -->
      <nav class="flex-1 p-4 space-y-1">
        <RouterLink
          v-for="item in filteredNavItems"
          :key="item.to"
          :to="item.to"
          :class="[
            'flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors',
            isActive(item.to)
              ? 'bg-primary text-primary-foreground'
              : 'text-muted-foreground hover:bg-muted hover:text-foreground'
          ]"
          @click="closeMobileSidebar"
        >
          <component :is="item.icon" class="h-5 w-5 flex-shrink-0" />
          <span v-if="!sidebarCollapsed" class="flex-1">{{ item.name }}</span>
          <Badge v-if="item.badge && !sidebarCollapsed" variant="destructive" class="ml-auto">
            {{ item.badge }}
          </Badge>
        </RouterLink>
      </nav>
      
      <!-- 侧边栏底部 -->
      <div class="p-4 border-t space-y-2">
        <RouterLink
          to="/"
          :class="[
            'flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium text-muted-foreground hover:bg-muted hover:text-foreground transition-colors'
          ]"
        >
          <Home class="h-5 w-5 flex-shrink-0" />
          <span v-if="!sidebarCollapsed">返回前台</span>
        </RouterLink>
        
        <button
          :class="[
            'w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium text-muted-foreground hover:bg-muted hover:text-foreground transition-colors'
          ]"
          @click="authStore.logout"
        >
          <LogOut class="h-5 w-5 flex-shrink-0" />
          <span v-if="!sidebarCollapsed">退出登录</span>
        </button>
        
        <!-- 用户信息 -->
        <div v-if="!sidebarCollapsed" class="flex items-center gap-3 px-3 py-2">
          <Avatar 
            :src="authStore.user?.avatar" 
            :fallback="authStore.user?.userName || 'A'"
            size="sm"
          />
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium truncate">{{ authStore.user?.userName }}</p>
            <p class="text-xs text-muted-foreground truncate">
              {{ authStore.user?.role }}
            </p>
          </div>
        </div>
      </div>
    </aside>
    
    <!-- 主内容区 -->
    <main
      :class="[
        'min-h-screen transition-all duration-300',
        sidebarCollapsed ? 'lg:ml-16' : 'lg:ml-64'
      ]"
    >
      <div class="container p-6">
        <RouterView />
      </div>
    </main>
  </div>
</template>
