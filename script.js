// Tafsilk Platform - Frontend JavaScript
// Changelog:
// - Removed duplicated listeners and consolidated navigation, language, and search logic
// - Added accessible toast notifications (role=status, aria-live) using .toast styles (no inline styles)
// - Implemented accessible video modal (role=dialog, aria-modal, focus trap, ESC/overlay close)
// - Converted quick-filters to buttons with aria-pressed + delegation
// - Added input sanitization and safer DOM handling; avoided inline style mutations
// - Kept Arabic-first RTL support and translation system

/** @type {"ar"|"en"} */
let currentLanguage = "ar";
/** @type {Record<string, Record<string,string>>} */
let translations = {};

// Boot
document.addEventListener("DOMContentLoaded", function () {
  initTranslationSystem()
    .then(() => {
      initMobileNavigation();
      initLanguageToggle();
      initSearchFunctionality();
      initLocationServices();
      initLazyLoading();
      initScrollAnimations();
      initTailorCarousel();
      initFormValidation();
      initNewHeroSection();
      tryRegisterServiceWorker();
      console.log("Tafsilk platform loaded successfully");
    })
    .catch(() => {
      initMobileNavigation();
      initLanguageToggle();
      initSearchFunctionality();
      initLocationServices();
      initLazyLoading();
      initScrollAnimations();
      initTailorCarousel();
      initFormValidation();
      initNewHeroSection();
      tryRegisterServiceWorker();
    });
});

// Translation System
async function initTranslationSystem() {
  const response = await fetch("./locales.json");
  if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
  translations = await response.json();
  currentLanguage = localStorage.getItem("tafsilk_language") || "ar";
  applyLanguage(currentLanguage);
}

/**
 * @param {string} key
 * @returns {string}
 */
function translate(key) {
  const pack = translations[currentLanguage];
  return (pack && pack[key]) || key;
}

/** @param {"ar"|"en"} lang */
function applyLanguage(lang) {
  currentLanguage = lang;
  document.documentElement.lang = lang;
  document.documentElement.dir = lang === "ar" ? "rtl" : "ltr";
  updateTranslatableElements();
  localStorage.setItem("tafsilk_language", lang);
}

function updateTranslatableElements() {
  document.querySelectorAll("[data-translate]").forEach((el) => {
    const key = el.getAttribute("data-translate");
    if (key) el.textContent = translate(key);
  });
  document.querySelectorAll("[data-translate-placeholder]").forEach((el) => {
    const key = el.getAttribute("data-translate-placeholder");
    if (key && /** @type {HTMLInputElement} */ (el).placeholder !== undefined) {
      /** @type {HTMLInputElement} */ (el).placeholder = translate(key);
    }
  });
  document.querySelectorAll("[data-translate-aria]").forEach((el) => {
    const key = el.getAttribute("data-translate-aria");
    if (key) el.setAttribute("aria-label", translate(key));
  });
  document.querySelectorAll("[data-translate-alt]").forEach((el) => {
    const key = el.getAttribute("data-translate-alt");
    if (key) el.setAttribute("alt", translate(key));
  });
}

// Language Toggle
function initLanguageToggle() {
  document.querySelectorAll(".lang-toggle").forEach((btn) => {
    btn.addEventListener("click", () => {
      const newLang = currentLanguage === "ar" ? "en" : "ar";
      switchLanguage(newLang);
    });
  });
}

/** @param {"ar"|"en"} lang */
function switchLanguage(lang) {
  applyLanguage(lang);
  const text = lang === "ar" ? "العربية / English" : "English / العربية";
  document.querySelectorAll(".lang-toggle span").forEach((span) => {
    span.textContent = text;
  });
  showToast(lang === "ar" ? translate("toast.language.ar") : translate("toast.language.en"), "info");
}

// Mobile Navigation
function initMobileNavigation() {
  const navToggle = document.querySelector(".nav-toggle");
  const navMenu = document.getElementById("nav-menu");
  if (!navToggle || !navMenu) return;

  let overlay = document.querySelector(".nav-overlay");
  if (!overlay) {
    overlay = document.createElement("div");
    overlay.className = "nav-overlay";
    document.body.appendChild(overlay);
  }

  function openNav() {
    document.body.classList.add("drawer-open");
    navMenu.classList.add("active");
    navToggle.classList.add("active");
    navToggle.setAttribute("aria-expanded", "true");
  }
  function closeNav() {
    document.body.classList.remove("drawer-open");
    navMenu.classList.remove("active");
    navToggle.classList.remove("active");
    navToggle.setAttribute("aria-expanded", "false");
  }

  navToggle.addEventListener("click", () => {
    const open = document.body.classList.contains("drawer-open");
    open ? closeNav() : openNav();
  });
  overlay.addEventListener("click", closeNav);
  document.addEventListener("keydown", (e) => {
    if (e.key === "Escape") closeNav();
  });

  // In-page nav
  document.querySelectorAll(".nav-link").forEach((link) => {
    link.addEventListener("click", (e) => {
      const targetId = link.getAttribute("href") || "";
      let targetSection = null;
      if (targetId && targetId.startsWith("#") && targetId.length > 1) {
        try { targetSection = document.querySelector(targetId); } catch (_) { targetSection = null; }
      }
      if (targetSection) {
        e.preventDefault();
        targetSection.scrollIntoView({ behavior: "smooth" });
      }
      closeNav();
    });
  });
}

