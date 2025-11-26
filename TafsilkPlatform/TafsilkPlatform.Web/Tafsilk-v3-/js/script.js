// Tafsilk Platform - Frontend JavaScript
// Mobile-first, Arabic RTL support with translation functionality

// Global variables for translation
let currentLanguage = "ar";
let translations = {};

function waitForPartialsLoaded() {
  return new Promise((resolve) => {
    if (window.__partialsLoaded) {
      resolve();
      return;
    }
    document.addEventListener("partials:loaded", () => resolve(), { once: true });
    setTimeout(() => resolve(), 1500);
  });
}

document.addEventListener("DOMContentLoaded", async function () {
  await waitForPartialsLoaded();
  // Initialize translation system first
  initTranslationSystem()
    .then(() => {
      // Initialize all other components
      initMobileNavigation();
      initSearchFunctionality();
      initLocationServices();
      initLazyLoading();
      initScrollAnimations();
      initTailorCarousel();
      initFormValidation();
      initLanguageToggle();

      console.log("Tafsilk platform loaded successfully");
    })
    .catch((error) => {
      console.error("Failed to load translations:", error);
      // Continue with default language if translation loading fails
      initMobileNavigation();
      initSearchFunctionality();
      initLocationServices();
      initLazyLoading();
      initScrollAnimations();
      initTailorCarousel();
      initFormValidation();
      initLanguageToggle();
    });
});

// Translation System
async function initTranslationSystem() {
  try {
    // Load translations from locales.json
    const response = await fetch("./locales.json");
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    translations = await response.json();

    // Get saved language from localStorage or use default
    currentLanguage = localStorage.getItem("tafsilk_language") || "ar";

    // Apply initial language
    applyLanguage(currentLanguage);

    console.log("Translation system initialized successfully");
  } catch (error) {
    console.error("Error loading translations:", error);
    // Fallback to Arabic if translation loading fails
    currentLanguage = "ar";
    throw error;
  }
}

function translate(key) {
  try {
    if (!translations[currentLanguage]) {
      console.warn(`No translations found for language: ${currentLanguage}`);
      return key;
    }

    const translation = translations[currentLanguage][key];
    if (!translation) {
      console.warn(
        `Translation key not found: ${key} for language: ${currentLanguage}`
      );
      return key;
    }

    return translation;
  } catch (error) {
    console.error("Translation error:", error);
    return key;
  }
}

function applyLanguage(lang) {
  currentLanguage = lang;

  // Update HTML attributes
  document.documentElement.lang = lang;
  document.documentElement.dir = lang === "ar" ? "rtl" : "ltr";

  // Update all translatable elements
  updateTranslatableElements();

  // Update text alignment based on language
  updateTextAlignment(lang);

  // Save language preference
  localStorage.setItem("tafsilk_language", lang);

  console.log(`Language switched to: ${lang}`);
}

function updateTranslatableElements() {
  // Update elements with data-translate attribute
  const translatableElements = document.querySelectorAll("[data-translate]");
  translatableElements.forEach((element) => {
    const key = element.getAttribute("data-translate");
    element.textContent = translate(key);
  });

  // Update placeholders
  const placeholderElements = document.querySelectorAll(
    "[data-translate-placeholder]"
  );
  placeholderElements.forEach((element) => {
    const key = element.getAttribute("data-translate-placeholder");
    element.placeholder = translate(key);
  });

  // Update aria-labels
  const ariaElements = document.querySelectorAll("[data-translate-aria]");
  ariaElements.forEach((element) => {
    const key = element.getAttribute("data-translate-aria");
    element.setAttribute("aria-label", translate(key));
  });

  // Update alt attributes for images
  const altElements = document.querySelectorAll("[data-translate-alt]");
  altElements.forEach((element) => {
    const key = element.getAttribute("data-translate-alt");
    element.setAttribute("alt", translate(key));
  });
}

function updateTextAlignment(lang) {
  const isRTL = lang === "ar";

  // Update container alignment
  document.body.style.textAlign = isRTL ? "right" : "left";

  // Update specific elements that need special handling
  const searchInput = document.querySelector(".search-input");
  if (searchInput) {
    searchInput.style.textAlign = isRTL ? "right" : "left";
  }

  // Update navigation alignment
  const navMenu = document.querySelector(".nav-menu");
  if (navMenu) {
    navMenu.style.direction = isRTL ? "rtl" : "ltr";
  }
}

// Language Toggle Functionality
function initLanguageToggle() {
  const languageToggles = document.querySelectorAll(".lang-toggle");

  languageToggles.forEach((toggle) => {
    toggle.addEventListener("click", function () {
      const newLang = currentLanguage === "ar" ? "en" : "ar";
      switchLanguage(newLang);
    });
  });
}

