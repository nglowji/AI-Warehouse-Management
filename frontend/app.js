const API_BASE = 'http://localhost:5076/api';

const fallback = {
  summary: {
    totalProducts: 6,
    totalWarehouses: 2,
    lowStockProducts: 3,
    pendingStocktakes: 1,
    totalInventoryQuantity: 1394,
    incomingThisMonth: 168,
    outgoingThisMonth: 86,
    netMovementThisMonth: 82
  },
  lowStock: [
    { productName: 'Xi măng PCB40', sku: 'XM-PCB40-001', warehouseName: 'Kho vật liệu Bình Dương', locationName: 'Bãi A01', availableQuantity: 42, minimumStock: 80, shortfall: 38 },
    { productName: 'Thép xây dựng D16', sku: 'THEP-D16-016', warehouseName: 'Kho vật liệu Bình Dương', locationName: 'Kệ thép B02', availableQuantity: 18, minimumStock: 50, shortfall: 32 },
    { productName: 'Sơn ngoại thất trắng 18L', sku: 'SON-NGOAI-018', warehouseName: 'Kho hoàn thiện Thủ Đức', locationName: 'Kệ sơn C03', availableQuantity: 9, minimumStock: 24, shortfall: 15 }
  ],
  inventory: [
    { name: 'Xi măng PCB40', sku: 'XM-PCB40-001', warehouse: 'Kho vật liệu Bình Dương', location: 'Bãi A01', quantity: 42, reserved: 6, status: 'Sắp hết' },
    { name: 'Gạch block ống 4 lỗ', sku: 'GACH-4LO-004', warehouse: 'Kho vật liệu Bình Dương', location: 'Bãi B01', quantity: 680, reserved: 120, status: 'Ổn định' },
    { name: 'Cát xây dựng loại 1', sku: 'CAT-XD-001', warehouse: 'Kho vật liệu Bình Dương', location: 'Bãi C01', quantity: 235, reserved: 40, status: 'Ổn định' },
    { name: 'Thép xây dựng D16', sku: 'THEP-D16-016', warehouse: 'Kho vật liệu Bình Dương', location: 'Kệ thép B02', quantity: 18, reserved: 4, status: 'Sắp hết' },
    { name: 'Đá 1x2 xây dựng', sku: 'DA-1X2-002', warehouse: 'Kho vật liệu Bình Dương', location: 'Bãi C02', quantity: 410, reserved: 80, status: 'Ổn định' },
    { name: 'Sơn ngoại thất trắng 18L', sku: 'SON-NGOAI-018', warehouse: 'Kho hoàn thiện Thủ Đức', location: 'Kệ sơn C03', quantity: 9, reserved: 2, status: 'Sắp hết' }
  ],
  receipts: [
    { code: 'PN-202609-001', supplier: 'Công ty VLXD Minh Phát', warehouse: 'Kho vật liệu Bình Dương', quantity: 120, status: 'Đã xác nhận', date: '01/09/2026' },
    { code: 'PN-202609-002', supplier: 'Nhà phân phối Sơn Việt', warehouse: 'Kho hoàn thiện Thủ Đức', quantity: 48, status: 'Nháp', date: '04/09/2026' }
  ],
  issues: [
    { code: 'PX-202609-001', warehouse: 'Kho vật liệu Bình Dương', quantity: 86, status: 'Đã xác nhận', date: '03/09/2026', receiver: 'Công trình nhà phố An Phú' },
    { code: 'PX-202609-002', warehouse: 'Kho hoàn thiện Thủ Đức', quantity: 24, status: 'Chờ duyệt', date: '05/09/2026', receiver: 'Đội thi công hoàn thiện' }
  ],
  stocktakes: [
    { code: 'KK-202609-001', warehouse: 'Kho vật liệu Bình Dương', status: 'Nháp', items: 6, owner: 'Nguyễn Minh Quản' },
    { code: 'KK-202608-004', warehouse: 'Kho hoàn thiện Thủ Đức', status: 'Đã đóng', items: 24, owner: 'Trần Hoàng Nam' }
  ],
  audit: [
    { time: '09:10', action: 'CONFIRM', module: 'Nhập kho', detail: 'Xác nhận phiếu PN-202609-001' },
    { time: '10:25', action: 'CREATE', module: 'Kiểm kê', detail: 'Tạo phiên kiểm kê KK-202609-001' },
    { time: '14:40', action: 'ISSUE', module: 'Xuất kho', detail: 'Xuất 80 bao xi măng PCB40' }
  ]
};

