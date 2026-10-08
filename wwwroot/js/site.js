// Sidebar toggle
const sidebarToggle = document.getElementById('sidebarToggle');
if (sidebarToggle) {
    sidebarToggle.addEventListener('click', function () {
        const sidebar = document.getElementById('sidebar');
        const content = document.getElementById('page-content-wrapper');

        if (window.innerWidth <= 768) {
            sidebar.classList.toggle('show');
        } else {
            sidebar.classList.toggle('collapsed');
            content.classList.toggle('expanded');
        }
    });
}

// Dark mode toggle
(function () {
    const btn = document.getElementById('themeToggle');
    const root = document.documentElement;
    const sync = () => {
        if (btn) btn.innerHTML = root.getAttribute('data-bs-theme') === 'dark'
            ? '<i class="fas fa-sun"></i>' : '<i class="fas fa-moon"></i>';
    };
    sync();
    if (btn) btn.addEventListener('click', function () {
        const next = root.getAttribute('data-bs-theme') === 'dark' ? 'light' : 'dark';
        root.setAttribute('data-bs-theme', next);
        localStorage.setItem('theme', next);
        sync();
    });
})();

// Delete confirmation modal: any form with data-confirm="message"
(function () {
    const modalEl = document.getElementById('confirmDeleteModal');
    if (!modalEl) return;
    const modal = new bootstrap.Modal(modalEl);
    let pendingForm = null;
    document.addEventListener('submit', function (e) {
        const form = e.target;
        if (!(form instanceof HTMLFormElement) || !form.dataset.confirm) return;
        e.preventDefault();
        pendingForm = form;
        document.getElementById('confirmDeleteMessage').textContent = form.dataset.confirm;
        modal.show();
    });
    document.getElementById('confirmDeleteBtn').addEventListener('click', function () {
        if (!pendingForm) return;
        const form = pendingForm;
        pendingForm = null;
        form.removeAttribute('data-confirm');
        form.submit();
    });
})();

// Show toasts rendered by the server
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('#toastContainer .toast').forEach(el => bootstrap.Toast.getOrCreateInstance(el).show());
});
