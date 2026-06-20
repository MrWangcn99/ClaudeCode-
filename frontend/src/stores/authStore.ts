import { defineStore } from 'pinia'
import { ref } from 'vue'
import http from '@/api/http'
import { ElMessage } from 'element-plus'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('token') || '')
  const username = ref(localStorage.getItem('username') || '')
  const isLoggedIn = ref(!!token.value)

  async function login(user: string, pwd: string): Promise<boolean> {
    try {
      const { data } = await http.post('/auth/login', {
        username: user,
        password: pwd
      })
      token.value = data.token
      username.value = data.username
      isLoggedIn.value = true
      localStorage.setItem('token', data.token)
      localStorage.setItem('username', data.username)
      ElMessage.success(data.message || '登录成功')
      return true
    } catch (error: any) {
      const msg = error.response?.data?.error || '登录失败'
      ElMessage.error(msg)
      return false
    }
  }

  function logout() {
    token.value = ''
    username.value = ''
    isLoggedIn.value = false
    localStorage.removeItem('token')
    localStorage.removeItem('username')
    ElMessage.info('已退出登录')
  }

  return { token, username, isLoggedIn, login, logout }
})
