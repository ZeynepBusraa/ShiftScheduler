// =====================================================================
//  ShiftScheduler – Frontend Ana Mantığı
//  API Base: http://localhost:5094
// =====================================================================

const API = 'http://localhost:5094/api';
let currentUser = null;  // { id, name, role, isSenior, departmentId, remainingChangeRequests }
let activeTab = 'asistan'; // 'asistan' | 'uzman'
let allShifts = [];        // Backend'den gelen tüm nöbetler (o anki görünüm için)

// ─── Yardımcı: Authenticated Fetch ───────────────────────────────────
function authHeaders() {
    const token = localStorage.getItem('token');
    return { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` };
}

async function apiFetch(path, options = {}) {
    options.headers = { ...authHeaders(), ...(options.headers || {}) };
    const res = await fetch(API + path, options);
    const data = await res.json();
    if (!res.ok && res.status === 401) {
        logout();
        return null;
    }
    return data;
}

// ─── Ay/Yıl yardımcıları ─────────────────────────────────────────────
function getSelectedYear()  { return parseInt(document.getElementById('year-select').value); }
function getSelectedMonth() { return parseInt(document.getElementById('month-select').value); }
function getDeptFilter()    { 
    const sel = document.getElementById('global-dept-select');
    return sel ? parseInt(sel.value) : (currentUser?.departmentId ?? 1);
}

// ─── Kayıt / Giriş form geçişleri ────────────────────────────────────
document.getElementById('show-register').addEventListener('click', (e) => {
    e.preventDefault();
    document.getElementById('login-form').classList.add('hidden');
    document.getElementById('register-form').classList.remove('hidden');
});
document.getElementById('show-login').addEventListener('click', (e) => {
    e.preventDefault();
    document.getElementById('register-form').classList.add('hidden');
    document.getElementById('login-form').classList.remove('hidden');
});

// ─── GİRİŞ ───────────────────────────────────────────────────────────
document.getElementById('login-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    const email    = document.getElementById('email').value;
    const password = document.getElementById('password').value;

    try {
        const res  = await fetch(`${API}/Auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        });
        const data = await res.json();
        console.log('Login response:', data);

        if (res.ok && data.success) {
            localStorage.setItem('token', data.data.token);
            currentUser = {
                id: data.data.userId,
                name: data.data.fullName,
                role: data.data.role,          // "Bashekim" | "Asistan" | "Uzman"
                isSenior: data.data.isSenior,
                departmentId: data.data.departmentId,
                departmentName: data.data.departmentName,
                remainingChangeRequests: data.data.remainingChangeRequests ?? 0
            };
            console.log('currentUser:', currentUser);
            document.getElementById('login-form').reset();
            showDashboard();
        } else {
            alert('Hata: ' + (data.message || 'Giriş başarısız.'));
        }
    } catch (err) {
        console.error('Bağlantı hatası:', err);
        alert('Sunucuya bağlanılamadı. API çalışıyor mu?');
    }
});

// ─── KAYIT ───────────────────────────────────────────────────────────
document.getElementById('register-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    const body = {
        firstName: document.getElementById('reg-firstname').value,
        lastName:  document.getElementById('reg-lastname').value,
        email:     document.getElementById('reg-email').value,
        password:  document.getElementById('reg-password').value,
        departmentId: parseInt(document.getElementById('reg-department').value),
        role: 2    // Asistan (varsayılan)
    };
    try {
        const res  = await fetch(`${API}/Users`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
        const data = await res.json();
        if (data.success) {
            alert('Kayıt başarılı! Giriş yapabilirsiniz.');
            document.getElementById('register-form').reset();
            document.getElementById('register-form').classList.add('hidden');
            document.getElementById('login-form').classList.remove('hidden');
        } else {
            alert('Kayıt hatası: ' + (data.message || 'Bilinmeyen hata.'));
        }
    } catch (err) {
        alert('Sunucuya bağlanılamadı.');
    }
});

