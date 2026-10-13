const APP_URLS = (window.APP && window.APP.urls) || {};

// ── Helpers shared with page scripts ─────────────────────────────────────────
window.formatMoney = function (value, decimals) {
    const n = Number(value || 0).toLocaleString(undefined, {
        minimumFractionDigits: decimals || 0,
        maximumFractionDigits: decimals || 0
    });
    return ((window.APP && window.APP.currency) || 'PKR') + ' ' + n;
};

// Theme-aware colours for Chart.js
window.chartTheme = function () {
    const css = getComputedStyle(document.documentElement);
    return {
        text: css.getPropertyValue('--bs-secondary-color').trim() || '#6b7280',
        grid: css.getPropertyValue('--bs-border-color').trim() || '#e5e7eb',
        surface: css.getPropertyValue('--bs-body-bg').trim() || '#fff'
    };
};

// Page scripts register charts here so they can be restyled when the theme flips
window.themedCharts = [];
window.registerChart = function (chart, applyTheme) {
    window.themedCharts.push({ chart, applyTheme });
    applyTheme(chart, window.chartTheme());
    chart.update('none');
};

// Removes the skeleton placeholder that precedes a freshly drawn chart
window.revealChart = function (canvas) {
    if (!canvas) return;
    canvas.classList.remove('d-none');
    const sk = canvas.parentElement && canvas.parentElement.querySelector('.skeleton');
    if (sk) sk.remove();
};

// ── Sidebar toggle ───────────────────────────────────────────────────────────
(function () {
    const toggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');
    const backdrop = document.getElementById('sidebar-backdrop');
    const content = document.getElementById('page-content-wrapper');
    if (!toggle || !sidebar) return;

    const closeMobile = () => {
        sidebar.classList.remove('show');
        backdrop && backdrop.classList.remove('show');
    };

    toggle.addEventListener('click', function () {
        if (window.innerWidth <= 768) {
            const open = sidebar.classList.toggle('show');
            backdrop && backdrop.classList.toggle('show', open);
        } else {
            sidebar.classList.toggle('collapsed');
            content.classList.toggle('expanded');
        }
    });
    backdrop && backdrop.addEventListener('click', closeMobile);
    document.addEventListener('keydown', e => { if (e.key === 'Escape') closeMobile(); });
})();

// ── Dark mode toggle ─────────────────────────────────────────────────────────
function setTheme(next) {
    document.documentElement.setAttribute('data-bs-theme', next);
    localStorage.setItem('theme', next);
    syncThemeButton();
    // Charts read their colours once, so restyle them after the CSS variables change
    requestAnimationFrame(() => {
        const t = window.chartTheme();
        window.themedCharts.forEach(c => { c.applyTheme(c.chart, t); c.chart.update('none'); });
    });
}
function syncThemeButton() {
    const btn = document.getElementById('themeToggle');
    if (btn) btn.innerHTML = document.documentElement.getAttribute('data-bs-theme') === 'dark'
        ? '<i class="fas fa-sun" aria-hidden="true"></i>' : '<i class="fas fa-moon" aria-hidden="true"></i>';
}
function toggleTheme() {
    setTheme(document.documentElement.getAttribute('data-bs-theme') === 'dark' ? 'light' : 'dark');
}
syncThemeButton();
document.getElementById('themeToggle')?.addEventListener('click', toggleTheme);

// ── Delete confirmation modal: any form with data-confirm="message" ──────────
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

// ── Submit feedback: disable the button and show a spinner once the form is valid ──
document.addEventListener('submit', function (e) {
    const form = e.target;
    if (!(form instanceof HTMLFormElement) || form.dataset.confirm || e.defaultPrevented) return;
    if (window.jQuery && jQuery.fn.valid && jQuery(form).valid && !jQuery(form).valid()) return;
    if (!form.checkValidity()) return;
    const btn = form.querySelector('button[type="submit"]:not(.btn-link)');
    if (!btn || btn.dataset.busy) return;
    btn.dataset.busy = '1';
    // Disable after the browser has collected the form data
    setTimeout(() => {
        btn.disabled = true;
        btn.insertAdjacentHTML('afterbegin', '<span class="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>');
    }, 0);
});
// Restore buttons when coming back via the browser's back button
window.addEventListener('pageshow', e => {
    if (!e.persisted) return;
    document.querySelectorAll('button[data-busy]').forEach(b => {
        b.disabled = false; delete b.dataset.busy;
        b.querySelector('.spinner-border')?.remove();
    });
});

