//Route: /Auth/Login
const API = window.API_BASE ?? 'https://localhost:7157'

// Util: ler cookie
function getCookie(name) {
    const m = document.cookie.match(new RegExp('(?:^|; )' + name.replace(/([.$?*|{}()[\]\\/+^])/g, '\\$1') + '=([^;]*)'));
    return m ? decodeURIComponent(m[1]) : null;
}

// i18n simples via cookie 'lang'
function langIsPt() { return (getCookie('lang') || 'pt').toLowerCase() === 'pt'; }
function t(pt, en) { return langIsPt() ? pt : en; }

// Troca de idioma: grava cookie e recarrega
document.querySelectorAll('[data-lang]').forEach(btn => {
    btn.addEventListener('click', () => {
        const v = btn.getAttribute('data-lang');
        const days = 365;
        const d = new Date(Date.now() + days * 24 * 60 * 60 * 1000).toUTCString();
        document.cookie = `lang=${v === 'pt' ? 'pt' : 'en'}; expires=${d}; path=/; SameSite=Lax`;
        location.reload();
    });
});

// Toggle de senha
document.getElementById('togglePwd')?.addEventListener('click', function () {
    const inp = document.getElementById('pass');
    const icon = this.querySelector('i');
    if (!inp) return;
    const isPwd = inp.type === 'password';
    inp.type = isPwd ? 'text' : 'password';
    if (icon) {
        icon.classList.toggle('bi-eye');
        icon.classList.toggle('bi-eye-slash');
    }
});

// Helpers
const $ = (id) => document.getElementById(id);

// Login
async function doLogin() {
    const user = $('user').value.trim();
    const pass = $('pass').value.trim();
    if (!user || !pass) {
        Swal.fire({ icon: 'warning', title: t('Ops', 'Oops'), text: t('Preencha usuário e senha.', 'Fill username and password.') });
        return;
    }

    try {
        const API = window.API_BASE ?? 'https://localhost:7157'; // <-- use https da API
        const res = await fetch(`${API}/api/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            // credentials: 'include', // só se precisar enviar cookies
            body: JSON.stringify({ userName: user, password: pass })
        });

        if (!res.ok) {
            let msg = t('Credenciais inválidas', 'Invalid credentials');
            try { const err = await res.json(); if (err?.message || err?.error) msg = err.message || err.error; } catch { }
            throw new Error(msg);
        }

        const data = await res.json();
        const remember = $('remember').checked;
        const payload = {
            token: data.token,
            displayName: data.displayName,
            exp: new Date(Date.now() + (remember ? 30 * 24 * 3600 * 1000 : 12 * 3600 * 1000)).toISOString()
        };

        if (remember) { localStorage.setItem('auth', JSON.stringify(payload)); sessionStorage.removeItem('auth'); }
        else { sessionStorage.setItem('auth', JSON.stringify(payload)); localStorage.removeItem('auth'); }

        Swal.fire({ icon: 'success', title: t('OK', 'OK'), text: t('Login efetuado!', 'Signed in!') })
            .then(() => location.href = '/Index');

    } catch (e) {
        Swal.fire({ icon: 'error', title: t('Erro', 'Error'), text: e.message || t('Falha no login', 'Login failed') });
    }
}

$('btnLogin').addEventListener('click', doLogin);

// Enter para enviar
document.addEventListener('keydown', (ev) => {
    if (ev.key === 'Enter') {
        ev.preventDefault();
        doLogin();
    }
});
