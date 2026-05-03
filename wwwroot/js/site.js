// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("submit", async (e) => {
    const form = e.target;
    if (!form.classList.contains("add-to-cart-form")) return;

    e.preventDefault();

    const url = form.getAttribute("action");
    const formData = new FormData(form);

    const tokenInput = form.querySelector('input[name="__RequestVerificationToken"]');
    const token = tokenInput ? tokenInput.value : "";

    try {
        const res = await fetch(url, {
            method: "POST",
            headers: {
                "X-Requested-With": "XMLHttpRequest",
                "RequestVerificationToken": token
            },
            body: formData
        });

        if (!res.ok) return showToast("Bir hata oluştu ❌");

        const data = await res.json();
        if (!data?.success) return showToast("Bir hata oluştu ❌");

        const badge = document.getElementById("cartBadge");
        if (badge) {
            badge.textContent = data.cartCount;
            badge.style.display = data.cartCount > 0 ? "inline-block" : "none";
            badge.classList.remove("badge-pop");
            void badge.offsetWidth;
            badge.classList.add("badge-pop");
        }

        showToast(data.message || "Sepete eklendi ✅");

        async function loadCartPanel() {
            const body = document.getElementById("cartPanelBody");
            const totalEl = document.getElementById("cartPanelTotal");

            if (!body || !totalEl) return;

            body.innerHTML = `<div class="text-muted">Sepet yükleniyor...</div>`;

            try {
                const res = await fetch("/Cart/GetCartSummary", {
                    method: "GET",
                    cache: "no-store"
                });

                if (!res.ok) {
                    body.innerHTML = `<div class="cart-panel-empty">Sepet yüklenemedi.</div>`;
                    return;
                }

                const data = await res.json();

                const items = data.items || data.Items || [];
                const grandTotal = data.grandTotal || data.GrandTotal || 0;

                if (!items || items.length === 0) {
                    body.innerHTML = `
                <div class="cart-panel-empty">
                    <div style="font-size:42px;">🛒</div>
                    <h6 class="mt-2">Sepetin boş</h6>
                    <p class="mb-0">Ürün ekleyerek başlayabilirsin.</p>
                </div>
            `;
                    totalEl.textContent = "0 ₺";
                    return;
                }

                body.innerHTML = items.map(item => {
                    const productName = item.productName || item.ProductName;
                    const quantity = item.quantity || item.Quantity;
                    const price = item.price || item.Price;
                    const total = item.total || item.Total;

                    return `
                <div class="cart-panel-item">
                    <div>
                        <div class="cart-panel-item-name">${productName}</div>
                        <div class="cart-panel-item-meta">${quantity} adet x ${price} ₺</div>
                    </div>
                    <div class="fw-bold">${total} ₺</div>
                </div>
            `;
                }).join("");

                totalEl.textContent = `${grandTotal} ₺`;
            } catch {
                body.innerHTML = `<div class="cart-panel-empty">Bağlantı hatası.</div>`;
            }
        }
    } catch {
        showToast("Bağlantı hatası ❌");
    }
});

function showToast(message) {
    const toastEl = document.getElementById("appToast");
    const toastMsg = document.getElementById("appToastMessage");
    if (!toastEl || !toastMsg) return;

    toastMsg.textContent = message;
    const toast = bootstrap.Toast.getOrCreateInstance(toastEl, { delay: 2000 });
    toast.show();
}

window.addEventListener("load", () => {
    const loader = document.getElementById("pageLoader");
    if (loader) {
        loader.style.opacity = "0";
        loader.style.transition = "opacity .25s ease";

        setTimeout(() => {
            loader.style.display = "none";
        }, 250);
    }
});

async function openCartPanel() {
    const overlay = document.getElementById("cartOverlay");
    const panel = document.getElementById("cartPanel");

    if (!overlay || !panel) return;

    overlay.classList.add("show");
    panel.classList.add("show");

    await loadCartPanel();
}

function closeCartPanel() {
    const overlay = document.getElementById("cartOverlay");
    const panel = document.getElementById("cartPanel");

    if (!overlay || !panel) return;

    overlay.classList.remove("show");
    panel.classList.remove("show");
}

async function loadCartPanel() {
    const body = document.getElementById("cartPanelBody");
    const totalEl = document.getElementById("cartPanelTotal");

    if (!body || !totalEl) return;

    body.innerHTML = `<div class="text-muted">Sepet yükleniyor...</div>`;

    try {
        const res = await fetch("/Cart/GetCartSummary");

        if (!res.ok) {
            body.innerHTML = `<div class="cart-panel-empty">Sepet yüklenemedi.</div>`;
            return;
        }

        const data = await res.json();

        if (!data.items || data.items.length === 0) {
            body.innerHTML = `
                <div class="cart-panel-empty">
                    <div style="font-size:42px;">🛒</div>
                    <h6 class="mt-2">Sepetin boş</h6>
                    <p class="mb-0">Ürün ekleyerek başlayabilirsin.</p>
                </div>
            `;
            totalEl.textContent = "0 ₺";
            return;
        }

        body.innerHTML = data.items.map(item => `
            <div class="cart-panel-item">
                <div>
                    <div class="cart-panel-item-name">${item.productName}</div>
                    <div class="cart-panel-item-meta">${item.quantity} adet x ${item.price} ₺</div>
                </div>
                <div class="fw-bold">${item.total} ₺</div>
            </div>
        `).join("");

        totalEl.textContent = `${data.grandTotal} ₺`;
    } catch {
        body.innerHTML = `<div class="cart-panel-empty">Bağlantı hatası.</div>`;
    }
}
