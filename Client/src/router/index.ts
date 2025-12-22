import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

// 读者端页面
const readerRoutes: RouteRecordRaw[] = [
  {
    path: '/',
    name: 'Home',
    component: () => import('@/views/reader/HomePage.vue'),
    meta: { title: '首页' }
  },
  {
    path: '/books',
    name: 'Books',
    component: () => import('@/views/reader/BooksPage.vue'),
    meta: { title: '图书搜索', requiresAuth: true }
  },
  {
    path: '/book/:id',
    name: 'BookDetail',
    component: () => import('@/views/reader/BookDetailPage.vue'),
    meta: { title: '图书详情', requiresAuth: true }
  },
  {
    path: '/profile',
    name: 'Profile',
    component: () => import('@/views/reader/ProfilePage.vue'),
    meta: { title: '个人中心', requiresAuth: true }
  },
  {
    path: '/my-borrows',
    name: 'MyBorrows',
    component: () => import('@/views/reader/MyBorrowsPage.vue'),
    meta: { title: '我的借阅', requiresAuth: true }
  },
  {
    path: '/history',
    name: 'History',
    component: () => import('@/views/reader/HistoryPage.vue'),
    meta: { title: '借阅历史', requiresAuth: true }
  },
  {
    path: '/recommend',
    name: 'Recommend',
    component: () => import('@/views/reader/RecommendPage.vue'),
    meta: { title: '荐购图书', requiresAuth: true }
  },
  {
    path: '/cart',
    name: 'Cart',
    component: () => import('@/views/reader/CartPage.vue'),
    meta: { title: '购物车' }
  },
  {
    path: '/orders',
    name: 'Orders',
    component: () => import('@/views/reader/OrdersPage.vue'),
    meta: { title: '我的订单', requiresAuth: true }
  },
]

// 管理端页面
const adminRoutes: RouteRecordRaw[] = [
  {
    path: '/admin',
    name: 'AdminLayout',
    component: () => import('@/layouts/AdminLayout.vue'),
    meta: { requiresAuth: true, requiresRole: 'Moderator' },
    children: [
      {
        path: '',
        name: 'AdminDefault',
        redirect: () => {
          const authStore = useAuthStore()
          // 管理员跳转到仪表盘，版主跳转到图书管理
          return authStore.hasRole('Admin') ? '/admin/dashboard' : '/admin/books'
        }
      },
      {
        path: 'dashboard',
        name: 'Dashboard',
        component: () => import('@/views/admin/DashboardPage.vue'),
        meta: { title: '仪表盘', requiresRole: 'Admin' }
      },
      {
        path: 'books',
        name: 'AdminBooks',
        component: () => import('@/views/admin/BooksManagePage.vue'),
        meta: { title: '图书管理' }
      },
      {
        path: 'borrows',
        name: 'AdminBorrows',
        component: () => import('@/views/admin/BorrowsManagePage.vue'),
        meta: { title: '借阅管理' }
      },
      {
        path: 'orders',
        name: 'AdminOrders',
        component: () => import('@/views/admin/OrdersManagePage.vue'),
        meta: { title: '订单管理' }
      },
      {
        path: 'users',
        name: 'AdminUsers',
        component: () => import('@/views/admin/UsersManagePage.vue'),
        meta: { title: '用户管理', requiresRole: 'Admin' }
      },
      {
        path: 'recommends',
        name: 'AdminRecommends',
        component: () => import('@/views/admin/RecommendsManagePage.vue'),
        meta: { title: '荐购管理' }
      },
      {
        path: 'logs',
        name: 'AdminLogs',
        component: () => import('@/views/admin/LogsPage.vue'),
        meta: { title: '系统日志', requiresRole: 'Admin' }
      },
    ]
  }
]

const routes: RouteRecordRaw[] = [
  ...readerRoutes,
  ...adminRoutes,
  {
    path: '/:pathMatch(.*)*',
    name: 'NotFound',
    component: () => import('@/views/NotFoundPage.vue'),
    meta: { title: '页面未找到' }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior(_to, _from, savedPosition) {
    if (savedPosition) {
      return savedPosition
    }
    return { top: 0 }
  }
})

// 路由守卫
router.beforeEach((to, _from, next) => {
  // 设置页面标题
  document.title = `${to.meta.title || '网上图书馆'} - 网上图书馆`
  
  const authStore = useAuthStore()
  
  // 检查是否需要登录
  if (to.meta.requiresAuth && !authStore.isLoggedIn) {
    authStore.openLoginModal()
    return next({ name: 'Home' })
  }
  
  // 检查角色权限
  if (to.meta.requiresRole) {
    const requiredRole = to.meta.requiresRole as string
    if (!authStore.hasRole(requiredRole)) {
      return next({ name: 'Home' })
    }
  }
  
  next()
})

export default router
