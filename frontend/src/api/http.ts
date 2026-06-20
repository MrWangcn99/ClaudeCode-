import axios from 'axios'
import { ElMessage } from 'element-plus'

const http = axios.create({
  baseURL: '/api',
  timeout: 30000,
  headers: { 'Content-Type': 'application/json' }
})

// 请求拦截器：自动附加 token
http.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Response interceptor for error handling
http.interceptors.response.use(
  response => response,
  error => {
    const message = error.response?.data?.error || error.message || 'Network error'
    if (error.response?.status !== 404) {
      ElMessage.error(message)
    }
    return Promise.reject(error)
  }
)

export default http