// ── Server toasts ────────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('#toastContainer .toast').forEach(el => bootstrap.Toast.getOrCreateInstance(el).show());
});

// ── Quick add modal: today's date and the last used category ─────────────────
(function () {
    const modalEl = document.getElementById('quickAddModal');
    if (!modalEl) return;
    const date = modalEl.querySelector('#qa-date');
    const category = modalEl.querySelector('#qa-category');
    modalEl.addEventListener('show.bs.modal', function () {
        if (date && !date.value) {
            const d = new Date();
            date.value = d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0');
        }
        const last = localStorage.getItem('lastCategory');
        if (category && last && !category.value && category.querySelector('option[value="' + last + '"]')) category.value = last;
    });
    modalEl.addEventListener('shown.bs.modal', () => modalEl.querySelector('#qa-title')?.focus());
    modalEl.querySelector('form')?.addEventListener('submit', function () {
        if (category && category.value) localStorage.setItem('lastCategory', category.value);
    });
})();

// ── Command palette (Ctrl/Cmd + K) ───────────────────────────────────────────
(function () {
    const modalEl = document.getElementById('paletteModal');
    if (!modalEl) return;
    const modal = new bootstrap.Modal(modalEl);
    const input = document.getElementById('paletteInput');
    const list = document.getElementById('paletteList');
    const go = url => () => { window.location.href = url; };
    const commands = [
        { icon: 'fa-plus-circle', label: 'Add expense', hint: 'Quick add', run: () => { modal.hide(); bootstrap.Modal.getOrCreateInstance('#quickAddModal').show(); } },
        { icon: 'fa-tachometer-alt', label: 'Go to Dashboard', run: go(APP_URLS.dashboard) },
        { icon: 'fa-receipt', label: 'Go to Expenses', run: go(APP_URLS.expenses) },
        { icon: 'fa-tags', label: 'Go to Categories', run: go(APP_URLS.categories) },
        { icon: 'fa-tag', label: 'New category', run: go(APP_URLS.newCategory) },
        { icon: 'fa-bullseye', label: 'Go to Budgets', run: go(APP_URLS.budgets) },
        { icon: 'fa-chart-bar', label: 'Go to Reports', run: go(APP_URLS.reports) },
        { icon: 'fa-user-cog', label: 'Profile & settings', run: go(APP_URLS.profile) },
        { icon: 'fa-key', label: 'Change password', run: go(APP_URLS.password) },
        { icon: 'fa-adjust', label: 'Toggle dark mode', run: () => { modal.hide(); toggleTheme(); } },
        { icon: 'fa-route', label: 'Take the tour', run: () => { modal.hide(); window.startTour && window.startTour(true); } }
    ];
    let shown = [], active = 0;

    function render() {
        const q = input.value.trim().toLowerCase();
        shown = commands.filter(c => c.label.toLowerCase().includes(q));
        active = Math.min(active, Math.max(shown.length - 1, 0));
        list.innerHTML = '';
        if (!shown.length) {
            list.innerHTML = '<div class="p-3 text-muted small">No matching commands</div>';
            return;
        }
        shown.forEach((c, i) => {
            const b = document.createElement('button');
            b.type = 'button';
            b.setAttribute('role', 'option');
            b.className = 'list-group-item list-group-item-action d-flex align-items-center' + (i === active ? ' active' : '');
            b.innerHTML = '<i class="fas ' + c.icon + ' fa-fw me-3" aria-hidden="true"></i><span class="flex-grow-1"></span>' +
                (c.hint ? '<small class="opacity-75">' + c.hint + '</small>' : '');
            b.querySelector('span').textContent = c.label;
            b.addEventListener('click', c.run);
            b.addEventListener('mousemove', () => { active = i; mark(); });
            list.appendChild(b);
        });
    }
    function mark() {
        [...list.children].forEach((el, i) => el.classList.toggle('active', i === active));
        list.children[active]?.scrollIntoView({ block: 'nearest' });
    }

    input.addEventListener('input', () => { active = 0; render(); });
    input.addEventListener('keydown', e => {
        if (e.key === 'ArrowDown') { e.preventDefault(); active = (active + 1) % Math.max(shown.length, 1); mark(); }
        else if (e.key === 'ArrowUp') { e.preventDefault(); active = (active - 1 + shown.length) % Math.max(shown.length, 1); mark(); }
        else if (e.key === 'Enter') { e.preventDefault(); shown[active]?.run(); }
    });
    modalEl.addEventListener('shown.bs.modal', () => input.focus());
    modalEl.addEventListener('show.bs.modal', () => { input.value = ''; active = 0; render(); });

    document.addEventListener('keydown', e => {
        if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
            e.preventDefault();
            modal.show();
        }
    });
    document.querySelectorAll('[data-open-palette]').forEach(b => b.addEventListener('click', () => modal.show()));
})();

