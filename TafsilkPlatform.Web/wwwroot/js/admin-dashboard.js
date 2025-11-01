/**
 * Modern Admin Dashboard JavaScript
 * Handles all dashboard interactions and data management
 */

// Mock Data
const mockData = {
    recentActivity: [
        { user: "محمد أحمد", action: "طلب جديد #ORD-2023-5875", time: "منذ 5 دقائق", avatar: "https://ui-avatars.com/api/?name=محمد+أحمد&background=667eea&color=fff" },
   { user: "خالد محمود", action: "تم التحقق من الخياط", time: "منذ 15 دقيقة", avatar: "https://ui-avatars.com/api/?name=خالد+محمود&background=10b981&color=fff" },
    { user: "سارة علي", action: "تسجيل مستخدم جديد", time: "منذ 30 دقيقة", avatar: "https://ui-avatars.com/api/?name=سارة+علي&background=f59e0b&color=fff" },
      { user: "فاطمة أحمد", action: "تحديث حالة الطلب", time: "منذ ساعة", avatar: "https://ui-avatars.com/api/?name=فاطمة+أحمد&background=3b82f6&color=fff" }
    ],
    pendingActions: [
      { title: "التحقق من الخياطين", count: "12 طلب", description: "هناك 12 طلب للتحقق من الخياطين بانتظار المراجعة", icon: "fa-user-check" },
        { title: "النزاعات", count: "5 نزاع", description: "هناك 5 نزاعات بانتظار الحل", icon: "fa-exclamation-triangle" },
        { title: "مراجعة الصور", count: "8 صورة", description: "هناك 8 صور بانتظار المراجعة والموافقة", icon: "fa-images" },
        { title: "طلبات الاسترداد", count: "3 طلب", description: "هناك 3 طلبات استرداد بانتظار المعالجة", icon: "fa-money-bill-wave" }
    ],
    users: [
   { id: 1, name: "محمد أحمد", email: "mohamed@example.com", role: "customer", registrationDate: "15 أكتوبر 2023", status: "active" },
      { id: 2, name: "خالد محمود", email: "khaled@example.com", role: "tailor", registrationDate: "10 أكتوبر 2023", status: "active" },
        { id: 3, name: "سارة علي", email: "sara@example.com", role: "customer", registrationDate: "5 أكتوبر 2023", status: "inactive" },
        { id: 4, name: "أحمد سامي", email: "ahmed@example.com", role: "tailor", registrationDate: "1 أكتوبر 2023", status: "suspended" },
        { id: 5, name: "فاطمة محمد", email: "fatma@example.com", role: "corporate", registrationDate: "28 سبتمبر 2023", status: "active" }
    ]
};

// DOM Elements
const sidebar = document.getElementById('sidebar');
const sidebarToggle = document.getElementById('sidebarToggle');
const sidebarClose = document.getElementById('sidebarClose');
const sidebarBackdrop = document.getElementById('sidebarBackdrop');
const mainWrapper = document.querySelector('.main-wrapper');
const navLinks = document.querySelectorAll('.nav-link');
const pages = document.querySelectorAll('.page');
const userMenuToggle = document.getElementById('userMenuToggle');
const userMenuDropdown = document.getElementById('userMenuDropdown');

// Sidebar state
let sidebarCollapsed = false;

// Initialize Dashboard
document.addEventListener('DOMContentLoaded', () => {
    initializeSidebar();
    initializeNavigation();
    initializeUserMenu();
    loadDashboardData();
    initializeEventListeners();
    
    // Check saved sidebar state
    const savedState = localStorage.getItem('sidebarCollapsed');
    if (savedState === 'true' && window.innerWidth > 768) {
        toggleSidebarCollapse();
    }
});

// Sidebar Functions
function initializeSidebar() {
    // Toggle sidebar on mobile/desktop
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', () => {
            if (window.innerWidth <= 768) {
             // Mobile: Show overlay sidebar
                sidebar.classList.add('active');
         sidebarBackdrop.classList.add('active');
                document.body.style.overflow = 'hidden';
            } else {
                // Desktop: Collapse/expand sidebar
    toggleSidebarCollapse();
     }
 });
    }

    // Close sidebar (mobile only)
  if (sidebarClose) {
        sidebarClose.addEventListener('click', closeSidebar);
    }

    // Close sidebar when clicking backdrop
    if (sidebarBackdrop) {
  sidebarBackdrop.addEventListener('click', closeSidebar);
    }
}