// Search
function initSearchFunctionality() {
  const form = document.getElementById("searchForm");
  const searchInput = document.querySelector(".search-input") || document.getElementById("locationInput");
  const useLocationBtn = document.getElementById("useLocationBtn");
  if (!form) return;

  form.addEventListener("submit", (e) => {
    e.preventDefault();
    const value = sanitizeInput((searchInput && /** @type {HTMLInputElement} */(searchInput).value) || "").trim();
    if (value.length < 2) {
      showToast("يرجى إدخال كلمتين على الأقل للبحث", "warning");
      updateLiveStatus("searchStatus", translate("toast.searching"));
      return;
    }
    performSearch(value);
  });

  if (useLocationBtn) {
    useLocationBtn.addEventListener("click", () => getCurrentLocation());
  }
}

// Location Services
function initLocationServices() {
  if (!navigator.geolocation) {
    const locationBtn = document.getElementById("useLocationBtn");
    if (locationBtn) {
      locationBtn.setAttribute("disabled", "true");
      locationBtn.title = "تحديد الموقع غير مدعوم في متصفحك";
    }
  }
}

function getCurrentLocation() {
  const locationBtn = document.getElementById("useLocationBtn");
  if (!locationBtn || !navigator.geolocation) return;

  const original = locationBtn.innerHTML;
  locationBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> <span>جاري تحديد الموقع...</span>';
  locationBtn.setAttribute("disabled", "true");

  navigator.geolocation.getCurrentPosition(
    (position) => {
      reverseGeocode(position.coords.latitude, position.coords.longitude)
        .then((address) => {
          const locationField = /** @type {HTMLInputElement|null} */ (document.getElementById("locationInput"));
          const hiddenSearchInput = /** @type {HTMLInputElement|null} */ (document.querySelector(".search-input"));
          if (locationField) {
            locationField.value = address;
            locationField.placeholder = "تم تحديد موقعك بنجاح...";
          }
          if (hiddenSearchInput) hiddenSearchInput.value = address;
          showToast(translate("toast.location.success"), "success");
          updateLiveStatus("searchStatus", translate("toast.location.success"));
          locationBtn.innerHTML = '<i class="fas fa-check"></i> <span>تم تحديد الموقع</span>';
        })
        .catch(() => {
          showToast("لم نتمكن من تحديد عنوانك الدقيق", "warning");
          locationBtn.innerHTML = original;
        })
        .finally(() => locationBtn.removeAttribute("disabled"));
    },
    (error) => {
      let message = translate("toast.location.fail");
      if (error.code === error.PERMISSION_DENIED) message = translate("toast.location.permission");
      else if (error.code === error.POSITION_UNAVAILABLE) message = translate("toast.location.unavailable");
      else if (error.code === error.TIMEOUT) message = translate("toast.location.timeout");
      showToast(message, "error");
      updateLiveStatus("searchStatus", message);
      locationBtn.innerHTML = original;
      locationBtn.removeAttribute("disabled");
    },
    { enableHighAccuracy: true, timeout: 10000, maximumAge: 60000 }
  );
}

async function reverseGeocode(lat, lng) {
  try {
    const res = await fetch(`https://api.bigdatacloud.net/data/reverse-geocode-client?latitude=${lat}&longitude=${lng}&localityLanguage=${currentLanguage}`);
    const data = await res.json();
    return data.city || data.locality || (currentLanguage === "ar" ? "موقعك الحالي" : "Your current location");
  } catch {
    return currentLanguage === "ar" ? "موقعك الحالي" : "Your current location";
  }
}

function performSearch(query) {
  const submitBtn = document.querySelector('#searchForm button[type="submit"]');
  if (submitBtn) {
    const original = submitBtn.innerHTML;
    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> <span>' + translate("toast.searching") + '</span>';
    setTimeout(() => {
      submitBtn.innerHTML = original;
      const msg = currentLanguage === "ar" ? "تم العثور على 15 خياط في منطقتك!" : "Found 15 tailors near you!";
      showToast(msg, "success");
      updateLiveStatus("searchStatus", msg);
    }, 1200);
  }
}