const pages = {
  overview: { title: 'Tổng quan kho', eyebrow: 'Vận hành hôm nay' },
  inventory: { title: 'Tồn kho', eyebrow: 'Theo dõi số lượng và vị trí' },
  receipts: { title: 'Nhập kho', eyebrow: 'Phiếu nhập và nhà cung cấp' },
  issues: { title: 'Xuất kho', eyebrow: 'Yêu cầu xuất và giao hàng' },
  stocktakes: { title: 'Kiểm kê', eyebrow: 'Đối soát số liệu thực tế' },
  audit: { title: 'Nhật ký hoạt động', eyebrow: 'Lịch sử thao tác hệ thống' },
  assistant: { title: 'Trợ lý AI', eyebrow: 'Tra cứu nhanh dữ liệu kho' }
};

let state = { currentPage: 'overview', summary: fallback.summary, lowStock: fallback.lowStock, online: false };

const pageContent = document.getElementById('pageContent');
const pageTitle = document.getElementById('pageTitle');
const pageEyebrow = document.getElementById('pageEyebrow');
const apiStatus = document.getElementById('apiStatus');
const refreshBtn = document.getElementById('refreshBtn');
const navItems = [...document.querySelectorAll('.nav-item')];

async function fetchJson(url) {
  const response = await fetch(url, { headers: { Accept: 'application/json' } });
  if (!response.ok) throw new Error(`Request failed: ${response.status}`);
  return response.json();
}

function formatNumber(value) {
  return Number(value || 0).toLocaleString('vi-VN', { maximumFractionDigits: 2 });
}

function badge(text, tone = 'muted') {
  return `<span class="status-badge ${tone}">${text}</span>`;
}

function toneByStatus(status) {
  if (['Đã xác nhận', 'Đã đóng', 'Ổn định'].includes(status)) return 'success';
  if (['Sắp hết', 'Chờ duyệt'].includes(status)) return 'warning';
  if (['Nháp'].includes(status)) return 'muted';
  return 'danger';
}

function setApiStatus() {
  apiStatus.textContent = state.online ? 'API sẵn sàng' : 'Dữ liệu mẫu';
  apiStatus.className = `status-badge ${state.online ? 'success' : 'warning'}`;
}

async function loadDashboardData() {
  refreshBtn.disabled = true;
  refreshBtn.textContent = 'Đang tải...';

  try {
    const [summary, lowStock] = await Promise.all([
      fetchJson(`${API_BASE}/dashboard/summary`),
      fetchJson(`${API_BASE}/dashboard/low-stock`)
    ]);
    const apiLowStock = Array.isArray(lowStock) ? lowStock : lowStock.value || [];
    const isConstructionData = apiLowStock.some(item => /xi măng|thép|gạch|cát|sơn|đá/i.test(item.productName || ''));
    state = {
      ...state,
      summary: isConstructionData ? summary : fallback.summary,
      lowStock: isConstructionData ? apiLowStock : fallback.lowStock,
      online: true
    };
  } catch (error) {
    state = { ...state, summary: fallback.summary, lowStock: fallback.lowStock, online: false };
    console.error(error);
  } finally {
    refreshBtn.disabled = false;
    refreshBtn.textContent = 'Làm mới';
    setApiStatus();
    renderPage(state.currentPage);
  }
}

function statCard(label, value, tone = '') {
  return `<article class="stat-card ${tone}"><p>${label}</p><h3>${formatNumber(value)}</h3></article>`;
}

function table(headers, rows, emptyText = 'Chưa có dữ liệu') {
  return `
    <div class="table-wrap">
      <table>
        <thead><tr>${headers.map(header => `<th>${header}</th>`).join('')}</tr></thead>
        <tbody>${rows.length ? rows.join('') : `<tr><td colspan="${headers.length}" class="empty-cell">${emptyText}</td></tr>`}</tbody>
      </table>
    </div>`;
}

