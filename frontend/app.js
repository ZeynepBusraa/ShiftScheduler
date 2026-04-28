document.addEventListener('DOMContentLoaded', () => {
    const loginForm = document.getElementById('login-form');
    const calendarDays = document.getElementById('calendar-days');
    const requestModal = document.getElementById('request-modal');
    const monthSelect = document.getElementById('month-select');
    const yearSelect = document.getElementById('year-select');
    
    const API_BASE_URL = 'http://localhost:5094/api';

    // Test Verileri
    let shifts = [
        { shiftDate: "2026-04-01", doctorName: "Mert Yüce", isSenior: true },
        { shiftDate: "2026-04-02", doctorName: "Dr. Ahmet", isSenior: false }
    ];

    // Geçişler
    document.getElementById('show-register').onclick = (e) => { e.preventDefault(); loginForm.classList.add('hidden'); document.getElementById('register-form').classList.remove('hidden'); };
    document.getElementById('show-login').onclick = (e) => { e.preventDefault(); document.getElementById('register-form').classList.add('hidden'); loginForm.classList.remove('hidden'); };
    document.getElementById('logout-btn').onclick = () => { localStorage.removeItem('token'); location.reload(); };
    document.getElementById('close-modal').onclick = () => requestModal.classList.add('hidden');

    loginForm.addEventListener('submit', (e) => { e.preventDefault(); localStorage.setItem('token', 'test'); showDashboard(); });

    function showDashboard() { 
        document.getElementById('login-view').classList.add('hidden'); 
        document.getElementById('dashboard-view').classList.remove('hidden'); 
        renderCalendar(shifts); 
    }

    // Seçim değişince takvimi güncelle
    monthSelect.onchange = () => renderCalendar(shifts);
    yearSelect.onchange = () => renderCalendar(shifts);

    document.getElementById('manual-add-btn').onclick = () => {
        const name = prompt("Doktor:");
        const day = prompt("Gün (1-31):");
        const m = parseInt(monthSelect.value) + 1;
        const y = parseInt(yearSelect.value);
        if(name && day) { 
            shifts.push({ shiftDate: `${y}-${m.toString().padStart(2, '0')}-${day.padStart(2, '0')}`, doctorName: name, isSenior: false }); 
            renderCalendar(shifts); 
        }
    };

    function renderCalendar(data) {
        calendarDays.innerHTML = '';
        const m = parseInt(monthSelect.value);
        const y = parseInt(yearSelect.value);
        const daysInMonth = new Date(y, m + 1, 0).getDate();

        for (let i = 1; i <= daysInMonth; i++) {
            const dayBox = document.createElement('div');
            dayBox.className = 'day-box';
            dayBox.innerHTML = `<div class="day-number">${i < 10 ? '0'+i : i}</div>`;
            
            const s = data.find(x => {
                const d = new Date(x.shiftDate);
                return d.getDate() === i && d.getMonth() === m && d.getFullYear() === y;
            });

            if(s) {
                dayBox.style.cursor = 'pointer';
                const cls = s.isSenior ? 'shift-badge shift-kidemli' : 'shift-badge shift-comez';
                dayBox.innerHTML += `<div class="${cls}">${s.doctorName}</div>`;
                dayBox.onclick = () => {
                    document.getElementById('modal-info').innerText = `${i} ${monthSelect.options[m].text} - ${s.doctorName} nöbeti için değişim talebi gönderilsin mi?`;
                    requestModal.classList.remove('hidden');
                };
            }
            calendarDays.appendChild(dayBox);
        }
    }

    if (localStorage.getItem('token')) showDashboard();
});