function switchLanguage(lang) {
  applyLanguage(lang);

  // Show appropriate message
  const messageKey =
    lang === "ar" ? "تم التبديل للعربية" : "Switched to English";
  showMessage(messageKey, "info");

  // Update language toggle buttons
  const languageToggles = document.querySelectorAll(".lang-toggle");
  languageToggles.forEach((toggle) => {
    const newText = lang === "ar" ? "العربية / English" : "English / العربية";
    const span = toggle.querySelector("span");
    if (span) {
      span.textContent = newText;
    }
  });
}

// Mobile Navigation Toggle
function initMobileNavigation() {
  const navToggle = document.querySelector(".nav-toggle");
  const navMenu = document.querySelector(".nav-menu");
  const navActions = document.querySelector(".nav-actions");

  if (!navToggle || !navMenu) return;

  // Create overlay once
  let overlay = document.querySelector(".nav-overlay");
  if (!overlay) {
    overlay = document.createElement("div");
    overlay.className = "nav-overlay";
    document.body.appendChild(overlay);
  }
  overlay.addEventListener("click", () => closeNavMenu());

  navToggle.addEventListener("click", function () {
    const isOpen = document.body.classList.contains("drawer-open");
    if (isOpen) {
      closeNavMenu();
    } else {
      openNavMenu();
    }
  });

  document.addEventListener("keydown", function (e) {
    if (e.key === "Escape") closeNavMenu();
  });

  const navLinks = document.querySelectorAll(".nav-link");
  navLinks.forEach((link) => {
    link.addEventListener("click", function (e) {
      const href = this.getAttribute("href") || "";
      // Intercept same-page hash links only
      if (href.startsWith("#")) {
        e.preventDefault();
        const targetId = href.slice(1);
        const targetSection = document.getElementById(targetId);
        if (targetSection) {
          targetSection.scrollIntoView({ behavior: "smooth" });
        }
      }
      // Always close the drawer if open
      closeNavMenu();
    });
  });

  function openNavMenu() {
    document.body.classList.add("drawer-open");
    navMenu.classList.add("active");
    if (navActions) navActions.classList.add("active");
    navToggle.classList.add("active");
    navToggle.setAttribute("aria-expanded", "true");
    document.body.style.overflow = "hidden";
  }

  function closeNavMenu() {
    document.body.classList.remove("drawer-open");
    navMenu.classList.remove("active");
    if (navActions) navActions.classList.remove("active");
    navToggle.classList.remove("active");
    navToggle.setAttribute("aria-expanded", "false");
    document.body.style.overflow = "";
  }
}

// Search Functionality
function initSearchFunctionality() {
  const searchForm = document.querySelector(".search-form");
  const searchInput = document.querySelector(".search-input");
  const useLocationBtn = document.getElementById("useLocationBtn");

  if (!searchForm) return;

  // Handle search form submission
  searchForm.addEventListener("submit", function (e) {
    e.preventDefault();
    const query = searchInput.value.trim();

    if (query.length < 2) {
      showMessage("يرجى إدخال كلمتين على الأقل للبحث", "warning");
      return;
    }

    performSearch(query);
  });

  // Location button functionality
  if (useLocationBtn) {
    useLocationBtn.addEventListener("click", function () {
      getCurrentLocation();
    });
  }
}

// Location Services
function initLocationServices() {
  // Check if geolocation is supported
  if (!navigator.geolocation) {
    console.warn("Geolocation is not supported by this browser");
    const locationBtn = document.getElementById("useLocationBtn");
    if (locationBtn) {
      locationBtn.disabled = true;
      locationBtn.title = "تحديد الموقع غير مدعوم في متصفحك";
    }
    return;
  }
}

