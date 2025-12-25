import { createRouter, createWebHashHistory } from 'vue-router'

import Greeting from '@/components/Greeting/Greeting.vue';
import User from '@/components/User/User.vue';
import Admin from '@/components/Admin/Admin.vue';
import AdminGreeting from '@/components/AdminGreeting/AdminGreeting.vue';

const router = createRouter({
  history: createWebHashHistory(),
  routes: [
    {
      path: '/',
      name: 'Greeting',
      component: Greeting,
      meta: {
        title: '登录',
      },
    },
    {
      path: '/admin-login',
      name: 'AdminGreeting',
      component: AdminGreeting,
      meta: {
        title: '管理员登录',
      },
    },
    {
      path: '/user',
      component: User,
      meta: {
        title: '用户页',
      }
    },
    {
      path: '/admin',
      component: Admin,
      meta: {
        title: '管理页',
      }
    },
  ]
})

// 添加全局前置守卫
router.beforeEach((to) => {
  // 如果路由配置中没有定义标题，则使用默认标题
  const destTitle = to.meta.title as string;
  document.title = destTitle || '网上图书馆';

  const token = localStorage.getItem('token');
  // 两个登录页面无需 token
  if (to.name !== 'Greeting' && to.name !== 'AdminGreeting' && token === null) {
    // 根据目标路由判断跳转到哪个登录页
    if (to.path.startsWith('/admin')) {
      return { name: 'AdminGreeting' };
    }
    return { name: 'Greeting' };
  }
});

export default router