function renderOverview() {
  const s = state.summary;
  const netTone = Number(s.netMovementThisMonth) >= 0 ? 'success' : 'danger';
  const lowStockRows = state.lowStock.map(item => `
    <tr>
      <td><strong>${item.productName}</strong><span>${item.sku}</span></td>
      <td>${item.warehouseName}</td>
      <td>${item.locationName}</td>
      <td>${formatNumber(item.availableQuantity)}</td>
      <td>${formatNumber(item.minimumStock)}</td>
      <td>${badge(formatNumber(item.shortfall), 'danger')}</td>
    </tr>`);

  return `
    <section class="stats-grid">
      ${statCard('Tổng sản phẩm', s.totalProducts)}
      ${statCard('Tổng kho', s.totalWarehouses)}
      ${statCard('Sản phẩm sắp hết', s.lowStockProducts, 'warning')}
      ${statCard('Kiểm kê chờ', s.pendingStocktakes, 'accent')}
    </section>
    <section class="summary-row">
      <article class="panel">
        <div class="panel-header split"><div><p class="eyebrow">Tồn kho</p><h3>Tình trạng hiện tại</h3></div>${badge(`${Number(s.netMovementThisMonth) >= 0 ? '+' : ''}${formatNumber(s.netMovementThisMonth)}`, netTone)}</div>
        <div class="metric-row"><div><span>Tổng lượng khả dụng</span><strong>${formatNumber(s.totalInventoryQuantity)}</strong></div><div><span>Nhập trong tháng</span><strong>${formatNumber(s.incomingThisMonth)}</strong></div></div>
        <div class="metric-row"><div><span>Xuất trong tháng</span><strong>${formatNumber(s.outgoingThisMonth)}</strong></div><div><span>Dịch chuyển ròng</span><strong>${formatNumber(s.netMovementThisMonth)}</strong></div></div>
      </article>
      <article class="panel action-panel">
        <div class="panel-header"><p class="eyebrow">Thao tác nhanh</p><h3>Luồng nghiệp vụ</h3></div>
        <div class="action-list">
          <button type="button" data-go="receipts">Tạo phiếu nhập</button>
          <button type="button" data-go="issues">Tạo phiếu xuất</button>
          <button type="button" data-go="stocktakes">Mở kiểm kê</button>
          <button type="button" data-go="assistant">Hỏi trợ lý AI</button>
        </div>
      </article>
    </section>
    <section class="panel table-panel">
      <div class="panel-header split"><div><p class="eyebrow">Cảnh báo</p><h3>Danh sách sắp hết hàng</h3></div><span class="table-meta">${state.lowStock.length} mặt hàng</span></div>
      ${table(['Sản phẩm', 'Kho', 'Vị trí', 'Khả dụng', 'Tối thiểu', 'Thiếu hụt'], lowStockRows, 'Tất cả sản phẩm đang trên mức tối thiểu.')}
    </section>`;
}

function renderInventory() {
  const rows = fallback.inventory.map(item => `
    <tr><td><strong>${item.name}</strong><span>${item.sku}</span></td><td>${item.warehouse}</td><td>${item.location}</td><td>${formatNumber(item.quantity)}</td><td>${formatNumber(item.reserved)}</td><td>${badge(item.status, toneByStatus(item.status))}</td></tr>`);
  return `<section class="panel table-panel"><div class="panel-header split"><div><p class="eyebrow">Tồn kho</p><h3>Danh mục đang lưu kho</h3></div><span class="table-meta">${fallback.inventory.length} dòng tồn</span></div>${table(['Sản phẩm', 'Kho', 'Vị trí', 'Số lượng', 'Đã giữ', 'Trạng thái'], rows)}</section>`;
}

function renderReceipts() {
  const rows = fallback.receipts.map(item => `<tr><td><strong>${item.code}</strong><span>${item.date}</span></td><td>${item.supplier}</td><td>${item.warehouse}</td><td>${formatNumber(item.quantity)}</td><td>${badge(item.status, toneByStatus(item.status))}</td></tr>`);
  return `<section class="work-grid"><article class="panel form-panel"><h3>Phiếu nhập mới</h3><label>Nhà cung cấp<input value="Công ty VLXD Minh Phát" /></label><label>Kho nhận<input value="Kho vật liệu Bình Dương" /></label><label>Ghi chú<textarea>Nhập vật liệu theo kế hoạch bổ sung tồn tối thiểu cho công trình.</textarea></label><button class="primary-btn" type="button">Lưu nháp</button></article><article class="panel table-panel"><div class="panel-header"><p class="eyebrow">Gần đây</p><h3>Phiếu nhập kho</h3></div>${table(['Mã phiếu', 'Nhà cung cấp', 'Kho', 'Số lượng', 'Trạng thái'], rows)}</article></section>`;
}

function renderIssues() {
  const rows = fallback.issues.map(item => `<tr><td><strong>${item.code}</strong><span>${item.date}</span></td><td>${item.receiver}</td><td>${item.warehouse}</td><td>${formatNumber(item.quantity)}</td><td>${badge(item.status, toneByStatus(item.status))}</td></tr>`);
  return `<section class="work-grid"><article class="panel form-panel"><h3>Yêu cầu xuất kho</h3><label>Bên nhận<input value="Công trình nhà phố An Phú" /></label><label>Kho xuất<input value="Kho vật liệu Bình Dương" /></label><label>Lý do<textarea>Cấp xi măng, thép và cát cho đội thi công móng.</textarea></label><button class="primary-btn" type="button">Tạo yêu cầu</button></article><article class="panel table-panel"><div class="panel-header"><p class="eyebrow">Theo dõi</p><h3>Phiếu xuất kho</h3></div>${table(['Mã phiếu', 'Bên nhận', 'Kho', 'Số lượng', 'Trạng thái'], rows)}</article></section>`;
}