function getCurrentLocation() {
  const locationBtn = document.getElementById("useLocationBtn");
  if (!locationBtn) return;

  const originalContent = locationBtn.innerHTML;
  locationBtn.innerHTML =
    '<i class="fas fa-spinner fa-spin"></i> <span>جاري تحديد الموقع...</span>';
  locationBtn.disabled = true;

  navigator.geolocation.getCurrentPosition(
    function (position) {
      const lat = position.coords.latitude;
      const lng = position.coords.longitude;

      // Use reverse geocoding to get address
      reverseGeocode(lat, lng)
        .then((address) => {
          const searchInput = document.querySelector(".search-input");
          if (searchInput) {
            searchInput.value = address;
            searchInput.placeholder = "تم تحديد موقعك بنجاح...";
          }
          showMessage("تم تحديد موقعك بنجاح", "success");
          locationBtn.innerHTML =
            '<i class="fas fa-check"></i> <span>تم تحديد الموقع</span>';
        })
        .catch((error) => {
          console.error("Reverse geocoding error:", error);
          showMessage("لم نتمكن من تحديد عنوانك الدقيق", "warning");
          locationBtn.innerHTML = originalContent;
        })
        .finally(() => {
          locationBtn.disabled = false;
        });
    },
    function (error) {
      let message = "فشل في تحديد الموقع";

      switch (error.code) {
        case error.PERMISSION_DENIED:
          message = "تم رفض إذن الموقع";
          break;
        case error.POSITION_UNAVAILABLE:
          message = "الموقع غير متاح";
          break;
        case error.TIMEOUT:
          message = "انتهت مهلة تحديد الموقع";
          break;
      }

      showMessage(message, "error");
      locationBtn.innerHTML = originalContent;
      locationBtn.disabled = false;
    },
    {
      enableHighAccuracy: true,
      timeout: 10000,
      maximumAge: 60000,
    }
  );
}

// Reverse Geocoding
async function reverseGeocode(lat, lng) {
  try {
    const response = await fetch(
      `https://api.bigdatacloud.net/data/reverse-geocode-client?latitude=${lat}&longitude=${lng}&localityLanguage=ar`
    );
    const data = await response.json();
    return data.city || data.locality || "موقعك الحالي";
  } catch (error) {
    console.error("Geocoding error:", error);
    return "موقعك الحالي";
  }
}

// Search Functions
function performSearch(query) {
  console.log("Searching for:", query);
  const searchBtn = document.querySelector(
    '.search-form button[type="submit"]'
  );

  if (searchBtn) {
    const originalContent = searchBtn.innerHTML;
    searchBtn.innerHTML =
      '<i class="fas fa-spinner fa-spin"></i> <span>جاري البحث...</span>';

    setTimeout(() => {
      searchBtn.innerHTML = originalContent;
      showMessage("تم العثور على 15 خياط في منطقتك!", "success");
    }, 2000);
  }
}

// Lazy Loading for Images
function initLazyLoading() {
  const images = document.querySelectorAll('img[loading="lazy"]');

  if ("IntersectionObserver" in window) {
    const imageObserver = new IntersectionObserver(
      (entries, observer) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            const img = entry.target;
            img.src = img.dataset.src || img.src;
            img.classList.add("loaded");
            observer.unobserve(img);
          }
        });
      },
      {
        rootMargin: "50px",
      }
    );

    images.forEach((img) => imageObserver.observe(img));
  } else {
    // Fallback for browsers without IntersectionObserver
    images.forEach((img) => {
      img.src = img.dataset.src || img.src;
      img.classList.add("loaded");
    });
  }
}

// Scroll Animations
function initScrollAnimations() {
  const animateElements = document.querySelectorAll(".animate-on-scroll");

  if ("IntersectionObserver" in window) {
    const animationObserver = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add("animate-fade-in");
          }
        });
      },
      {
        threshold: 0.1,
      }
    );

    animateElements.forEach((el) => animationObserver.observe(el));
  }

  // Smooth scroll for anchor links (non-navigation)
  document.querySelectorAll('a[href^="#"]:not(.nav-link)').forEach((anchor) => {
    anchor.addEventListener("click", function (e) {
      e.preventDefault();
      const targetId = this.getAttribute("href").substring(1);
      const targetElement = document.getElementById(targetId);

      if (targetElement) {
        targetElement.scrollIntoView({
          behavior: "smooth",
          block: "start",
        });
      }
    });
  });
}

// Tailor Carousel
function initTailorCarousel() {
  const carousel = document.querySelector(".tailors-carousel");
  if (!carousel) return;

  // Add touch/swipe support for mobile
  let startX = 0;
  let scrollLeft = 0;
  let isDown = false;

  carousel.addEventListener("mousedown", (e) => {
    isDown = true;
    startX = e.pageX - carousel.offsetLeft;
    scrollLeft = carousel.scrollLeft;
  });

  carousel.addEventListener("mouseleave", () => {
    isDown = false;
  });

  carousel.addEventListener("mouseup", () => {
    isDown = false;
  });

  carousel.addEventListener("mousemove", (e) => {
    if (!isDown) return;
    e.preventDefault();
    const x = e.pageX - carousel.offsetLeft;
    const walk = (x - startX) * 2;
    carousel.scrollLeft = scrollLeft - walk;
  });

  // Touch events for mobile
  carousel.addEventListener("touchstart", (e) => {
    startX = e.touches[0].pageX - carousel.offsetLeft;
    scrollLeft = carousel.scrollLeft;
  });

  carousel.addEventListener("touchmove", (e) => {
    if (!startX) return;
    const x = e.touches[0].pageX - carousel.offsetLeft;
    const walk = (x - startX) * 2;
    carousel.scrollLeft = scrollLeft - walk;
  });
}