function toggleSidebarCollapse() {
    sidebarCollapsed = !sidebarCollapsed;
    sidebar.classList.toggle('collapsed');
    mainWrapper.classList.toggle('expanded');
    
    // Save state to localStorage
  localStorage.setItem('sidebarCollapsed', sidebarCollapsed);
    
    // Update toggle button icon
    const icon = sidebarToggle.querySelector('i');
    if (icon) {
        icon.className = sidebarCollapsed ? 'fas fa-bars' : 'fas fa-bars';
  }
}

function closeSidebar() {
    sidebar.classList.remove('active');
    sidebarBackdrop.classList.remove('active');
  document.body.style.overflow = '';
}

// Navigation Functions
function initializeNavigation() {
    navLinks.forEach(link => {
   link.addEventListener('click', (e) => {
   e.preventDefault();
      const pageId = link.getAttribute('data-page');
        
            // Update active state
            document.querySelectorAll('.nav-item').forEach(item => {
    item.classList.remove('active');
            });
     link.parentElement.classList.add('active');

         // Show page
       showPage(pageId);
    
      // Update breadcrumb
            const navText = link.querySelector('.nav-text');
     if (navText) {
 updateBreadcrumb(navText.textContent.trim());
    }
   
    // Close sidebar on mobile
            if (window.innerWidth <= 768) {
      closeSidebar();
            }
     });
    });
}

function showPage(pageId) {
    pages.forEach(page => {
page.classList.remove('active');
    });
    
    const targetPage = document.getElementById(pageId);
    if (targetPage) {
        targetPage.classList.add('active');
 loadPageData(pageId);
    }
}

function updateBreadcrumb(pageName) {
    const breadcrumb = document.querySelector('.breadcrumb');
    if (breadcrumb) {
    breadcrumb.innerHTML = `
            <i class="fas fa-home"></i>
  <span class="breadcrumb-separator">/</span>
    <span class="breadcrumb-item active">${pageName}</span>
        `;
    }
}

