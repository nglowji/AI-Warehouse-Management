const API_BASE = 'http://localhost:5076/api';

const summaryEls = {
  totalProducts: document.getElementById('totalProducts'),
  totalWarehouses: document.getElementById('totalWarehouses'),
  lowStockProducts: document.getElementById('lowStockProducts'),
  pendingStocktakes: document.getElementById('pendingStocktakes'),
  totalInventoryQuantity: document.getElementById('totalInventoryQuantity'),
  incomingThisMonth: document.getElementById('incomingThisMonth'),
  outgoingThisMonth: document.getElementById('outgoingThisMonth'),
  netMovementThisMonth: document.getElementById('netMovementThisMonth')
};

const lowStockTableBody = document.getElementById('lowStockTableBody');
const refreshBtn = document.getElementById('refreshBtn');

async function fetchJson(url) {
  const response = await fetch(url, {
    headers: {
      'Accept': 'application/json'
    }
  });

  if (!response.ok) {
    throw new Error(`Request failed: ${response.status}`);
  }

  return response.json();
}

function formatNumber(value) {
  return Number(value || 0).toLocaleString(undefined, { maximumFractionDigits: 2 });
}

function renderSummary(summary) {
  summaryEls.totalProducts.textContent = formatNumber(summary.totalProducts);
  summaryEls.totalWarehouses.textContent = formatNumber(summary.totalWarehouses);
  summaryEls.lowStockProducts.textContent = formatNumber(summary.lowStockProducts);
  summaryEls.pendingStocktakes.textContent = formatNumber(summary.pendingStocktakes);
  summaryEls.totalInventoryQuantity.textContent = formatNumber(summary.totalInventoryQuantity);
  summaryEls.incomingThisMonth.textContent = formatNumber(summary.incomingThisMonth);
  summaryEls.outgoingThisMonth.textContent = formatNumber(summary.outgoingThisMonth);
  summaryEls.netMovementThisMonth.textContent = formatNumber(summary.netMovementThisMonth);
}

function renderLowStockItems(items) {
  if (!items || items.length === 0) {
    lowStockTableBody.innerHTML = `
      <tr>
        <td colspan="6">No low stock issues detected.</td>
      </tr>
    `;
    return;
  }

  lowStockTableBody.innerHTML = items.map(item => `
    <tr>
      <td>${item.productName}</td>
      <td>${item.warehouseName}</td>
      <td>${item.locationName}</td>
      <td>${formatNumber(item.availableQuantity)}</td>
      <td>${formatNumber(item.minimumStock)}</td>
      <td>${formatNumber(item.shortfall)}</td>
    </tr>
  `).join('');
}

async function loadDashboard() {
  try {
    const [summary, lowStockItems] = await Promise.all([
      fetchJson(`${API_BASE}/dashboard/summary`),
      fetchJson(`${API_BASE}/dashboard/low-stock`)
    ]);

    renderSummary(summary);
    renderLowStockItems(lowStockItems);
  } catch (error) {
    lowStockTableBody.innerHTML = `
      <tr>
        <td colspan="6">Unable to load dashboard data. Please ensure the backend is running on ${API_BASE}.</td>
      </tr>
    `;
    console.error(error);
  }
}

refreshBtn.addEventListener('click', loadDashboard);
loadDashboard();