// Form Validation
function initFormValidation() {
  const forms = document.querySelectorAll("form");

  forms.forEach((form) => {
    form.addEventListener("submit", function (e) {
      if (!validateForm(this)) {
        e.preventDefault();
      }
    });

    // Real-time validation
    const inputs = form.querySelectorAll("input, textarea, select");
    inputs.forEach((input) => {
      input.addEventListener("blur", function () {
        validateField(this);
      });
    });
  });
}

function validateForm(form) {
  const inputs = form.querySelectorAll(
    "input[required], textarea[required], select[required]"
  );
  let isValid = true;

  inputs.forEach((input) => {
    if (!validateField(input)) {
      isValid = false;
    }
  });

  return isValid;
}

function validateField(field) {
  const value = field.value.trim();
  const type = field.type;
  let isValid = true;
  let message = "";

  // Remove existing error messages
  removeFieldError(field);

  // Required field check
  if (field.hasAttribute("required") && !value) {
    isValid = false;
    message = "هذا الحقل مطلوب";
  } else if (value) {
    // Type-specific validation
    switch (type) {
      case "email":
        if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
          isValid = false;
          message = "يرجى إدخال بريد إلكتروني صحيح";
        }
        break;
      case "tel":
        if (!/^[\+]?[0-9\s\-\(\)]{10,}$/.test(value)) {
          isValid = false;
          message = "يرجى إدخال رقم هاتف صحيح";
        }
        break;
      case "text":
        if (field.name === "name" && value.length < 2) {
          isValid = false;
          message = "الاسم يجب أن يكون حرفين على الأقل";
        }
        break;
    }
  }

  if (!isValid) showFieldError(field, message);
  return isValid;
}

function showFieldError(field, message) {
  field.classList.add("error");

  const errorDiv = document.createElement("div");
  errorDiv.className = "field-error";
  errorDiv.textContent = message;

  field.parentNode.appendChild(errorDiv);
}

function removeFieldError(field) {
  field.classList.remove("error");
  const errorDiv = field.parentNode.querySelector(".field-error");
  if (errorDiv) {
    errorDiv.remove();
  }
}

// Message System
function showMessage(text, type = "info") {
  // Remove existing messages
  const existingMessage = document.querySelector(".message-toast");
  if (existingMessage) {
    existingMessage.remove();
  }

  // Create message element
  const message = document.createElement("div");
  message.className = `message-toast message-${type}`;
  message.textContent = text;

  // Set styles
  Object.assign(message.style, {
    position: "fixed",
    top: "20px",
    right: currentLanguage === "ar" ? "20px" : "auto",
    left: currentLanguage === "ar" ? "auto" : "20px",
    backgroundColor:
      type === "success"
        ? "#27ae60"
        : type === "error"
        ? "#e74c3c"
        : type === "warning"
        ? "#f39c12"
        : "#2c5aa0",
    color: "white",
    padding: "12px 24px",
    borderRadius: "8px",
    boxShadow: "0 4px 12px rgba(0,0,0,0.15)",
    zIndex: "9999",
    transform: "translateX(100%)",
    transition: "transform 0.3s ease",
    cursor: "pointer",
    maxWidth: "300px",
    wordWrap: "break-word",
  });

  // Add to page
  document.body.appendChild(message);

  // Animate in
  setTimeout(() => {
    message.style.transform = "translateX(0)";
  }, 100);

  // Auto remove after 5 seconds
  setTimeout(() => {
    if (message.parentNode) {
      message.style.transform = "translateX(100%)";
      setTimeout(() => {
        if (message.parentNode) {
          message.parentNode.removeChild(message);
        }
      }, 300);
    }
  }, 5000);

  // Click to dismiss
  message.addEventListener("click", () => {
    message.style.transform = "translateX(100%)";
    setTimeout(() => {
      if (message.parentNode) {
        message.parentNode.removeChild(message);
      }
    }, 300);
  });
}

// Export functions for external use
window.TafsilkPlatform = {
  showMessage,
  trackEvent: function (category, action, label = "") {
    console.log("Analytics Event:", { category, action, label });
  },
  performSearch,
  getCurrentLocation,
  switchLanguage,
  translate,
  currentLanguage: () => currentLanguage,
};