// ─── DASHBOARD KURULUMU ───────────────────────────────────────────────
function showDashboard() {
    document.getElementById('login-view').classList.add('hidden');
    document.getElementById('dashboard-view').classList.remove('hidden');

    // Kullanıcı adını göster
    const userLabel = document.getElementById('user-name-label');
    if (userLabel) userLabel.textContent = currentUser.name + ' (' + currentUser.role + ')';

    const role     = currentUser.role; // "Bashekim" | "Asistan" | "Uzman"
    const isSenior = String(currentUser.isSenior).toLowerCase() === 'true';

    // Rol bazlı buton görünürlükleri
    const btnOnay      = document.getElementById('btn-onay-ekrani');
    const btnNobetEkle = document.getElementById('btn-nobet-ekle');
    const btnDegisim   = document.getElementById('btn-nobet-degisimi');
    const btnTalepler  = document.getElementById('btn-taleplerim');
    const deptFilter   = document.getElementById('bashekim-dept-filter');

    // Hepsini gizle, sonra role göre aç
    [btnOnay, btnNobetEkle, btnDegisim, btnTalepler].forEach(b => { if (b) b.style.display = 'none'; });
    if (deptFilter) deptFilter.classList.add('hidden');

    if (role === 'Bashekim') {
        if (btnOnay)    btnOnay.style.display    = 'inline-block';
        if (deptFilter) deptFilter.classList.remove('hidden');
    } else if (role === 'Asistan' || role === 'Uzman') {
        if (btnDegisim)  btnDegisim.style.display  = 'inline-block';
        if (btnTalepler) btnTalepler.style.display  = 'inline-block';
        if (isSenior && btnNobetEkle) btnNobetEkle.style.display = 'block';
    }

    // Ay/Yıl seçicilerini doldur
    fillYearMonth();

    // Takvimi yükle
    loadCalendar();
}

// ─── YIL / AY SEÇİCİLERİ ─────────────────────────────────────────────
function fillYearMonth() {
    const now = new Date();
    const yearSel  = document.getElementById('year-select');
    const monthSel = document.getElementById('month-select');

    // Yılları doldur (2024-2027)
    yearSel.innerHTML = '';
    for (let y = 2024; y <= 2027; y++) {
        const opt = document.createElement('option');
        opt.value = y; opt.textContent = y;
        if (y === now.getFullYear()) opt.selected = true;
        yearSel.appendChild(opt);
    }

    // Ayları doldur
    const aylar = ['Ocak','Şubat','Mart','Nisan','Mayıs','Haziran','Temmuz','Ağustos','Eylül','Ekim','Kasım','Aralık'];
    monthSel.innerHTML = '';
    aylar.forEach((ad, i) => {
        const opt = document.createElement('option');
        opt.value = i + 1; opt.textContent = ad;
        if (i + 1 === now.getMonth() + 1) opt.selected = true;
        monthSel.appendChild(opt);
    });

    yearSel.addEventListener('change',  loadCalendar);
    monthSel.addEventListener('change', loadCalendar);

    const deptSel = document.getElementById('global-dept-select');
    if (deptSel) deptSel.addEventListener('change', loadCalendar);
}

// ─── TAKVİM YÜKLE ────────────────────────────────────────────────────
async function loadCalendar() {
    const container = document.getElementById('calendar-days');
    container.innerHTML = '<div style="padding:2rem;text-align:center;color:#888;">Yükleniyor...</div>';

    const data = await apiFetch('/Shifts/list');
    if (!data || !data.success) {
        container.innerHTML = '<div style="padding:2rem;text-align:center;color:red;">Nöbetler yüklenemedi.</div>';
        return;
    }

    allShifts = data.data || [];
    renderCalendar();
}

