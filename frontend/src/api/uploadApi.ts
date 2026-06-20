import axios from 'axios'

const uploadHttp = axios.create({
  baseURL: '/api',
  timeout: 30000,
  headers: { 'Content-Type': 'multipart/form-data' }
})

// 请求拦截器：自动附加 token
uploadHttp.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

export const uploadApi = {
  uploadImage: (file: File) => {
    const formData = new FormData()
    formData.append('file', file)
    return uploadHttp.post<{ url: string; fileName: string }>('/upload/image', formData)
  }
}