// ── Onboarding tour (first visit to the dashboard) ───────────────────────────
(function () {
    const card = document.getElementById('tourCard');
    if (!card) return;
    const steps = [
        { sel: '[data-tour="nav-dashboard"]', title: 'Your dashboard', text: 'Spending totals, trends and budget progress for the month at a glance.' },
        { sel: '[data-tour="add-expense"]', title: 'Add expenses fast', text: 'Use this button from any page. It remembers your last category and defaults to today.' },
        { sel: '[data-tour="nav-expenses"]', title: 'Browse and filter', text: 'Search, filter by category or month, and edit or delete entries.' },
        { sel: '[data-tour="nav-budgets"]', title: 'Set budgets', text: 'Give categories a monthly limit and get warned at 80% and 100%.' },
        { sel: '[data-open-palette]', title: 'Quick search', text: 'Press Ctrl+K anywhere to jump to a page or run an action.' },
        { sel: '#themeToggle', title: 'Dark mode', text: 'Switch themes here. Your choice is remembered.' }
    ];
    let i = 0, current = null;

    function clear() { current && current.classList.remove('tour-highlight'); current = null; }
    function show() {
        clear();
        const s = steps[i];
        const el = document.querySelector(s.sel);
        if (!el || el.offsetParent === null) { next(); return; }   // skip hidden targets
        current = el;
        el.classList.add('tour-highlight');
        el.scrollIntoView({ block: 'nearest' });
        document.getElementById('tourTitle').textContent = s.title;
        document.getElementById('tourText').textContent = s.text;
        document.getElementById('tourStep').textContent = (i + 1) + ' / ' + steps.length;
        document.getElementById('tourNext').textContent = i === steps.length - 1 ? 'Done' : 'Next';
        card.classList.remove('d-none');
        const r = el.getBoundingClientRect();
        const w = card.offsetWidth, h = card.offsetHeight;
        let left = r.right + 14, top = r.top + r.height / 2 - h / 2;
        if (left + w > window.innerWidth - 12) { left = Math.max(12, r.left + r.width / 2 - w / 2); top = r.bottom + 14; }
        card.style.left = Math.max(12, Math.min(left, window.innerWidth - w - 12)) + 'px';
        card.style.top = Math.max(12, Math.min(top, window.innerHeight - h - 12)) + 'px';
    }
    function end() {
        clear();
        card.classList.add('d-none');
        localStorage.setItem('tourDone', '1');
    }
    function next() {
        i++;
        if (i >= steps.length) end(); else show();
    }
    document.getElementById('tourNext').addEventListener('click', next);
    document.getElementById('tourSkip').addEventListener('click', end);
    document.addEventListener('keydown', e => { if (e.key === 'Escape' && !card.classList.contains('d-none')) end(); });

    window.startTour = function (force) {
        if (!force && localStorage.getItem('tourDone')) return;
        if (window.innerWidth <= 768) return;   // sidebar targets are hidden on small screens
        i = 0;
        show();
    };
    // Only auto-start on the dashboard
    if (document.body.querySelector('[data-auto-tour]')) window.addEventListener('load', () => setTimeout(() => window.startTour(false), 400));
})();
