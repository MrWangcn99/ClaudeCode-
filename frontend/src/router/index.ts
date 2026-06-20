import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/Login.vue'),
    meta: { noAuth: true }
  },
  {
    path: '/',
    redirect: '/dashboard'
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('@/views/Dashboard.vue')
  },
  {
    path: '/products',
    name: 'ProductList',
    component: () => import('@/views/ProductList.vue')
  },
  {
    path: '/products/new',
    name: 'ProductCreate',
    component: () => import('@/views/ProductForm.vue')
  },
  {
    path: '/products/:id/edit',
    name: 'ProductEdit',
    component: () => import('@/views/ProductForm.vue'),
    props: true
  },
  {
    path: '/inventory',
    name: 'Inventory',
    component: () => import('@/views/InventoryManage.vue')
  },
  {
    path: '/orders',
    name: 'OrderList',
    component: () => import('@/views/OrderList.vue')
  },
  {
    path: '/orders/new',
    name: 'OrderCreate',
    component: () => import('@/views/OrderCreate.vue')
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// 路由守卫：未登录跳转到登录页
router.beforeEach((to, _from, next) => {
  const token = localStorage.getItem('token')

  if (to.meta.noAuth) {
    // 如果已登录且去登录页，重定向到首页
    if (token) {
      next('/dashboard')
    } else {
      next()
    }
  } else {
    // 需要登录的页面
    if (!token) {
      next('/login')
    } else {
      next()
    }
  }
})

export default router
