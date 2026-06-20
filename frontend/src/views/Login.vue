<template>
  <div class="login-container">
    <div class="login-bg-shapes">
      <div class="shape shape-1"></div>
      <div class="shape shape-2"></div>
      <div class="shape shape-3"></div>
      <div class="shape shape-4"></div>
    </div>

    <div class="login-card">
      <div class="login-header">
        <div class="login-icon">
          <el-icon :size="40"><ShoppingBag /></el-icon>
        </div>
        <h2>电商管理平台</h2>
        <p>E-Commerce Management Platform</p>
      </div>

      <el-form
        ref="formRef"
        :model="form"
        :rules="rules"
        class="login-form"
        @submit.prevent="handleLogin"
      >
        <el-form-item prop="username">
          <el-input
            v-model="form.username"
            placeholder="请输入账号"
            size="large"
            :prefix-icon="User"
            clearable
          />
        </el-form-item>

        <el-form-item prop="password">
          <el-input
            v-model="form.password"
            type="password"
            placeholder="请输入密码"
            size="large"
            :prefix-icon="Lock"
            show-password
            @keyup.enter="handleLogin"
          />
        </el-form-item>

        <el-form-item>
          <el-button
            type="primary"
            size="large"
            class="login-btn"
            :loading="loading"
            @click="handleLogin"
          >
            {{ loading ? '登录中...' : '登 录' }}
          </el-button>
        </el-form-item>
      </el-form>

      <div class="login-tip">
        <el-text type="info" size="small">演示账号：12345 / 密码：12345</el-text>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { User, Lock } from '@element-plus/icons-vue'
import { useAuthStore } from '@/stores/authStore'
import type { FormInstance, FormRules } from 'element-plus'

const router = useRouter()
const authStore = useAuthStore()

const formRef = ref<FormInstance>()
const loading = ref(false)

const form = reactive({
  username: '',
  password: ''
})

const rules: FormRules = {
  username: [{ required: true, message: '请输入账号', trigger: 'blur' }],
  password: [{ required: true, message: '请输入密码', trigger: 'blur' }]
}

async function handleLogin() {
  if (!formRef.value) return
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return

  loading.value = true
  const success = await authStore.login(form.username, form.password)
  loading.value = false

  if (success) {
    router.push('/products')
  }
}
</script>

<style scoped>
.login-container {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  position: relative;
  overflow: hidden;
}

/* 背景动画形状 */
.login-bg-shapes {
  position: absolute;
  inset: 0;
  pointer-events: none;
}

.shape {
  position: absolute;
  border-radius: 50%;
  opacity: 0.15;
  animation: float 20s infinite ease-in-out;
}

.shape-1 {
  width: 400px;
  height: 400px;
  background: #fff;
  top: -100px;
  left: -100px;
  animation-delay: 0s;
}

.shape-2 {
  width: 300px;
  height: 300px;
  background: #fff;
  bottom: -80px;
  right: -80px;
  animation-delay: -5s;
}

.shape-3 {
  width: 200px;
  height: 200px;
  background: rgba(255, 255, 255, 0.3);
  top: 50%;
  left: 60%;
  animation-delay: -10s;
}

.shape-4 {
  width: 150px;
  height: 150px;
  background: rgba(255, 255, 255, 0.2);
  top: 20%;
  right: 20%;
  animation-delay: -15s;
}

@keyframes float {
  0%, 100% { transform: translateY(0) scale(1); }
  25% { transform: translateY(-20px) scale(1.05); }
  50% { transform: translateY(10px) scale(0.95); }
  75% { transform: translateY(-10px) scale(1.02); }
}

/* 登录卡片 */
.login-card {
  width: 420px;
  background: rgba(255, 255, 255, 0.95);
  border-radius: 20px;
  padding: 48px 40px;
  box-shadow:
    0 20px 60px rgba(0, 0, 0, 0.15),
    0 0 120px rgba(102, 126, 234, 0.3);
  backdrop-filter: blur(10px);
  position: relative;
  z-index: 1;
  transform: translateY(0);
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.login-card:hover {
  transform: translateY(-4px);
  box-shadow:
    0 24px 70px rgba(0, 0, 0, 0.2),
    0 0 140px rgba(102, 126, 234, 0.4);
}

/* 头部 */
.login-header {
  text-align: center;
  margin-bottom: 36px;
}

.login-icon {
  width: 72px;
  height: 72px;
  margin: 0 auto 16px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  box-shadow: 0 8px 24px rgba(102, 126, 234, 0.4);
}

.login-header h2 {
  font-size: 26px;
  font-weight: 700;
  color: #303133;
  margin: 0 0 6px 0;
  letter-spacing: 2px;
}

.login-header p {
  font-size: 13px;
  color: #909399;
  margin: 0;
  letter-spacing: 1px;
}

/* 表单 */
.login-form {
  margin-top: 8px;
}

.login-form :deep(.el-input__wrapper) {
  border-radius: 10px;
  box-shadow: 0 0 0 1px #e4e7ed inset;
  transition: all 0.3s;
}

.login-form :deep(.el-input__wrapper:hover) {
  box-shadow: 0 0 0 1px #c0c4cc inset;
}

.login-form :deep(.el-input__wrapper.is-focus) {
  box-shadow: 0 0 0 2px rgba(102, 126, 234, 0.3) inset;
}

.login-form :deep(.el-form-item) {
  margin-bottom: 22px;
}

.login-btn {
  width: 100%;
  border-radius: 10px;
  font-size: 16px;
  font-weight: 600;
  letter-spacing: 4px;
  height: 46px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
  transition: all 0.3s;
}

.login-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 8px 24px rgba(102, 126, 234, 0.5);
}

.login-btn:active {
  transform: translateY(0);
}

.login-tip {
  text-align: center;
  margin-top: 12px;
}
</style>
