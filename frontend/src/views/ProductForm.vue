<template>
  <div class="page-container">
    <!-- 返回按钮 -->
    <div class="back-bar">
      <el-button @click="$router.push('/products')" text>
        <el-icon><ArrowLeft /></el-icon>
        返回商品列表
      </el-button>
    </div>

    <!-- 表单卡片 -->
    <el-card class="form-card" shadow="never">
      <template #header>
        <div class="form-header">
          <el-icon :size="22" class="form-icon"><Goods /></el-icon>
          <span>{{ isEdit ? '编辑商品' : '添加新商品' }}</span>
        </div>
      </template>

      <el-form
        ref="formRef"
        :model="form"
        :rules="rules"
        label-width="100px"
        label-position="right"
        class="product-form"
        @submit.prevent="handleSubmit"
      >
        <el-row :gutter="24">
          <el-col :span="12">
            <el-form-item label="商品名称" prop="name">
              <el-input
                v-model="form.name"
                placeholder="请输入商品名称"
                maxlength="200"
                show-word-limit
                size="large"
              />
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="价格" prop="price">
              <el-input-number
                v-model="form.price"
                :min="0.01"
                :precision="2"
                :step="1"
                placeholder="0.00"
                size="large"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="库存数量" prop="stock">
              <el-input-number
                v-model="form.stock"
                :min="0"
                :step="1"
                placeholder="0"
                size="large"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item label="商品描述" prop="description">
          <el-input
            v-model="form.description"
            type="textarea"
            :rows="4"
            placeholder="请输入商品描述信息"
            maxlength="2000"
            show-word-limit
          />
        </el-form-item>

        <el-form-item label="商品图片">
          <div style="display: flex; flex-direction: column; gap: 12px; width: 100%">
            <div style="display: flex; align-items: center; gap: 12px">
              <el-upload
                :auto-upload="false"
                :show-file-list="false"
                accept=".jpg,.jpeg"
                :on-change="handleFileChange"
              >
                <el-button type="primary" :loading="uploading">
                  <el-icon><Upload /></el-icon>
                  本地上传 JPG
                </el-button>
              </el-upload>
              <el-text type="info" size="small">仅支持 JPG 格式，不超过 10MB</el-text>
            </div>
            <el-input
              v-model="form.imageUrl"
              placeholder="或直接输入图片链接地址"
              maxlength="500"
              clearable
            />
          </div>
          <div v-if="form.imageUrl" class="image-preview">
            <el-image
              :src="form.imageUrl"
              style="width: 140px; height: 140px; border-radius: 12px"
              fit="cover"
            >
              <template #error>
                <div class="img-error">
                  <el-icon :size="32"><Picture /></el-icon>
                  <span>图片链接无效</span>
                </div>
              </template>
            </el-image>
          </div>
        </el-form-item>

        <el-divider />

        <el-form-item>
          <el-button type="primary" size="large" round @click="handleSubmit" :loading="submitting">
            <el-icon><Check /></el-icon>
            {{ isEdit ? '保存修改' : '立即创建' }}
          </el-button>
          <el-button size="large" round @click="$router.push('/products')">取消</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useProductStore } from '@/stores/productStore'
import type { FormInstance, FormRules, UploadFile } from 'element-plus'
import { ElMessage } from 'element-plus'
import { uploadApi } from '@/api/uploadApi'

const route = useRoute()
const router = useRouter()
const store = useProductStore()

const isEdit = computed(() => !!route.params.id)
const productId = computed(() => Number(route.params.id))

const formRef = ref<FormInstance>()
const submitting = ref(false)
const uploading = ref(false)

async function handleFileChange(file: UploadFile) {
  const raw = file.raw
  if (!raw) return

  // 校验格式
  const ext = raw.name.split('.').pop()?.toLowerCase()
  if (ext !== 'jpg' && ext !== 'jpeg') {
    ElMessage.error('仅支持 JPG 格式的图片')
    return
  }

  // 校验大小
  if (raw.size > 10 * 1024 * 1024) {
    ElMessage.error('图片大小不能超过 10MB')
    return
  }

  uploading.value = true
  try {
    const { data } = await uploadApi.uploadImage(raw)
    form.value.imageUrl = data.url
    ElMessage.success('图片上传成功')
  } catch (error: any) {
    const msg = error.response?.data?.error || '图片上传失败'
    ElMessage.error(msg)
  } finally {
    uploading.value = false
  }
}

const form = ref({
  name: '',
  price: 0,
  stock: 0,
  description: '',
  imageUrl: ''
})

const rules: FormRules = {
  name: [{ required: true, message: '请输入商品名称', trigger: 'blur' }],
  price: [{ required: true, message: '请输入价格', trigger: 'blur' }],
  stock: [{ required: true, message: '请输入库存数量', trigger: 'blur' }]
}

onMounted(async () => {
  if (isEdit.value) {
    try {
      const { data } = await import('@/api/productApi').then(m => m.productApi.getById(productId.value))
      form.value = {
        name: data.name,
        price: data.price,
        stock: data.stock,
        description: data.description || '',
        imageUrl: data.imageUrl || ''
      }
    } catch {
      ElMessage.error('加载商品信息失败')
      router.push('/products')
    }
  }
})

async function handleSubmit() {
  if (!formRef.value) return

  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return

  submitting.value = true
  try {
    if (isEdit.value) {
      await store.update(productId.value, form.value)
      ElMessage.success('商品信息已更新')
    } else {
      await store.create(form.value)
      ElMessage.success('商品创建成功')
    }
    router.push('/products')
  } catch {
    ElMessage.error(isEdit.value ? '更新商品失败' : '创建商品失败')
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
.page-container {
  max-width: 900px;
  margin: 0 auto;
}

.back-bar {
  margin-bottom: 16px;
}

.form-card {
  border-radius: 12px;
  border: 1px solid #f0f0f0;
}

.form-header {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 17px;
  font-weight: 600;
  color: #1d2129;
}

.form-icon {
  color: #1890ff;
}

.product-form {
  max-width: 800px;
}

.product-form :deep(.el-input-number) {
  width: 100%;
}

.image-preview {
  margin-top: 12px;
}

.img-error {
  width: 140px;
  height: 140px;
  border-radius: 12px;
  background: #f5f7fa;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  color: #c0c4cc;
  font-size: 12px;
}
</style>