function renderCalendar() {
    const year  = getSelectedYear();
    const month = getSelectedMonth(); // 1-12
    const container = document.getElementById('calendar-days');
    container.innerHTML = '';

    // Ay bazında filtrele
    const monthShifts = allShifts.filter(s => {
        if (!s.date) return false;
        const d = new Date(s.date);
        return d.getFullYear() === year && (d.getMonth() + 1) === month;
    });

    const daysInMonth  = new Date(year, month, 0).getDate();
    let firstDayOfWeek = new Date(year, month - 1, 1).getDay(); // 0=Pazar
    // Pazartesi başlangıçlı grid
    firstDayOfWeek = (firstDayOfWeek === 0) ? 6 : firstDayOfWeek - 1;

    // Boş hücreler
    for (let i = 0; i < firstDayOfWeek; i++) {
        const empty = document.createElement('div');
        empty.className = 'day-box empty-box';
        container.appendChild(empty);
    }

    for (let d = 1; d <= daysInMonth; d++) {
        const box = document.createElement('div');
        box.className = 'day-box';

        const numEl = document.createElement('div');
        numEl.className = 'day-number';
        numEl.textContent = d;
        box.appendChild(numEl);

        // O güne ait nöbetler
        const dayShifts = monthShifts.filter(s => new Date(s.date).getDate() === d);
        if (dayShifts.length === 0) {
            const emptyTxt = document.createElement('span');
            emptyTxt.style.cssText = 'font-size:0.68rem;color:#bbb;display:block;margin-top:6px;';
            emptyTxt.textContent = '—';
            box.appendChild(emptyTxt);
        } else {
            dayShifts.forEach(s => {
                const badge = document.createElement('span');
                badge.className = 'shift-badge';
                // Gösterim: doktor adını göstermek isterseniz ek bir API çağrısı gerekir, şimdilik ID ile kalıyor
                badge.textContent = `Dr. #${s.userId}`;
                badge.title = `Nöbet ID: ${s.id} | Tür: ${s.shiftType}`;
                badge.classList.add(s.shiftType === 0 ? 'shift-kidemli' : 'shift-comez');
                box.appendChild(badge);
            });
        }

        container.appendChild(box);
    }

    // Boşluk tamamlama (7'nin katı olsun)
    const totalCells = firstDayOfWeek + daysInMonth;
    const remainder  = totalCells % 7;
    if (remainder !== 0) {
        for (let i = 0; i < (7 - remainder); i++) {
            const empty = document.createElement('div');
            empty.className = 'day-box empty-box';
            container.appendChild(empty);
        }
    }
}

// ─── TAB BUTONLARI ────────────────────────────────────────────────────
document.getElementById('tab-asistan').addEventListener('click', () => {
    activeTab = 'asistan';
    document.getElementById('tab-asistan').className = 'btn-primary';
    document.getElementById('tab-uzman').className   = 'btn-outline';
    renderCalendar();
});
document.getElementById('tab-uzman').addEventListener('click', () => {
    activeTab = 'uzman';
    document.getElementById('tab-asistan').className = 'btn-outline';
    document.getElementById('tab-uzman').className   = 'btn-primary';
    renderCalendar();
});

// ─── NÖBET OLUŞTUR (OTO) ─────────────────────────────────────────────
document.getElementById('generate-btn').addEventListener('click', async () => {
    const year  = getSelectedYear();
    const month = getSelectedMonth();
    const deptId = currentUser.role === 'Bashekim' ? getDeptFilter() : currentUser.departmentId;
    const listType = activeTab === 'asistan' ? 0 : 1; // 0=Asistan, 1=Uzman

    if (!confirm(`${year}/${month} ayı için ${activeTab} nöbet listesi otomatik oluşturulsun mu?`)) return;

    const data = await apiFetch('/Shifts/generate', {
        method: 'POST',
        body: JSON.stringify({ year, month, departmentId: deptId, listType })
    });

    if (data && data.success) {
        alert('Nöbet listesi başarıyla oluşturuldu!');
        loadCalendar();
    } else {
        alert('Hata: ' + (data?.message || 'Nöbet oluşturulamadı.'));
    }
});

// ─── MANUEL SHIFT EKLE (YENİ MODAL) ──────────────────────────────────
document.getElementById('manual-add-btn').addEventListener('click', () => {
    openManualAddModal();
});

