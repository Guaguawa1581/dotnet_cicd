# DotNetApiTest — Docker 部署指南

## 架構說明

```
客戶端
  │
  ▼ port 5088
[ Nginx 容器 ]  ──反向代理──▶  [ .NET API 容器 (port 8080) ]
                                       │
                                       ▼ 實體連線
                              [ MSSQL 你的DB位址 ]（不進 Docker）
```

---

## 專案結構

```
dotnet_cicd/
├── ApiServer/              # .NET 8 WebAPI
├── ApiServer.Tests/        # 測試專案（不打包進 Image）
├── nginx/
│   └── default.conf        # Nginx 反向代理設定
├── Dockerfile              # Multi-stage build
├── docker-compose.yml
├── .env.example            # 環境變數範本
└── README.md
```

---

## 首次安裝步驟

### 1. 準備環境變數

```bash
cp .env.example .env
```

編輯 `.env`，確認連線字串與 port 正確：

```
DB_CONNECTION_STRING=Server=你的DB位址; Database=資料庫名稱; User Id=帳號; Password=密碼; TrustServerCertificate=True;
APP_PORT=5088
```

### 2. 建置 Image 並啟動

```bash
docker compose build
docker compose up -d
```

### 3. 確認服務正常

```bash
docker compose ps
```

瀏覽器開啟：`http://localhost:5088/api/products`

---

## 日常操作

### 查看 Log

```bash
docker compose logs -f api
docker compose logs -f nginx
```

### 停止服務

```bash
docker compose down
```

### 重啟服務

```bash
docker compose restart
```

---

## 更新部署步驟（改程式碼後）

```bash
# 1. 重新建置 Image
docker compose build

# 2. 無停機重啟（先停舊容器、啟動新容器）
docker compose up -d --force-recreate
```

---

## 離線交付（匯出 .tar）

> 適用於完全離線內網環境，將 Image 打包後以 USB/網路共享傳送給客戶。

### 在開發機器上（有網路）

```bash
# 1. 建置 API Image
docker compose build

# 2. 拉取 Nginx Image（若尚未存在）
docker pull nginx:1.25-alpine

# 3. 匯出兩個 Image 為單一 .tar 檔
docker save dotnet-apiserver:latest nginx:1.25-alpine -o myapp.tar
```

### 在客戶機器上（離線環境）

```bash
# 1. 載入 Images
docker load -i myapp.tar

# 2. 複製專案資料夾（含 docker-compose.yml、nginx/、.env）到目標主機

# 3. 建立 .env（參考 .env.example）
cp .env.example .env
# 編輯 .env 填入正確的 DB 連線字串

# 4. 啟動服務（不需要 build，直接用載入的 Image）
docker compose up -d
```

---

## 環境變數說明

| 變數名稱 | 說明 | 範例 |
|---|---|---|
| `DB_CONNECTION_STRING` | MSSQL 連線字串 | `Data Source=192.168.100.87;...` |
| `APP_PORT` | 對外開放 port | `5088` |

> 在 .NET 中，`ConnectionStrings__DefaultConnection` 對應 `appsettings.json` 的 `ConnectionStrings:DefaultConnection`，Docker Compose 透過 `ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}` 注入。

---

## 注意事項

- `.env` 包含密碼，已加入 `.gitignore`，**不可提交至 git**
- `*.tar` 匯出檔已加入 `.gitignore`
- Swagger UI 僅在 `ASPNETCORE_ENVIRONMENT=Development` 時開放；如需在 Production 開啟，請修改 `Program.cs`
- HTTPS 由 Nginx 負責終止（如有需要），.NET API 容器僅監聽 HTTP
