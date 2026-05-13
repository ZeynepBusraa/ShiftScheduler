const loginForm = document.getElementById('login-form');
let currentUser = null;

// Giriş işlemi
loginForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const emailInput = loginForm.querySelector('input[type="email"]').value;
    const passwordInput = loginForm.querySelector('input[type="password"]').value;

    try {
        const response = await fetch('http://localhost:5094/api/Auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email: emailInput, password: passwordInput })
        });

        const data = await response.json();
        console.log("Backend'den gelen ham veri:", data); // Hata ayıklama için kritik!

        if (response.ok && data.success) {
            localStorage.setItem('token', data.data.token); 
            localStorage.setItem('userEmail', emailInput);
            
            // Backend'den gelen veriyi sisteme kaydediyoruz
            // Rol bilgisi Enum'dan "Bashekim" veya "1" olarak gelebilir, ikisini de kontrol ediyoruz
            currentUser = {
                name: data.data.fullName,
                role: data.data.role, // "Bashekim", "Asistan" veya "1", "2"
                isSenior: data.data.isSenior,
                remainingChangeRequests: data.data.remainingChangeRequests || 0
            };
            
            console.log("Sisteme tanımlanan kullanıcı:", currentUser);
            
            loginForm.reset();
            showDashboard();
        } else {
            alert("Hata: " + (data.message || "Giriş başarısız."));
        }
    } catch (error) {
        console.error("Bağlantı hatası:", error);
        alert("Sunucuya bağlanılamadı.");
    }
});

function showDashboard() {
    document.getElementById('login-view').classList.add('hidden');
    document.getElementById('dashboard-view').classList.remove('hidden');

    const btnOnay = document.getElementById('btn-onay-ekrani');
    const btnNobetEkle = document.getElementById('btn-nobet-ekle'); 
    const btnDegisim = document.getElementById('btn-nobet-degisimi');
    const btnTalepler = document.getElementById('btn-taleplerim');

    const userRole = currentUser.role.toString();
    
    // Hata ayıklama için: Hüseyin ile girdiğinde konsolda ne yazdığına bakacağız
    console.log("Yetki kontrolü yapılıyor:", currentUser);

    if (userRole === "Bashekim" || userRole === "1") {
        if (btnOnay) btnOnay.style.display = 'inline-block';
        if (btnNobetEkle) btnNobetEkle.style.display = 'none';
        if (btnDegisim) btnDegisim.style.display = 'none';
        if (btnTalepler) btnTalepler.style.display = 'none';
    } 
    else if (userRole === "Asistan" || userRole === "2") {
        if (btnOnay) btnOnay.style.display = 'none';
        if (btnDegisim) btnDegisim.style.display = 'inline-block';
        if (btnTalepler) btnTalepler.style.display = 'inline-block';

        // Daha esnek bir isSenior kontrolü (Hem string hem boolean destekler)
        const isSenior = String(currentUser.isSenior).toLowerCase() === 'true';

        if (isSenior) {
            console.log("Kıdemli asistan yetkisi açıldı!");
            if (btnNobetEkle) btnNobetEkle.style.display = 'block'; 
        } else {
            console.log("Düz asistan yetkisi (Nöbet ekleme kapalı).");
            if (btnNobetEkle) btnNobetEkle.style.display = 'none'; 
        }
    }
}

// --- OTURUMU KAPATMA İŞLEMİ ---
const logoutBtn = document.getElementById('logout-btn');

if (logoutBtn) {
    logoutBtn.addEventListener('click', () => {
        // 1. Ceplerimizdeki anahtarları (Token ve Mail) çöpe atıyoruz
        localStorage.removeItem('token');
        localStorage.removeItem('userEmail');
        
        // 2. Kimliği sıfırlıyoruz
        currentUser = null;

        // 3. Hastaneden (Dashboard) çıkıp Kapı Önüne (Login) dönüyoruz
        document.getElementById('dashboard-view').classList.add('hidden');
        document.getElementById('login-view').classList.remove('hidden');
    });
}