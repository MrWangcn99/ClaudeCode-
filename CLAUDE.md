# CLAUDE.md

本文件为 Claude Code (claude.ai/code) 在此仓库中工作时提供指引。
每次输出前都带一下这几个字“你好帅哥”

## 项目概览

全栈电商管理平台 — 商品增删改查、库存管理、订单管理（含并发安全的库存扣减）、仪表盘数据分析和图片上传。Vue 3 前端 + ASP.NET Core 8 后端 + MySQL 数据库。

## 常用命令

**后端** (`backend/ECommerceApi/`):
```bash
cd backend/ECommerceApi
dotnet restore          # 还原 NuGet 包
dotnet build            # 编译项目
dotnet run              # 启动，监听 http://localhost:5000，Swagger 文档：/swagger
```

**前端** (`frontend/`):
```bash
cd frontend
npm install             # 安装依赖
npm run dev             # 开发服务器 http://localhost:5173（/api 和 /uploads 代理到 :5000）
npm run build           # 生产构建（vue-tsc 类型检查 + vite build）
```

**MySQL**（本地 Windows 服务，MySQL 5.5）:
```bash
mysql -u root -p12345 -e "USE ecommerce; SHOW TABLES;"
```

## 架构设计

### 后端：分层 IOC 架构

```
Controller → Service（接口） → Repository（接口） → AppDbContext → MySQL
                  ↑
         StockConcurrencyManager（Singleton，横切关注点）
```

- **Controllers** — 薄层，委托给 Service，返回 `ActionResult<T>`
- **Services** — 业务逻辑、校验、编排。依赖 Repository 接口，通过构造函数注入
- **Repositories** — 纯数据访问，接收/返回 Model 实体。依赖 `AppDbContext`
- **IOC** — 全部在 `Program.cs` 中通过 `AddScoped`/`AddSingleton` 注册，不使用 Service Locator 模式

### 前端：Vue 3 组合式 API + Pinia

```
View（Vue 单文件组件） → Pinia Store → API 模块（axios） → 后端
```

- **Views** — 页面级组件位于 `src/views/`，通过路由懒加载
- **Stores** — Pinia `defineStore`，使用 setup 函数语法（`src/stores/`）
- **API** — axios 实例，baseURL 为 `/api`，请求拦截器自动附加 token（`src/api/http.ts`）
- **Router** — 路由守卫检查 `localStorage.getItem('token')`，未登录跳转到 `/login`

### 并发控制（三层防超卖机制）

仅作用于 `POST /api/orders`，在 `OrderService.CreateOrderAsync` 中实现：

1. **逐商品 `SemaphoreSlim`** — `StockConcurrencyManager`（Singleton）持有 `ConcurrentDictionary<int, SemaphoreSlim>`。每个商品 ID 对应一个信号量（互斥锁）。获取锁前按商品 ID 排序，防止死锁。超时时间 30 秒。
2. **数据库事务** — `BeginTransactionAsync()` / `CommitAsync()` / `RollbackAsync()` 确保库存扣减和订单创建原子执行。
3. **锁内库存检查** — `product.Stock < quantity` 判断在持有信号量期间执行，其他线程无法在检查和扣减之间插入。

**关键约束：** `StockConcurrencyManager` 必须注册为 **Singleton**。如果改为 Scoped，每个 HTTP 请求会获得独立的锁字典，防超卖机制彻底失效。

### 登录认证

简单的硬编码登录：`POST /api/auth/login`，请求体 `{username, password}`。正确凭据：`12345 / 12345`。返回固定 token `admin-token-2024`。前端将 token 存入 `localStorage`，axios 拦截器自动附加 `Authorization: Bearer <token>`。路由守卫在 `router/index.ts` 中，无 token 时跳转到 `/login`。

### 数据库

MySQL 数据库 `ecommerce`，首次启动时通过 `EnsureCreatedAsync()` 自动创建。三张表：`Products`、`Orders`、`OrderItems`。EF Core Fluent API 配置在 `AppDbContext.OnModelCreating` 中。种子数据：5 个中文名称的演示商品。连接字符串在 `appsettings.json` 中：`server=localhost;port=3306;database=ecommerce;user=root;password=12345`。

### 图片上传

`POST /api/upload/image` 仅接受 `.jpg/.jpeg` 格式（最大 10MB）。文件保存到 `wwwroot/uploads/`，通过 `UseStaticFiles()` 作为静态文件对外提供访问。前端 `ProductForm.vue` 同时提供上传按钮和 URL 输入两种方式。Vite 开发服务器将 `/uploads` 路径代理到后端。

### 核心文件

| 文件 | 用途 |
|------|------|
| `Program.cs` | 启动配置：DI 注册、数据库初始化、中间件管道、CORS、并发冒烟测试 |
| `Services/StockConcurrencyManager.cs` | 逐商品锁字典（Singleton），防超卖核心 |
| `Services/OrderService.cs` | 订单创建：加锁 + 事务 + 库存扣减 |
| `Data/AppDbContext.cs` | EF Core 模型配置、种子数据 |
| `frontend/src/router/index.ts` | 路由定义 + 登录守卫 |
| `frontend/src/stores/orderStore.ts` | 购物车状态 + 下单逻辑 |
| `frontend/src/api/http.ts` | axios 实例 + token 拦截器 |

### 关键开发注意事项

- **async 代码中禁止使用 `lock` 语句** — 项目统一使用 `SemaphoreSlim.WaitAsync()` 实现异步互斥。
- **前端图片展示：** 商品列表中优先使用原生 `<img>` 标签而非 `<el-image>`，避免 Element Plus 在相对路径下的兼容性问题。
- **`el-input-number`：** 绝对不能出现 `min > max` 的情况（例如库存为 0 时 `min=1 max=0`）。必须用 `v-if="stock > 0"` 包裹。
- **页面 `onMounted`：** 务必 `await` 异步数据加载函数，等数据到之后再访问 store 中的状态。
- **Vite 代理：** `/api` 和 `/uploads` 两个路径都需要在 `vite.config.ts` 中代理到 `http://localhost:5000`。
- **MySQL 中文存储：** 数据库必须使用 `utf8mb4` 字符集创建（`CREATE DATABASE ecommerce CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci`），否则插入中文会报 "Incorrect string value" 错误。