// Lazy Loading
function initLazyLoading() {
  const images = document.querySelectorAll('img[loading="lazy"]');
  if ("IntersectionObserver" in window) {
    const io = new IntersectionObserver((entries, observer) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          const img = /** @type {HTMLImageElement} */ (entry.target);
          img.src = img.dataset.src || img.src;
          img.classList.add("loaded");
          observer.unobserve(img);
        }
      });
    }, { rootMargin: "50px" });
    images.forEach((img) => io.observe(img));
  } else {
    images.forEach((img) => img.classList.add("loaded"));
  }
}

// Scroll Animations
function initScrollAnimations() {
  const els = document.querySelectorAll(".animate-on-scroll");
  if ("IntersectionObserver" in window) {
    const io = new IntersectionObserver((entries) => {
      entries.forEach((entry) => { if (entry.isIntersecting) entry.target.classList.add("animate-fade-in"); });
    }, { threshold: 0.1 });
    els.forEach((el) => io.observe(el));
  }
  document.querySelectorAll('a[href^="#"]:not(.nav-link)').forEach((a) => {
    a.addEventListener("click", (e) => {
      const id = a.getAttribute("href") || "";
      if (!id || id === "#" || id.length < 2) return; // ignore placeholders
      e.preventDefault();
      let el = null;
      try { el = document.querySelector(id); } catch (_) { el = null; }
      if (el) el.scrollIntoView({ behavior: "smooth", block: "start" });
    });
  });
}

// Carousel
function initTailorCarousel() {
  const carousel = document.querySelector(".tailors-carousel");
  if (!carousel) return;
  let startX = 0; let scrollLeft = 0; let isDown = false;
  carousel.addEventListener("mousedown", (e) => { isDown = true; startX = e.pageX - carousel.offsetLeft; scrollLeft = carousel.scrollLeft; });
  carousel.addEventListener("mouseleave", () => { isDown = false; });
  carousel.addEventListener("mouseup", () => { isDown = false; });
  carousel.addEventListener("mousemove", (e) => { if (!isDown) return; e.preventDefault(); const x = e.pageX - carousel.offsetLeft; carousel.scrollLeft = scrollLeft - (x - startX) * 2; });
  carousel.addEventListener("touchstart", (e) => { startX = e.touches[0].pageX - carousel.offsetLeft; scrollLeft = carousel.scrollLeft; });
  carousel.addEventListener("touchmove", (e) => { if (!startX) return; const x = e.touches[0].pageX - carousel.offsetLeft; carousel.scrollLeft = scrollLeft - (x - startX) * 2; });
}

// Forms
function initFormValidation() {
  document.querySelectorAll("form").forEach((form) => {
    form.addEventListener("submit", (e) => { if (!validateForm(form)) e.preventDefault(); });
    form.querySelectorAll("input, textarea, select").forEach((input) => {
      input.addEventListener("blur", () => validateField(/** @type {HTMLInputElement} */(input)) );
    });
  });
}

function validateForm(form) {
  const inputs = form.querySelectorAll("input[required], textarea[required], select[required]");
  let ok = true; inputs.forEach((i) => { if (!validateField(/** @type {HTMLInputElement} */(i))) ok = false; });
  return ok;
}

function validateField(field) {
  const value = field.value.trim();
  let isValid = true; let message = "";
  removeFieldError(field);
  if (field.hasAttribute("required") && !value) { isValid = false; message = currentLanguage === "ar" ? "هذا الحقل مطلوب" : "This field is required"; }
  else if (value) {
    switch (field.type) {
      case "email": if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) { isValid = false; message = currentLanguage === "ar" ? "يرجى إدخال بريد إل��تروني صحيح" : "Enter a valid email"; } break;
      case "tel": if (!/^[\+]?[0-9\s\-\(\)]{10,}$/.test(value)) { isValid = false; message = currentLanguage === "ar" ? "يرجى إدخال رقم هاتف صحيح" : "Enter a valid phone"; } break;
    }
  }
  if (!isValid) showFieldError(field, message);
  return isValid;
}

function showFieldError(field, message) {
  field.classList.add("error");
  const div = document.createElement("div");
  div.className = "field-error";
  div.textContent = message;
  field.parentNode && field.parentNode.appendChild(div);
}

function removeFieldError(field) {
  field.classList.remove("error");
  const err = field.parentNode && field.parentNode.querySelector(".field-error");
  if (err) err.remove();
}

