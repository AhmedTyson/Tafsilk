/**
 * Modern Admin Dashboard JavaScript
 * Handles all dashboard interactions and data management
 */

// Mock Data
const mockData = {
    recentActivity: [
        { user: "Mohammed Ahmed", action: "New Order #ORD-2023-5875", time: "5 mins ago", avatar: "https://ui-avatars.com/api/?name=Mohammed+Ahmed&background=667eea&color=fff" },
        { user: "Khaled Mahmoud", action: "Tailor Verified", time: "15 mins ago", avatar: "https://ui-avatars.com/api/?name=Khaled+Mahmoud&background=10b981&color=fff" },
        { user: "Sara Ali", action: "New User Registration", time: "30 mins ago", avatar: "https://ui-avatars.com/api/?name=Sara+Ali&background=f59e0b&color=fff" },
        { user: "Fatima Ahmed", action: "Order Status Updated", time: "1 hour ago", avatar: "https://ui-avatars.com/api/?name=Fatima+Ahmed&background=3b82f6&color=fff" }
    ],
    pendingActions: [
        { title: "Verify Tailors", count: "12 requests", description: "There are 12 requests to verify tailors awaiting review", icon: "fa-user-check" },
        { title: "Disputes", count: "5 disputes", description: "There are 5 disputes awaiting resolution", icon: "fa-exclamation-triangle" },
        { title: "Image Review", count: "8 images", description: "There are 8 images awaiting review and approval", icon: "fa-images" },
        { title: "Refund Requests", count: "3 requests", description: "There are 3 refund requests awaiting processing", icon: "fa-money-bill-wave" }
    ],
    users: [
        { id: 1, name: "Mohammed Ahmed", email: "mohamed@example.com", role: "customer", registrationDate: "Oct 15, 2023", status: "active" },
        { id: 2, name: "Khaled Mahmoud", email: "khaled@example.com", role: "tailor", registrationDate: "Oct 10, 2023", status: "active" },
        { id: 3, name: "Sara Ali", email: "sara@example.com", role: "customer", registrationDate: "Oct 5, 2023", status: "inactive" },
        { id: 4, name: "Ahmed Sami", email: "ahmed@example.com", role: "tailor", registrationDate: "Oct 1, 2023", status: "suspended" },
        { id: 5, name: "Fatima Mohammed", email: "fatma@example.com", role: "corporate", registrationDate: "Sep 28, 2023", status: "active" }
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

// Navigation Functions - Removed SPA logic to support server-side routing
function initializeNavigation() {
    // Highlight active link based on current URL
    const currentPath = window.location.pathname;
    navLinks.forEach(link => {
        if (link.getAttribute('href') && currentPath.includes(link.getAttribute('href'))) {
            link.parentElement.classList.add('active');
        }
    });
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
                userMenuDropdown.classList.remove('show');
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
            customer: 'Customer',
            tailor: 'Tailor',
            corporate: 'Corporate Client'
        };
        return roles[role] || role;
    };

    const getStatusBadge = (status) => {
        const statuses = {
            active: { text: 'Active', class: 'success' },
            inactive: { text: 'Inactive', class: 'warning' },
            suspended: { text: 'Suspended', class: 'danger' }
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
                    <button class="btn-icon" onclick="viewUser(${user.id})" title="View Details">
                        <i class="fas fa-eye"></i>
                    </button>
                    <button class="btn-icon" onclick="editUser(${user.id})" title="Edit">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="btn-icon" onclick="deleteUser(${user.id})" title="Delete">
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
            showToast('Updating data...', 'info');
            setTimeout(() => {
                loadDashboardData();
                showToast('Data updated successfully', 'success');
            }, 1000);
        });
    }

    // Export Users Button
    const exportBtn = document.getElementById('exportUsers');
    if (exportBtn) {
        exportBtn.addEventListener('click', () => {
            showToast('Exporting data...', 'info');
            setTimeout(() => {
                showToast('Data exported successfully', 'success');
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
    showToast(`View user details #${id}`, 'info');
}

function editUser(id) {
    showToast(`Edit user #${id}`, 'info');
}

function deleteUser(id) {
    if (confirm('Are you sure you want to delete this user?')) {
        showToast(`User deleted #${id}`, 'success');
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