// Error handling
window.addEventListener("error", function (e) {
  console.error("JavaScript Error:", e.error);
});

// Analytics placeholder (replace with actual analytics code)
function trackEvent(category, action, label = "") {
  console.log("Analytics Event:", { category, action, label });
  // Replace with Google Analytics or similar
}

// Navigation toggle
const navToggle = document.querySelector(".nav-toggle");
const navMenu = document.querySelector(".nav-menu");

if (navToggle && navMenu) {
  navToggle.addEventListener("click", function () {
    navMenu.classList.toggle("active");
    navToggle.classList.toggle("active");
  });
}

// Smooth scrolling for navigation links
const navLinks2 = document.querySelectorAll(".nav-link");
navLinks2.forEach((link) => {
  link.addEventListener("click", function (e) {
    const href = this.getAttribute("href") || "";
    if (href.startsWith("#")) {
      e.preventDefault();
      const targetId = href.slice(1);
      const targetSection = document.getElementById(targetId);
      if (targetSection) {
        targetSection.scrollIntoView({ behavior: "smooth" });
      }
      if (navMenu && navMenu.classList.contains("active")) {
        navMenu.classList.remove("active");
        if (navToggle) navToggle.classList.remove("active");
      }
    }
    // Non-hash links navigate normally (e.g., portfolios-test1.html)
  });
});

// Location button functionality
const useLocationBtn = document.getElementById("useLocationBtn");
if (useLocationBtn) {
  useLocationBtn.addEventListener("click", function () {
    this.innerHTML =
      '<i class="fas fa-spinner fa-spin"></i> <span>جاري تحديد الموقع...</span>';

    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        function (position) {
          console.log("Location obtained:", position.coords);
          const searchInput = document.querySelector(".search-input");
          if (searchInput) {
            searchInput.placeholder = "تم تحديد موقعك بنجاح...";
          }
          useLocationBtn.innerHTML =
            '<i class="fas fa-check"></i> <span>تم تحديد الموقع</span>';
        },
        function (error) {
          console.error("Location error:", error);
          useLocationBtn.innerHTML =
            '<i class="fas fa-location-arrow"></i> <span>أو استخدم موقعي الحالي</span>';
          alert("لا يمكن الحصول على موقعك. يرجى المحاولة مرة أخرى.");
        }
      );
    } else {
      useLocationBtn.innerHTML =
        '<i class="fas fa-location-arrow"></i> <span>أو استخدم موقعي الحالي</span>';
      alert("متصفحك لا يدعم خدمة تحديد المواقع");
    }
  });
}

// Search form handling
const searchForm = document.querySelector(".search-form");
if (searchForm) {
  searchForm.addEventListener("submit", function (e) {
    e.preventDefault();
    const searchInput = document.querySelector(".search-input");
    const searchTerm = searchInput ? searchInput.value.trim() : "";

    if (searchTerm) {
      console.log("Searching for:", searchTerm);
      // Simulate search
      const searchBtn = this.querySelector('button[type="submit"]');
      if (searchBtn) {
        const originalContent = searchBtn.innerHTML;
        searchBtn.innerHTML =
          '<i class="fas fa-spinner fa-spin"></i> <span>جاري البحث...</span>';

        setTimeout(() => {
          searchBtn.innerHTML = originalContent;
          alert("تم العثور على 15 خياط في منطقتك!");
        }, 2000);
      }
    } else {
      alert("يرجى إدخال كلمة البحث أو استخدام موقعك الحالي");
    }
  });
}

// Language toggle functionality
const langToggles = document.querySelectorAll(".lang-toggle");
langToggles.forEach((toggle) => {
  toggle.addEventListener("click", function () {
    console.log("Language toggle clicked");
    // Simple language toggle simulation
    const span = this.querySelector("span");
    if (span) {
      if (span.textContent.includes("العربية")) {
        span.textContent = "English / العربية";
      } else {
        span.textContent = "العربية / English";
      }
    }
  });
});

// Intersection Observer for animations
const observerOptions = {
  threshold: 0.1,
  rootMargin: "0px 0px -50px 0px",
};

const observer = new IntersectionObserver(function (entries) {
  entries.forEach((entry) => {
    if (entry.isIntersecting) {
      entry.target.classList.add("fade-in");
    }
  });
}, observerOptions);

// Observe sections for fade-in animation
const sections = document.querySelectorAll("section");
sections.forEach((section) => {
  observer.observe(section);
});

console.log("Main script loaded successfully");