// Toasts + Live region
function showToast(text, type = "info") {
  document.querySelectorAll(".toast").forEach((t) => t.remove());
  const toast = document.createElement("div");
  toast.className = `toast ${type === "success" ? "success" : type === "error" ? "error" : type === "warning" ? "warning" : ""}`;
  toast.setAttribute("role", "status");
  toast.setAttribute("aria-live", "polite");
  toast.innerHTML = `<span>${escapeHtml(text)}</span>`;
  document.body.appendChild(toast);
  setTimeout(() => { if (toast.parentNode) toast.parentNode.removeChild(toast); }, 4000);
}

function updateLiveStatus(id, message) {
  const el = document.getElementById(id);
  if (el) el.textContent = message;
}

// Hero Enhancements
function initNewHeroSection() {
  const videoThumb = document.querySelector(".video-thumbnail");
  if (videoThumb) {
    const open = () => showVideoModal(videoThumb);
    videoThumb.addEventListener("click", open);
    videoThumb.addEventListener("keydown", (e) => { if (e.key === "Enter" || e.key === " ") { e.preventDefault(); open(); } });
  }

  const filters = document.querySelector(".search-quick-filters");
  if (filters) {
    filters.addEventListener("click", (e) => {
      const target = /** @type {HTMLElement} */(e.target instanceof HTMLElement ? (e.target.closest(".quick-filter")) : null);
      if (!target) return;
      filters.querySelectorAll(".quick-filter").forEach((b) => { b.classList.remove("active"); b.setAttribute("aria-pressed", "false"); });
      target.classList.add("active");
      target.setAttribute("aria-pressed", "true");
      const type = target.getAttribute("data-filter") || "all";
      updateSearchFilter(type);
    });
  }

  // keep hidden input synced
  const locationInput = /** @type {HTMLInputElement|null} */ (document.getElementById("locationInput"));
  const hiddenSearchInput = /** @type {HTMLInputElement|null} */ (document.querySelector(".search-input"));
  if (locationInput && hiddenSearchInput) {
    const sync = () => (hiddenSearchInput.value = locationInput.value);
    locationInput.addEventListener("input", sync);
    sync();
  }
}

/** @param {HTMLElement} trigger */
function showVideoModal(trigger) {
  const existing = document.querySelector(".video-modal");
  if (existing) existing.remove();

  const modal = document.createElement("div");
  modal.className = "video-modal";
  modal.setAttribute("role", "dialog");
  modal.setAttribute("aria-modal", "true");
  modal.innerHTML = `
    <div class="modal-content" tabindex="-1">
      <button class="close-modal" aria-label="إغلاق">&times;</button>
      <video controls autoplay>
        <source src="video.mp4" type="video/mp4" />
      </video>
    </div>
  `;
  document.body.appendChild(modal);

  const content = /** @type {HTMLElement} */ (modal.querySelector(".modal-content"));
  const closeBtn = /** @type {HTMLButtonElement} */ (modal.querySelector(".close-modal"));

  // Focus trapping
  /** @type {HTMLElement[]} */
  let focusables = Array.from(modal.querySelectorAll("button, [href], input, select, textarea, [tabindex]:not([tabindex='-1'])"));
  const first = focusables[0];
  const last = focusables[focusables.length - 1];
  function trap(e) {
    if (e.key !== "Tab") return;
    if (e.shiftKey && document.activeElement === first) { e.preventDefault(); last.focus(); }
    else if (!e.shiftKey && document.activeElement === last) { e.preventDefault(); first.focus(); }
  }
  function close() {
    modal.remove();
    if (trigger instanceof HTMLElement) trigger.focus();
    document.removeEventListener("keydown", escClose);
    document.removeEventListener("keydown", trap);
  }
  function escClose(e) { if (e.key === "Escape") close(); }

  modal.addEventListener("click", (e) => { if (e.target === modal) close(); });
  document.addEventListener("keydown", escClose);
  document.addEventListener("keydown", trap);
  closeBtn.addEventListener("click", close);
  setTimeout(() => content.focus(), 0);
}

function updateSearchFilter(filterType) { console.log("Filter updated to:", filterType); }

// Utils
function sanitizeInput(value) { return value.replace(/[<>"'`;(){}]/g, ""); }
function escapeHtml(s) {
  return s.replace(/[&<>"]/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;" }[c]));
}

function tryRegisterServiceWorker() {
  if ("serviceWorker" in navigator) {
    navigator.serviceWorker.register("sw.js").catch(() => {/* optional */});
  }
}

// Exported API
window.TafsilkPlatform = {
  showToast,
  performSearch,
  getCurrentLocation,
  switchLanguage,
  translate,
  currentLanguage: () => currentLanguage,
};

// Global error logging
window.addEventListener("error", (e) => console.error("JavaScript Error:", e.error));