function renderStocktakes() {
  const rows = fallback.stocktakes.map(item => `<tr><td><strong>${item.code}</strong><span>${item.owner}</span></td><td>${item.warehouse}</td><td>${formatNumber(item.items)}</td><td>${badge(item.status, toneByStatus(item.status))}</td><td><button class="ghost-btn" type="button">Xem chi tiết</button></td></tr>`);
  return `<section class="panel table-panel"><div class="panel-header split"><div><p class="eyebrow">Kiểm kê</p><h3>Phiên kiểm kê</h3></div><button class="primary-btn" type="button">Mở phiên mới</button></div>${table(['Mã phiên', 'Kho', 'Số mặt hàng', 'Trạng thái', 'Thao tác'], rows)}</section>`;
}

function renderAudit() {
  const rows = fallback.audit.map(item => `<tr><td>${item.time}</td><td>${badge(item.action, item.action === 'CONFIRM' ? 'success' : 'muted')}</td><td>${item.module}</td><td>${item.detail}</td></tr>`);
  return `<section class="panel table-panel"><div class="panel-header"><p class="eyebrow">Audit log</p><h3>Lịch sử thao tác gần nhất</h3></div>${table(['Thời gian', 'Hành động', 'Phân hệ', 'Chi tiết'], rows)}</section>`;
}

function renderAssistant() {
  return `<section class="assistant-layout"><article class="panel chat-panel"><div class="panel-header"><p class="eyebrow">AI Assistant</p><h3>Trợ lý kho thông minh</h3></div><div id="chatBox" class="chat-box">Bạn có thể hỏi: sản phẩm nào sắp hết, tổng nhập kho tháng này, hoặc có bao nhiêu kiểm kê đang chờ.</div><div class="prompt-row"><input id="questionInput" type="text" placeholder="Ví dụ: Sản phẩm nào sắp hết?" /><button id="askBtn" class="primary-btn" type="button">Gửi</button></div></article><aside class="panel insight-panel"><h3>Gợi ý câu hỏi</h3><button type="button" data-question="Sản phẩm nào sắp hết?">Sản phẩm nào sắp hết?</button><button type="button" data-question="Có bao nhiêu kho?">Có bao nhiêu kho?</button><button type="button" data-question="Tổng xuất kho tháng này là bao nhiêu?">Tổng xuất kho tháng này?</button></aside></section>`;
}

function renderPage(page) {
  const target = pages[page] ? page : 'overview';
  state.currentPage = target;
  pageTitle.textContent = pages[target].title;
  pageEyebrow.textContent = pages[target].eyebrow;
  navItems.forEach(item => item.classList.toggle('active', item.dataset.page === target));

  const renderers = { overview: renderOverview, inventory: renderInventory, receipts: renderReceipts, issues: renderIssues, stocktakes: renderStocktakes, audit: renderAudit, assistant: renderAssistant };
  pageContent.innerHTML = renderers[target]();
  bindDynamicActions();
}

function bindDynamicActions() {
  pageContent.querySelectorAll('[data-go]').forEach(button => {
    button.addEventListener('click', () => navigate(button.dataset.go));
  });

  const askBtn = document.getElementById('askBtn');
  const questionInput = document.getElementById('questionInput');
  const chatBox = document.getElementById('chatBox');

  if (askBtn && questionInput && chatBox) {
    const askAssistant = async questionOverride => {
      const question = (questionOverride || questionInput.value).trim();
      if (!question) return;
      questionInput.value = question;
      chatBox.textContent = 'Đang xử lý...';

      try {
        const response = await fetch(`${API_BASE}/WarehouseAssistant/ask`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
          body: JSON.stringify({ question })
        });
        if (!response.ok) throw new Error('AI API request failed');
        const data = await response.json();
        chatBox.textContent = data.answer || 'Không có phản hồi.';
      } catch (error) {
        chatBox.textContent = 'Chưa kết nối được backend AI. Gợi ý nhanh: hiện có 3 vật liệu dưới mức tồn tối thiểu, cần ưu tiên bổ sung xi măng PCB40.';
        console.error(error);
      }
    };

    askBtn.addEventListener('click', () => askAssistant());
    questionInput.addEventListener('keydown', event => { if (event.key === 'Enter') askAssistant(); });
    pageContent.querySelectorAll('[data-question]').forEach(button => button.addEventListener('click', () => askAssistant(button.dataset.question)));
  }
}

function navigate(page) {
  window.location.hash = page;
  renderPage(page);
}

navItems.forEach(item => item.addEventListener('click', () => navigate(item.dataset.page)));
if (refreshBtn) refreshBtn.addEventListener('click', loadDashboardData);
window.addEventListener('hashchange', () => renderPage(window.location.hash.replace('#', '') || 'overview'));

setApiStatus();
renderPage(window.location.hash.replace('#', '') || 'overview');
loadDashboardData();