async function openManualAddModal() {
    // Dropdown'ları doldur (yıl, ay)
    const yearSelect = document.getElementById('manual-year');
    const monthSelect = document.getElementById('manual-month');
    const currentYear = new Date().getFullYear();
    yearSelect.innerHTML = '';
    for (let y = currentYear - 1; y <= currentYear + 2; y++) {
        const opt = document.createElement('option');
        opt.value = y; opt.textContent = y;
        if (y === currentYear) opt.selected = true;
        yearSelect.appendChild(opt);
    }
    const months = ['Ocak','Şubat','Mart','Nisan','Mayıs','Haziran','Temmuz','Ağustos','Eylül','Ekim','Kasım','Aralık'];
    monthSelect.innerHTML = '';
    months.forEach((name, idx) => {
        const opt = document.createElement('option');
        opt.value = idx + 1; opt.textContent = name;
        if (idx === new Date().getMonth()) opt.selected = true;
        monthSelect.appendChild(opt);
    });

    // Gün dropdown'ını dinamik yap
    function updateDays() {
        const year = parseInt(yearSelect.value);
        const month = parseInt(monthSelect.value);
        const daysInMonth = new Date(year, month, 0).getDate();
        const daySelect = document.getElementById('manual-day');
        daySelect.innerHTML = '';
        for (let d = 1; d <= daysInMonth; d++) {
            const opt = document.createElement('option');
            opt.value = d; opt.textContent = d;
            if (d === 1) opt.selected = true;
            daySelect.appendChild(opt);
        }
    }
    yearSelect.addEventListener('change', updateDays);
    monthSelect.addEventListener('change', updateDays);
    updateDays();

    // Doktor listesini yükle
    await loadDoctorsIntoSelect();

    // Modal'ı göster
    document.getElementById('manual-add-modal').classList.remove('hidden');
}

async function loadDoctorsIntoSelect() {
    const select = document.getElementById('manual-doctor');
    select.innerHTML = '<option value="">Yükleniyor...</option>';
    try {
        const data = await apiFetch('/Users');
        if (data && data.success && Array.isArray(data.data)) {
            select.innerHTML = '';
            data.data.forEach(user => {
                const opt = document.createElement('option');
                opt.value = user.id;
                opt.textContent = `${user.firstName} ${user.lastName} (${user.role})`;
                select.appendChild(opt);
            });
        } else {
            select.innerHTML = '<option value="">Doktor listesi alınamadı</option>';
        }
    } catch (err) {
        console.error(err);
        select.innerHTML = '<option value="">Hata oluştu</option>';
    }
}

document.getElementById('submit-manual-btn').addEventListener('click', async () => {
    const year = parseInt(document.getElementById('manual-year').value);
    const month = parseInt(document.getElementById('manual-month').value);
    const day = parseInt(document.getElementById('manual-day').value);
    const userId = parseInt(document.getElementById('manual-doctor').value);
    const shiftType = parseInt(document.getElementById('manual-shift-type').value);

    if (!userId) { alert('Lütfen bir doktor seçin.'); return; }

    const dateStr = `${year}-${String(month).padStart(2,'0')}-${String(day).padStart(2,'0')}`;
    await addShiftManually(userId, dateStr, shiftType);
    document.getElementById('manual-add-modal').classList.add('hidden');
});

document.getElementById('close-manual-modal').addEventListener('click', () => {
    document.getElementById('manual-add-modal').classList.add('hidden');
});

// Güncellenmiş addShiftManually – artık shiftType parametresi alıyor
async function addShiftManually(userId, dateStr, shiftType = 0) {
    const data = await apiFetch('/Shifts/save', {
        method: 'POST',
        body: JSON.stringify({ id: 0, userId, date: dateStr + 'T00:00:00', shiftType: shiftType, isApproved: false })
    });
    if (data && data.success) {
        alert('Nöbet eklendi!');
        loadCalendar(); // Takvimi yenile
    } else {
        alert('Hata: ' + (data?.message || 'Nöbet eklenemedi.'));
    }
}

