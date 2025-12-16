<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import Button from '@/components/ui/Button.vue'
import Avatar from '@/components/ui/Avatar.vue'
import LoginModal from '@/components/auth/LoginModal.vue'
import { 
  BookOpen, 
  User, 
  History, 
  Heart, 
  LogOut,
  LayoutDashboard,
  Menu,
  X
} from 'lucide-vue-next'
import { ref } from 'vue'

const authStore = useAuthStore()
const route = useRoute()
const mobileMenuOpen = ref(false)

const navItems = computed(() => [
  { name: '图书搜索', to: '/books', icon: BookOpen, requiresAuth: true },
  { name: '我的借阅', to: '/my-borrows', icon: User, requiresAuth: true },
  { name: '借阅历史', to: '/history', icon: History, requiresAuth: true },
  { name: '荐购图书', to: '/recommend', icon: Heart, requiresAuth: true },
])

function toggleMobileMenu() {
  mobileMenuOpen.value = !mobileMenuOpen.value
}

function closeMobileMenu() {
  mobileMenuOpen.value = false
}
</script>

<template>
  <div class="min-h-screen bg-background">
    <!-- 顶部导航栏 -->
    <header class="sticky top-0 z-40 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div class="container flex h-16 items-center justify-between px-4">
        <!-- Logo -->
        <RouterLink to="/" class="flex items-center gap-2 text-xl font-bold text-primary">
          <BookOpen class="h-6 w-6" />
          <span class="hidden sm:inline">网上图书馆</span>
        </RouterLink>
        
        <!-- 桌面端导航 -->
        <nav class="hidden md:flex items-center gap-6">
          <RouterLink
            v-for="item in navItems"
            :key="item.to"
            :to="item.to"
            :class="[
              'flex items-center gap-2 text-sm font-medium transition-colors hover:text-primary',
              route.path === item.to ? 'text-primary' : 'text-muted-foreground'
            ]"
          >
            <component :is="item.icon" class="h-4 w-4" />
            {{ item.name }}
          </RouterLink>
        </nav>
        
        <!-- 用户区域 -->
        <div class="flex items-center gap-4">
          <template v-if="authStore.isLoggedIn">
            <!-- 管理端入口 -->
            <RouterLink
              v-if="authStore.isModerator"
              to="/admin"
              class="hidden sm:flex items-center gap-2 text-sm font-medium text-muted-foreground hover:text-primary transition-colors"
            >
              <LayoutDashboard class="h-4 w-4" />
              管理后台
            </RouterLink>
            
            <!-- 用户头像下拉 -->
            <RouterLink to="/profile" class="flex items-center gap-2">
              <Avatar 
                :src="authStore.user?.avatar" 
                :fallback="authStore.user?.userName || 'U'"
                size="sm"
              />
              <span class="hidden sm:inline text-sm font-medium">
                {{ authStore.user?.userName }}
              </span>
            </RouterLink>
            
            <Button variant="ghost" size="icon" @click="authStore.logout">
              <LogOut class="h-4 w-4" />
            </Button>
          </template>
          
          <template v-else>
            <Button @click="authStore.openLoginModal">
              登录
            </Button>
          </template>
          
          <!-- 移动端菜单按钮 -->
          <Button 
            variant="ghost" 
            size="icon" 
            class="md:hidden"
            @click="toggleMobileMenu"
          >
            <Menu v-if="!mobileMenuOpen" class="h-5 w-5" />
            <X v-else class="h-5 w-5" />
          </Button>
        </div>
      </div>
      
      <!-- 移动端菜单 -->
      <Transition
        enter-active-class="transition-all duration-200 ease-out"
        enter-from-class="opacity-0 -translate-y-2"
        enter-to-class="opacity-100 translate-y-0"
        leave-active-class="transition-all duration-150 ease-in"
        leave-from-class="opacity-100 translate-y-0"
        leave-to-class="opacity-0 -translate-y-2"
      >
        <div
          v-if="mobileMenuOpen"
          class="md:hidden border-t bg-background"
        >
          <nav class="container px-4 py-4 space-y-2">
            <RouterLink
              v-for="item in navItems"
              :key="item.to"
              :to="item.to"
              :class="[
                'flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium transition-colors',
                route.path === item.to 
                  ? 'bg-primary text-primary-foreground' 
                  : 'hover:bg-muted'
              ]"
              @click="closeMobileMenu"
            >
              <component :is="item.icon" class="h-5 w-5" />
              {{ item.name }}
            </RouterLink>
            
            <RouterLink
              v-if="authStore.isModerator"
              to="/admin"
              class="flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium hover:bg-muted"
              @click="closeMobileMenu"
            >
              <LayoutDashboard class="h-5 w-5" />
              管理后台
            </RouterLink>
          </nav>
        </div>
      </Transition>
    </header>
    
    <!-- 主内容区 -->
    <main>
      <slot />
    </main>
    
    <!-- 页脚 -->
    <footer class="border-t bg-muted/30">
      <div class="container px-4 py-8">
        <div class="flex flex-col md:flex-row items-center justify-between gap-4 text-sm text-muted-foreground">
          <div class="flex items-center gap-2">
            <BookOpen class="h-5 w-5" />
            <span>网上图书馆 © 2024</span>
          </div>
          <div class="flex items-center gap-6">
            <a href="#" class="hover:text-foreground transition-colors">关于我们</a>
            <a href="#" class="hover:text-foreground transition-colors">使用条款</a>
            <a href="#" class="hover:text-foreground transition-colors">隐私政策</a>
          </div>
        </div>
      </div>
    </footer>
    
    <!-- 登录模态框 -->
    <LoginModal />
  </div>
</template>