// User Menu Functions
function initializeUserMenu() {
if (userMenuToggle && userMenuDropdown) {
      userMenuToggle.addEventListener('click', (e) => {
            e.stopPropagation();
       userMenuDropdown.classList.toggle('show');
     });

        // Close when clicking outside
        document.addEventListener('click', (e) => {
         if (!userMenuToggle.contains(e.target) && !userMenuDropdown.contains(e.target)) {
   userMenuDropdown.classList.remove('show';
      }
      });
    }
}

// Data Loading Functions
function loadDashboardData() {
    setTimeout(() => {
        loadRecentActivity();
      loadPendingActions();
    }, 500);
}

function loadPageData(pageId) {
    switch (pageId) {
        case 'dashboard':
            loadDashboardData();
     break;
        case 'users':
         loadUsersTable();
          break;
    }
}

function loadRecentActivity() {
  const container = document.getElementById('recentActivity');
    if (!container) return;

    container.innerHTML = mockData.recentActivity.map(activity => `
    <div class="activity-item">
            <div class="activity-avatar">
              <img src="${activity.avatar}" alt="${activity.user}" style="width:100%;height:100%;object-fit:cover;border-radius:50%;">
    </div>
          <div class="activity-content">
           <div class="activity-user">${activity.user}</div>
           <div class="activity-text">${activity.action}</div>
                <div class="activity-time">${activity.time}</div>
       </div>
        </div>
    `).join('');
}

function loadPendingActions() {
    const container = document.getElementById('pendingActions');
    if (!container) return;

    container.innerHTML = mockData.pendingActions.map(action => `
   <div class="action-item">
            <div class="action-icon">
        <i class="fas ${action.icon}"></i>
            </div>
   <div class="action-content">
 <div class="action-title">${action.title}</div>
       <div class="action-text">${action.description}</div>
        <div class="action-count">${action.count}</div>
          </div>
        </div>
    `).join('');
}

function loadUsersTable() {
    const tbody = document.querySelector('#usersTable tbody');
    if (!tbody) return;

    const getRoleText = (role) => {
 const roles = {
            customer: 'عميل',
        tailor: 'خياط',
    corporate: 'عميل مؤسسي'
    };
  return roles[role] || role;
    };

    const getStatusBadge = (status) => {
      const statuses = {
          active: { text: 'نشط', class: 'success' },
          inactive: { text: 'غير نشط', class: 'warning' },
     suspended: { text: 'موقوف', class: 'danger' }
 };
        const statusInfo = statuses[status] || { text: status, class: 'info' };
   return `<span class="status-badge ${statusInfo.class}">${statusInfo.text}</span>`;
    };

    tbody.innerHTML = mockData.users.map(user => `
 <tr>
     <td>
        <div style="display:flex;align-items:center;gap:0.75rem;">
        <img src="https://ui-avatars.com/api/?name=${encodeURIComponent(user.name)}&background=random" 
      alt="${user.name}" 
   style="width:36px;height:36px;border-radius:50%;">
       <strong>${user.name}</strong>
            </div>
   </td>
        <td>${user.email}</td>
   <td>${getRoleText(user.role)}</td>
            <td>${user.registrationDate}</td>
        <td>${getStatusBadge(user.status)}</td>
     <td>
        <div class="action-buttons">
    <button class="btn-icon" onclick="viewUser(${user.id})" title="عرض التفاصيل">
          <i class="fas fa-eye"></i>
      </button>
       <button class="btn-icon" onclick="editUser(${user.id})" title="تعديل">
      <i class="fas fa-edit"></i>
            </button>
        <button class="btn-icon" onclick="deleteUser(${user.id})" title="حذف">
            <i class="fas fa-trash"></i>
               </button>
         </div>
          </td>
        </tr>
    `).join('');
}

// Event Listeners
function initializeEventListeners() {
    // Refresh Data Button
    const refreshBtn = document.getElementById('refreshData');
    if (refreshBtn) {
   refreshBtn.addEventListener('click', () => {
            showToast('جاري تحديث البيانات...', 'info');
            setTimeout(() => {
     loadDashboardData();
    showToast('تم تحديث البيانات بنجاح', 'success');
 }, 1000);
        });
  }

    // Export Users Button
    const exportBtn = document.getElementById('exportUsers');
    if (exportBtn) {
        exportBtn.addEventListener('click', () => {
            showToast('جاري تصدير البيانات...', 'info');
            setTimeout(() => {
                showToast('تم تصدير البيانات بنجاح', 'success');
            }, 1000);
        });
    }

    // User Search
    const userSearch = document.getElementById('userSearch');
    if (userSearch) {
        userSearch.addEventListener('input', () => {
          // Implement search logic
   console.log('Searching:', userSearch.value);
    });
    }
}

// User Actions
function viewUser(id) {
    showToast(`عرض تفاصيل المستخدم #${id}`, 'info');
}

function editUser(id) {
    showToast(`تعديل المستخدم #${id}`, 'info');
}

function deleteUser(id) {
    if (confirm('هل أنت متأكد من حذف هذا المستخدم؟')) {
        showToast(`تم حذف المستخدم #${id}`, 'success');
  }
}

// Toast Notifications
function showToast(message, type = 'info') {
    const container = document.getElementById('toastContainer');
    if (!container) return;

    const icons = {
      success: 'fa-check-circle',
   error: 'fa-exclamation-circle',
     warning: 'fa-exclamation-triangle',
        info: 'fa-info-circle'
    };

    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.innerHTML = `
   <div style="display:flex;align-items:center;gap:0.75rem;">
        <i class="fas ${icons[type]} fa-lg"></i>
          <span>${message}</span>
   </div>
`;

    container.appendChild(toast);

    setTimeout(() => {
        toast.style.animation = 'slideOutLeft 0.3s ease-out';
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// Resize Handler
window.addEventListener('resize', () => {
    if (window.innerWidth > 768) {
        // Close mobile sidebar if open
   closeSidebar();
        
        // Restore desktop sidebar state if it was collapsed
        const savedState = localStorage.getItem('sidebarCollapsed');
      if (savedState === 'true' && !sidebar.classList.contains('collapsed')) {
          sidebar.classList.add('collapsed');
          mainWrapper.classList.add('expanded');
    }
    } else {
  // Remove collapsed state on mobile
        if (sidebar.classList.contains('collapsed')) {
    sidebar.classList.remove('collapsed');
    mainWrapper.classList.remove('expanded');
    }
    }
});