// ─── BAŞHEKİME GÖNDER ────────────────────────────────────────────────
document.getElementById('submit-to-bashekim-btn').addEventListener('click', async () => {
    // Önce mevcut liste ID'sini bul
    const listsData = await apiFetch('/ShiftLists');
    if (!listsData || !listsData.success) { alert('Liste bilgisi alınamadı.'); return; }

    const year  = getSelectedYear();
    const month = getSelectedMonth();
    const listType = activeTab === 'asistan' ? 0 : 1;
    const deptId = currentUser.departmentId;

    const myList = (listsData.data || []).find(l =>
        l.year === year && l.month === month && l.listType === listType &&
        l.departmentId === deptId && l.status === 0 // 0=Taslak
    );

    if (!myList) {
        alert('Gönderilecek taslak liste bulunamadı. Önce nöbetleri oluşturun.');
        return;
    }

    if (!confirm('Liste başhekime onaya gönderilsin mi?')) return;

    const data = await apiFetch(`/ShiftLists/${myList.id}/submit`, { method: 'PUT' });
    if (data && data.success) {
        alert('Liste başhekime gönderildi!');
    } else {
        alert('Hata: ' + (data?.message || 'Gönderilemedi.'));
    }
});

// ─── ONAY BEKLEYENLER (BAŞHEKİM) ─────────────────────────────────────
document.getElementById('btn-onay-ekrani').addEventListener('click', async () => {
    await openApprovalPanel();
});

async function openApprovalPanel() {
    // ShiftLists: onay bekleyenler (status=1 → Gonderildi)
    const listsData = await apiFetch('/ShiftLists');
    const reqData   = await apiFetch('/ShiftRequests');

    let html = '<div id="approval-overlay" style="position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,0.6);z-index:200;overflow-y:auto;padding:2rem;box-sizing:border-box;">';
    html += '<div style="max-width:700px;margin:auto;background:white;border-radius:24px;padding:2rem;">';
    html += '<div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:1.5rem;">';
    html += '<h3 style="margin:0;">📋 Onay Bekleyen İşlemler</h3>';
    html += '<button onclick="document.getElementById(\'approval-overlay\').remove()" class="btn-logout">Kapat</button></div>';

    // Nöbet listeleri
    const pendingLists = (listsData?.data || []).filter(l => l.status === 1); // 1=Gonderildi
    html += `<h4>Nöbet Listeleri (${pendingLists.length} adet)</h4>`;
    if (pendingLists.length === 0) {
        html += '<p style="color:#888;">Onay bekleyen nöbet listesi yok.</p>';
    } else {
        pendingLists.forEach(l => {
            const tip = l.listType === 0 ? 'Asistan' : 'Uzman';
            html += `<div class="request-item">
                <strong>${l.departmentName} – ${tip} Listesi (${l.year}/${l.month})</strong>
                <br><small>Hazırlayan: ${l.preparedByUserName}</small>
                <div style="margin-top:10px;display:flex;gap:8px;">
                    <button onclick="approveList(${l.id}, true)" class="btn-success" style="width:auto;padding:0.4rem 1rem;font-size:0.85rem;">✓ Onayla</button>
                    <button onclick="approveList(${l.id}, false)" class="btn-logout" style="padding:0.4rem 1rem;font-size:0.85rem;">✗ Reddet</button>
                </div>
            </div>`;
        });
    }

    // Nöbet değişim talepleri
    const pendingReqs = (reqData?.data || []).filter(r => r.status === 2); // 2=KidemliOnayladi
    html += `<h4 style="margin-top:1.5rem;">🔄 Nöbet Değişim Talepleri (${pendingReqs.length} adet)</h4>`;
    if (pendingReqs.length === 0) {
        html += '<p style="color:#888;">Onay bekleyen değişim talebi yok.</p>';
    } else {
        pendingReqs.forEach(r => {
            html += `<div class="request-item">
                <strong>Talep #${r.id}</strong> — Nöbet ID: ${r.shiftId}
                <br><small>Talep Eden: #${r.requesterId} → Hedef: #${r.targetDoctorId}</small>
                <div style="margin-top:10px;display:flex;gap:8px;">
                    <button onclick="approveRequest(${r.id}, true)" class="btn-success" style="width:auto;padding:0.4rem 1rem;font-size:0.85rem;">✓ Onayla</button>
                    <button onclick="approveRequest(${r.id}, false)" class="btn-logout" style="padding:0.4rem 1rem;font-size:0.85rem;">✗ Reddet</button>
                </div>
            </div>`;
        });
    }

    html += '</div></div>';
    document.body.insertAdjacentHTML('beforeend', html);
}

window.approveList = async (id, approve) => {
    const data = await apiFetch(`/ShiftLists/${id}/approve`, { method: 'PUT', body: JSON.stringify(approve) });
    if (data && data.success) {
        alert(approve ? 'Liste onaylandı!' : 'Liste reddedildi.');
        document.getElementById('approval-overlay')?.remove();
    } else {
        alert('Hata: ' + (data?.message || 'İşlem başarısız.'));
    }
};

window.approveRequest = async (id, approve) => {
    const data = await apiFetch(`/ShiftRequests/${id}/approve`, { method: 'PUT', body: JSON.stringify(approve) });
    if (data && data.success) {
        alert(approve ? 'Talep onaylandı!' : 'Talep reddedildi.');
        document.getElementById('approval-overlay')?.remove();
    } else {
        alert('Hata: ' + (data?.message || 'İşlem başarısız.'));
    }
};

// ─── NÖBET DEĞİŞİMİ MODAL ────────────────────────────────────────────
document.getElementById('btn-nobet-degisimi').addEventListener('click', async () => {
    document.getElementById('modal-swap-left').textContent = currentUser.remainingChangeRequests;
    await loadMyShiftsIntoSelect();
    document.getElementById('swap-modal').classList.remove('hidden');
});

document.getElementById('close-swap-modal').addEventListener('click', () => {
    document.getElementById('swap-modal').classList.add('hidden');
});

async function loadMyShiftsIntoSelect() {
    const sel = document.getElementById('my-shifts-select');
    sel.innerHTML = '<option value="">Yükleniyor...</option>';

    const data = await apiFetch('/Shifts/list');
    if (!data || !data.success) {
        sel.innerHTML = '<option value="">Nöbetler alınamadı</option>';
        return;
    }

    const myShifts = (data.data || []).filter(s => s.userId === currentUser.id);
    sel.innerHTML = '<option value="">Nöbet Seçin</option>';
    myShifts.forEach(s => {
        const opt = document.createElement('option');
        opt.value = s.id;
        opt.textContent = new Date(s.date).toLocaleDateString('tr-TR');
        sel.appendChild(opt);
    });
}

document.getElementById('submit-swap-btn').addEventListener('click', async () => {
    const shiftId  = parseInt(document.getElementById('my-shifts-select').value);
    const targetId = parseInt(document.getElementById('swap-target-id').value);

    if (!shiftId)  { alert('Lütfen değiştirmek istediğiniz nöbeti seçin.'); return; }
    if (!targetId) { alert('Lütfen hedef doktorun ID\'sini girin.'); return; }

    const data = await apiFetch('/ShiftRequests', {
        method: 'POST',
        body: JSON.stringify({ shiftId, targetDoctorId: targetId })
    });

    if (data && data.success) {
        alert('Nöbet değişim talebiniz oluşturuldu!');
        currentUser.remainingChangeRequests = Math.max(0, currentUser.remainingChangeRequests - 1);
        document.getElementById('modal-swap-left').textContent = currentUser.remainingChangeRequests;
        document.getElementById('swap-modal').classList.add('hidden');
        // Formu temizle
        document.getElementById('my-shifts-select').value = '';
        document.getElementById('swap-target-id').value   = '';
        document.getElementById('swap-reason').value      = '';
    } else {
        alert('Hata: ' + (data?.message || 'Talep oluşturulamadı.'));
    }
});

// ─── TALEPLERİM PANELİ ───────────────────────────────────────────────
document.getElementById('btn-taleplerim').addEventListener('click', async () => {
    await openMyRequestsPanel();
});

async function openMyRequestsPanel() {
    const data = await apiFetch('/ShiftRequests');

    const statusLabel = ['Bekliyor', 'Hedef Onayladı', 'Kıdemli Onayladı', 'Başhekim Onayladı', 'Reddedildi'];
    const statusClass  = ['status-bekliyor', 'status-bekliyor', 'status-bekliyor', 'status-onaylandi', 'status-reddedildi'];

    let html = '<div id="requests-overlay" style="position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,0.6);z-index:200;overflow-y:auto;padding:2rem;box-sizing:border-box;">';
    html += '<div style="max-width:600px;margin:auto;background:white;border-radius:24px;padding:2rem;">';
    html += '<div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:1.5rem;">';
    html += '<h3 style="margin:0;">📁 Taleplerim</h3>';
    html += '<button onclick="document.getElementById(\'requests-overlay\').remove()" class="btn-logout">Kapat</button></div>';

    const requests = data?.data || [];
    if (requests.length === 0) {
        html += '<p style="color:#888;text-align:center;">Henüz hiç talebiniz yok.</p>';
    } else {
        requests.forEach(r => {
            const isIncoming = r.targetDoctorId === currentUser.id && r.status === 0;
            html += `<div class="request-item">
                <span class="request-status ${statusClass[r.status] || ''}">${statusLabel[r.status] ?? r.status}</span>
                <strong>${r.requesterId === currentUser.id ? '📤 Gönderdiğim' : '📥 Gelen'} Talep #${r.id}</strong>
                <br><small>Nöbet ID: ${r.shiftId} | Talep Eden: #${r.requesterId} → Hedef: #${r.targetDoctorId}</small>
                ${isIncoming ? `<div style="margin-top:8px;display:flex;gap:8px;">
                    <button onclick="respondRequest(${r.id}, true)" class="btn-success" style="width:auto;padding:0.3rem 0.8rem;font-size:0.8rem;">✓ Kabul</button>
                    <button onclick="respondRequest(${r.id}, false)" class="btn-logout" style="padding:0.3rem 0.8rem;font-size:0.8rem;">✗ Reddet</button>
                </div>` : ''}
            </div>`;
        });
    }

    html += '</div></div>';
    document.body.insertAdjacentHTML('beforeend', html);
}

window.respondRequest = async (id, accept) => {
    const data = await apiFetch(`/ShiftRequests/${id}/respond`, {
        method: 'PUT',
        body: JSON.stringify({ accept })
    });
    if (data && data.success) {
        alert(accept ? 'Talep kabul edildi!' : 'Talep reddedildi.');
        document.getElementById('requests-overlay')?.remove();
    } else {
        alert('Hata: ' + (data?.message || 'İşlem başarısız.'));
    }
};

// ─── ÇIKIŞ ───────────────────────────────────────────────────────────
function logout() {
    localStorage.removeItem('token');
    currentUser = null;
    allShifts   = [];
    document.getElementById('dashboard-view').classList.add('hidden');
    document.getElementById('login-view').classList.remove('hidden');
    document.getElementById('login-form').classList.remove('hidden');
    document.getElementById('register-form').classList.add('hidden');
}

document.getElementById('logout-btn').addEventListener('click', logout);

// ─── SAYFA AÇILIŞINDA OTOMATİK GİRİŞ KONTROLÜ ───────────────────────
(function checkExistingSession() {
    const token = localStorage.getItem('token');
    if (token) {
        // Token var ama currentUser yok → sayfayı yenilemiş olabilir
        // Güvenli taraf: login ekranına düş (token decode edilmedi)
        localStorage.removeItem('token'); // Temiz başla
    }
})();