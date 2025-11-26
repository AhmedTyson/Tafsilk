// ============================================================================
// STATE MANAGEMENT
// ============================================================================
const state = {
  products: [],
  filteredProducts: [],
  cart: JSON.parse(localStorage.getItem("cart")) || [],
  currentModalProduct: null,
  selectedSize: null,
  selectedColor: null,
  selectedQuantity: 1,
};

// ============================================================================
// DATA & API
// ============================================================================
async function loadProducts() {
  try {
    const res = await fetch("/api/products");
    if (!res.ok) throw new Error("Server error");
    state.products = await res.json();
  } catch {
    console.warn("Backend not found — using local data");
    state.products = getLocalProducts();
  }
  state.filteredProducts = [...state.products];
  renderProducts(state.filteredProducts);
}

function getLocalProducts() {
  return [
    {
      id: 1,
      name: "بدلة رجالية رمادية",
      category: "men",
      price: 750,
      originalPrice: 900,
      image: "https://placehold.co/400x500/e3f2fd/2c5aa0?text=بدلة+رجالية",
      additionalImages: [
        "https://placehold.co/400x500/e3f2fd/2c5aa0?text=بدلة+أمامية",
        "https://placehold.co/400x500/e3f2fd/2c5aa0?text=بدلة+خلفية",
        "https://placehold.co/400x500/e3f2fd/2c5aa0?text=بدلة+جانبية",
      ],
      description:
        "بدلة أنيقة مناسبة للمناسبات الرسمية والعمل. مصنوعة من قماش عالي الجودة مع تفاصيل دقيقة تضمن الراحة والأناقة.",
      material: {
        type: "صوف ممزوج",
        composition: "70% صوف، 30% بوليستر",
        weight: "متوسط الوزن",
        care: "تنظيف جاف فقط",
      },
      sizes: ["S", "M", "L", "XL"],
      colors: ["رمادي", "أسود", "أزرق"],
      tailor: {
        name: "أستاذ خالد محمود",
        specialty: "بدل رجالية",
        portfolio: "portfolios.html",
        avatar: "https://placehold.co/100x100/e3f2fd/2c5aa0?text=خ",
      },
      rating: 4.8,
      reviews: [
        {
          name: "محمد أحمد",
          avatar: "https://placehold.co/40x40/e3f2fd/2c5aa0?text=م",
          rating: 5,
          comment:
            "البدلة رائعة الجودة والتفصيل دقيق جداً. أنصح بالتعامل مع أستاذ خالد.",
          date: "منذ أسبوعين",
        },
        {
          name: "أحمد سامي",
          avatar: "https://placehold.co/40x40/e3f2fd/2c5aa0?text=أ",
          rating: 4,
          comment:
            "جيدة جداً ولكن تحتاج بعض التعديلات البسيطة. الخياط كان متعاوناً جداً.",
          date: "منذ شهر",
        },
      ],
      badge: "الأكثر مبيعاً",
    },
    {
      id: 2,
      name: "فستان سهرة أزرق",
      category: "women",
      price: 650,
      image: "https://placehold.co/400x500/e3f2fd/2c5aa0?text=فستان+سهرة",
      additionalImages: [
        "https://placehold.co/400x500/e3f2fd/2c5aa0?text=فستان+أمامية",
        "https://placehold.co/400x500/e3f2fd/2c5aa0?text=فستان+خلفي",
        "https://placehold.co/400x500/e3f2fd/2c5aa0?text=فستان+جانبي",
      ],
      description:
        "فستان أنيق للمناسبات والسهرات. تصميم مريح وأنيق يناسب جميع الأجسام مع تفاصيل تطريز فاخرة.",
      material: {
        type: "حرير صناعي",
        composition: "100% حرير صناعي",
        weight: "خفيف",
        care: "غسيل يدوي في ماء بارد",
      },
      sizes: ["XS", "S", "M", "L"],
      colors: ["أزرق", "أحمر", "أسود"],
      tailor: {
        name: "أستاذة فاطمة علي",
        specialty: "فساتين سهرة",
        portfolio: "portfolios.html",
        avatar: "https://placehold.co/100x100/e3f2fd/2c5aa0?text=ف",
      },
      rating: 4.9,
      reviews: [
        {
          name: "سارة محمود",
          avatar: "https://placehold.co/40x40/e3f2fd/2c5aa0?text=س",
          rating: 5,
          comment:
            "الفستان رائع جداً! التصميم والتفصيل أكثر من رائع. شكراً جزيلاً.",
          date: "منذ أسبوع",
        },
      ],
      badge: "جديد",
    },
    {
      id: 3,
      name: "بلوزة نسائية بيضاء",
      category: "women",
      price: 280,
      originalPrice: 350,
      image: "https://placehold.co/400x500/e3f2fd/2c5aa0?text=بلوزة+نسائية",
      additionalImages: [
        "https://placehold.co/400x500/e3f2fd/2c5aa0?text=بلوزة+أمامية",
        "https://placehold.co/400x500/e3f2fd/2c5aa0?text=بلوزة+خلفية",
      ],
      description:
        "بلوزة كلاسيكية مناسبة للعمل والمناسبات اليومية. مصنوعة من قطن عالي الجودة مع تفاصيل أنيقة.",
      material: {
        type: "قطن",
        composition: "100% قطن",
        weight: "خفيف",
        care: "غسيل آلي في ماء بارد",
      },
      sizes: ["S", "M", "L", "XL"],
      colors: ["أبيض", "أسود", "أزرق"],
      tailor: {
        name: "أستاذة نادية حسن",
        specialty: "ملابس نسائية",
        portfolio: "portfolios.html",
        avatar: "https://placehold.co/100x100/e3f2fd/2c5aa0?text=ن",
      },
      rating: 4.6,
      reviews: [
        {
          name: "منى إبراهيم",
          avatar: "https://placehold.co/40x40/e3f2fd/2c5aa0?text=م",
          rating: 4,
          comment: "البلوزة مريحة وجميلة، ولكن اللون يبهت قليلاً بعد الغسيل.",
          date: "منذ 3 أيام",
        },
      ],
    },
    {
      id: 4,
      name: "قميص رجالي أزرق",
      category: "men",
      price: 320,
      image: "https://placehold.co/400x500/e3f2fd/2c5aa0?text=قميص+رجالي",
      additionalImages: [
        "https://placehold.co/400x500/e3f2fd/2c5aa0?text=قميص+أمامي",
        "https://placehold.co/400x500/e3f2fd/2c5aa0?text=قميص+خلفي",
      ],
      description:
        "قميص رسمي أنيق مناسب للعمل والمناسبات. متوفر بعدة ألوان ومقاسات مع تفاصيل عالية الجودة.",
      material: {
        type: "قطن ممزوج",
        composition: "60% قطن، 40% بوليستر",
        weight: "خفيف",
        care: "غسيل آلي في ماء دافئ",
      },
      sizes: ["S", "M", "L", "XL", "XXL"],
      colors: ["أزرق", "أبيض", "رمادي"],
      tailor: {
        name: "أستاذ محمد أحمد",
        specialty: "ملابس رجالية",
        portfolio: "portfolios.html",
        avatar: "https://placehold.co/100x100/e3f2fd/2c5aa0?text=م",
      },
      rating: 4.7,
      reviews: [
        {
          name: "هشام جلال",
          avatar: "https://placehold.co/40x40/e3f2fd/2c5aa0?text=ه",
          rating: 5,
          comment: "القميص مريح وأنيق. الخياطة متقنة والأسعار معقولة.",
          date: "منذ أسبوعين",
        },
      ],
    },
    {
      id: 5,
      name: "بنطلون جينز",
      category: "men",
      price: 420,
      originalPrice: 500,
      image: "https://placehold.co/400x500/e3f2fd/2c5aa0?text=بنطلون+جينز",
      additionalImages: [
        "https://placehold.co/400x500/e3f2fd/2c5aa0?text=بنطلون+أمامي",
        "https://placehold.co/400x500/e3f2fd/2c5aa0?text=بنطلون+خلفي",
      ],
      description:
        "بنطلون جينز كلاسيكي مريح ومناسب للارتداء اليومي. متوفر بعدة مقاسات وألوان.",
      material: {
        type: "دينيم",
        composition: "98% قطن، 2% إيلاستين",
        weight: "متوسط",
        care: "غسيل آلي في ماء بارد",
      },
      sizes: ["28", "30", "32", "34", "36"],
      colors: ["أزرق", "أسود"],
      tailor: {
        name: "أستاذ خالد محمود",
        specialty: "بدل رجالية",
        portfolio: "portfolios.html",
        avatar: "https://placehold.co/100x100/e3f2fd/2c5aa0?text=خ",
      },
      rating: 4.5,
      reviews: [
        {
          name: "أحمد سامي",
          avatar: "https://placehold.co/40x40/e3f2fd/2c5aa0?text=أ",
          rating: 4,
          comment: "مريح وجيد، ولكن اللون يبهت قليلاً بعد عدة غسلات.",
          date: "منذ شهر",
        },
      ],
    },
    {
      id: 6,
      name: "طقم أطفال صيفي",
      category: "kids",
      price: 180,
      originalPrice: 220,
      image: "https://placehold.co/400x500/e3f2fd/2c5aa0?text=طقم+أطفال",
      additionalImages: [
        "https://placehold.co/400x500/e3f2fd/2c5aa0?text=طقم+أمامي",
        "https://placehold.co/400x500/e3f2fd/2c5aa0?text=طقم+خلفي",
      ],
      description:
        "طقم صيفي مريح وجميل للأطفال. مصنوع من أقمشة قطنية ناعمة على البشرة مع تصميمات ملونة وجذابة.",
      material: {
        type: "قطن",
        composition: "100% قطن",
        weight: "خفيف",
        care: "غسيل آلي في ماء بارد",
      },
      sizes: ["2-4 سنوات", "4-6 سنوات", "6-8 سنوات"],
      colors: ["أزرق", "أخضر", "أحمر"],
      tailor: {
        name: "أستاذة سارة محمود",
        specialty: "ملابس أطفال",
        portfolio: "portfolios.html",
        avatar: "https://placehold.co/100x100/e3f2fd/2c5aa0?text=س",
      },
      rating: 4.8,
      reviews: [
        {
          name: "أم محمد",
          avatar: "https://placehold.co/40x40/e3f2fd/2c5aa0?text=أ",
          rating: 5,
          comment: "الطقم جميل جداً ومريح لابني. الألوان زاهية والقماش ناعم.",
          date: "منذ أسبوع",
        },
      ],
      badge: "عرض خاص",
    },
    {
      id: 7,
      name: "حقيبة يد نسائية",
      category: "accessories",
      price: 250,
      image: "https://placehold.co/400x500/e3f2fd/2c5aa0?text=حقيبة+يد",
      additionalImages: [
        "https://placehold.co/400x500/e3f2fd/2c5aa0?text=حقيبة+أمامية",
        "https://placehold.co/400x500/e3f2fd/2c5aa0?text=حقيبة+خلفية",
        "https://placehold.co/400x500/e3f2fd/2c5aa0?text=حقيبة+داخلية",
      ],
      description:
        "حقيبة يد أنيقة ومناسبة للمناسبات اليومية والرسمية. مصنوعة من مواد عالية الجودة مع تفاصيل دقيقة.",
      material: {
        type: "جلد صناعي",
        composition: "بولي يوريثان",
        weight: "خفيف",
        care: "مسح بقطعة قماش ناعمة",
      },
      sizes: ["مقاس واحد"],
      colors: ["أسود", "بني", "أحمر"],
      tailor: {
        name: "أستاذة منى إبراهيم",
        specialty: "إكسسوارات",
        portfolio: "portfolios.html",
        avatar: "https://placehold.co/100x100/e3f2fd/2c5aa0?text=م",
      },
      rating: 4.4,
      reviews: [
        {
          name: "فاطمة علي",
          avatar: "https://placehold.co/40x40/e3f2fd/2c5aa0?text=ف",
          rating: 4,
          comment: "الحقيبة جميلة وأنيقة، ولكن المقبض يحتاج إلى تدعيم أفضل.",
          date: "منذ أسبوعين",
        },
      ],
    },
    {
      id: 8,
      name: "ربطة عنق حريرية",
      category: "accessories",
      price: 120,
      image: "https://placehold.co/400x500/e3f2fd/2c5aa0?text=ربطة+عنق",
      additionalImages: [
        "https://placehold.co/400x500/e3f2fd/2c5aa0?text=ربطة+عنق+1",
        "https://placehold.co/400x500/e3f2fd/2c5aa0?text=ربطة+عنق+2",
      ],
      description:
        "ربطة عنق حريرية أنيقة بتصميمات كلاسيكية وعصرية. مثالية لإكمال الأناقة في المناسبات الرسمية.",
      material: {
        type: "حرير",
        composition: "100% حرير",
        weight: "خفيف جداً",
        care: "تنظيف جاف فقط",
      },
      sizes: ["قياس قياسي"],
      colors: ["أزرق", "أحمر", "أسود", "رمادي"],
      tailor: {
        name: "أستاذ أحمد سامي",
        specialty: "إكسسوارات رجالية",
        portfolio: "portfolios.html",
        avatar: "https://placehold.co/100x100/e3f2fd/2c5aa0?text=أ",
      },
      rating: 4.7,
      reviews: [
        {
          name: "محمد أحمد",
          avatar: "https://placehold.co/40x40/e3f2fd/2c5aa0?text=م",
          rating: 5,
          comment: "ربطة العنق رائعة الجودة والتصميم أنيق جداً. أنصح بها.",
          date: "منذ شهر",
        },
      ],
    },
  ];
}

// ============================================================================
// PRODUCT RENDERING
// ============================================================================
function renderProducts(products) {
  const productsGrid = document.getElementById("productsGrid");

  if (!products.length) {
    productsGrid.innerHTML = `
      <div style="grid-column: 1/-1; text-align: center; padding: var(--spacing-3xl); color: var(--text-secondary);">
        <i class="fas fa-search" style="font-size: 3rem; opacity: 0.3; margin-bottom: var(--spacing-md);"></i>
        <p>لم يتم العثور على منتجات</p>
      </div>
    `;
    return;
  }

  productsGrid.innerHTML = "";
  products.forEach((product) => {
    const card = createProductCard(product);
    productsGrid.appendChild(card);
  });
}

function createProductCard(product) {
  const card = document.createElement("div");
  card.className = "product-card";
  card.setAttribute("data-category", product.category);

  const badgeHTML = product.badge
    ? `<span class="product-badge">${product.badge}</span>`
    : "";
  const originalPriceHTML = product.originalPrice
    ? `<span class="original-price">${product.originalPrice} ج.م</span>`
    : "";

  card.innerHTML = `
    <div class="product-image-container">
      <img src="${product.image}" alt="${
    product.name
  }" class="product-image" loading="lazy">
      ${badgeHTML}
    </div>
    <div class="product-info">
      <div class="product-category">${getCategoryName(product.category)}</div>
      <h3 class="product-title">${product.name}</h3>
      <p class="product-description">${product.description.substring(
        0,
        80
      )}...</p>
      <div class="product-price">
        <span class="current-price">${product.price} ج.م</span>
        ${originalPriceHTML}
      </div>
      <div class="product-tailor">
        <img src="${product.tailor.avatar}" alt="${
    product.tailor.name
  }" class="tailor-avatar" loading="lazy">
        <div class="tailor-info">
          <div class="tailor-name">${product.tailor.name}</div>
          <div class="tailor-specialty">${product.tailor.specialty}</div>
        </div>
        <a href="${
          product.tailor.portfolio
        }" class="tailor-portfolio">معرض الأعمال</a>
      </div>
      <div class="product-actions">
        <button class="btn btn-outline view-details" data-id="${
          product.id
        }">عرض التفاصيل</button>
        <button class="btn btn-primary add-to-cart" data-id="${product.id}">
          <i class="fas fa-shopping-cart"></i>
          أضف للسلة
        </button>
      </div>
    </div>
  `;

  return card;
}

// ============================================================================
// PRODUCT MODAL
// ============================================================================
function openProductModal(productId) {
  const product = state.products.find((p) => p.id === productId);
  if (!product) return;

  state.currentModalProduct = product;
  state.selectedSize = null;
  state.selectedColor = null;
  state.selectedQuantity = 1;

  // Basic info
  document.getElementById("modalProductTitle").textContent = product.name;
  document.getElementById("modalProductDescription").textContent =
    product.description;
  document.getElementById("modalMainImage").src = product.image;
  document.getElementById("modalMainImage").alt = product.name;

  // Additional images
  renderAdditionalImages(product);

  // Material info
  renderMaterialInfo(product);

  // Reviews
  renderReviews(product);

  // Size options
  renderSizeOptions(product);

  // Color options
  renderColorOptions(product);

  // Meta info
  renderMetaInfo(product);

  // Quantity
  document.getElementById("modalQuantity").textContent = state.selectedQuantity;
  updateQuantityButtons();

  // Update add to cart button data attribute
  document.getElementById("modalAddToCart").setAttribute("data-id", product.id);

  // Show modal
  const modal = document.getElementById("productModal");
  modal.style.display = "block";
  document.body.style.overflow = "hidden";
}

function closeProductModal() {
  const modal = document.getElementById("productModal");
  modal.style.display = "none";
  document.body.style.overflow = "";

  // Reset selections
  state.currentModalProduct = null;
  state.selectedSize = null;
  state.selectedColor = null;
  state.selectedQuantity = 1;
}

function renderAdditionalImages(product) {
  const container = document.getElementById("modalAdditionalImages");
  container.innerHTML = "";

  product.additionalImages.forEach((image, index) => {
    const imgElement = document.createElement("div");
    imgElement.className = `additional-image ${index === 0 ? "active" : ""}`;
    imgElement.innerHTML = `<img src="${image}" alt="${product.name} - صورة ${
      index + 1
    }" loading="lazy">`;
    imgElement.addEventListener("click", function () {
      document.getElementById("modalMainImage").src = image;
      document
        .querySelectorAll(".additional-image")
        .forEach((img) => img.classList.remove("active"));
      this.classList.add("active");
    });
    container.appendChild(imgElement);
  });
}

function renderMaterialInfo(product) {
  const container = document.getElementById("modalMaterialDetails");
  container.innerHTML = "";

  for (const [key, value] of Object.entries(product.material)) {
    const item = document.createElement("div");
    item.className = "material-item";
    item.innerHTML = `
      <span class="material-label">${getMaterialLabel(key)}:</span>
      <span class="material-value">${value}</span>
    `;
    container.appendChild(item);
  }
}

function renderReviews(product) {
  const container = document.getElementById("modalReviews");
  container.innerHTML = "";

  product.reviews.forEach((review) => {
    const stars = Array(5)
      .fill(0)
      .map((_, i) =>
        i < review.rating
          ? '<i class="fas fa-star"></i>'
          : '<i class="far fa-star"></i>'
      )
      .join("");

    const card = document.createElement("div");
    card.className = "review-card";
    card.innerHTML = `
      <div class="review-header">
        <div class="reviewer">
          <img src="${review.avatar}" alt="${review.name}" class="reviewer-avatar" loading="lazy">
          <div class="reviewer-info">
            <h4>${review.name}</h4>
            <p>عميل</p>
          </div>
        </div>
        <div class="review-stars">${stars}</div>
      </div>
      <div class="review-content">
        <p>${review.comment}</p>
      </div>
      <div class="review-date">${review.date}</div>
    `;
    container.appendChild(card);
  });
}

function renderSizeOptions(product) {
  const container = document.getElementById("sizeOptions");
  container.innerHTML = "";

  product.sizes.forEach((size) => {
    const btn = document.createElement("button");
    btn.className = "size-btn";
    btn.textContent = size;
    btn.type = "button";
    btn.addEventListener("click", function () {
      document
        .querySelectorAll(".size-btn")
        .forEach((b) => b.classList.remove("selected"));
      this.classList.add("selected");
      state.selectedSize = size;
      document.getElementById("sizeError").classList.remove("show");
    });
    container.appendChild(btn);
  });
}

function renderColorOptions(product) {
  const container = document.getElementById("colorOptions");
  container.innerHTML = "";

  product.colors.forEach((color) => {
    const btn = document.createElement("button");
    btn.className = "color-btn";
    btn.textContent = color;
    btn.type = "button";
    btn.setAttribute("data-color", color);
    btn.addEventListener("click", function () {
      document
        .querySelectorAll(".color-btn")
        .forEach((b) => b.classList.remove("selected"));
      this.classList.add("selected");
      state.selectedColor = color;
      document.getElementById("colorError").classList.remove("show");
    });
    container.appendChild(btn);
  });
}

function renderMetaInfo(product) {
  document.getElementById("modalTailorName").textContent = product.tailor.name;
  document.getElementById("modalCategory").textContent = getCategoryName(
    product.category
  );
  document.getElementById("modalSizes").textContent = product.sizes.join("، ");
  document.getElementById("modalRating").textContent = `${product.rating}/5`;
  document.getElementById("modalPrice").textContent = `${product.price} ج.م`;
  document.getElementById("modalTailorPortfolio").href =
    product.tailor.portfolio;
}

// ============================================================================
// CART MANAGEMENT
// ============================================================================
function addToCart(productId, fromModal = false) {
  const product = state.products.find((p) => p.id === productId);
  if (!product) return;

  let size, color, quantity;

  if (
    fromModal &&
    state.currentModalProduct &&
    state.currentModalProduct.id === productId
  ) {
    // Validate selections from modal
    if (!state.selectedSize) {
      document.getElementById("sizeError").classList.add("show");
      return;
    }
    if (!state.selectedColor) {
      document.getElementById("colorError").classList.add("show");
      return;
    }
    size = state.selectedSize;
    color = state.selectedColor;
    quantity = state.selectedQuantity;
  } else {
    // Quick add from product card with default options
    size = product.sizes[0];
    color = product.colors[0];
    quantity = 1;
  }

  // Create unique cart key for each variation
  const cartKey = `${productId}-${size}-${color}`;

  // Find existing item with same variation
  const existingItem = state.cart.find((item) => item.cartKey === cartKey);

  if (existingItem) {
    // Update quantity of existing variation
    existingItem.quantity += quantity;
    if (existingItem.quantity > 10) {
      existingItem.quantity = 10;
      showNotification("تم تحديث الكمية إلى الحد الأقصى (10)");
    }
  } else {
    // Add new variation to cart
    state.cart.push({
      id: product.id,
      name: product.name,
      price: product.price,
      image: product.image,
      tailor: product.tailor.name,
      size,
      color,
      quantity,
      cartKey,
    });
  }

  updateCartCount();
  updateCartDisplay();
  saveCart();

  const message = fromModal
    ? `تمت إضافة ${quantity} من ${product.name} (${size}, ${color}) إلى السلة`
    : `تمت إضافة ${product.name} إلى السلة`;

  showNotification(message);

  // Reset modal selections after adding
  if (fromModal) {
    document
      .querySelectorAll(".size-btn, .color-btn")
      .forEach((btn) => btn.classList.remove("selected"));
    state.selectedSize = null;
    state.selectedColor = null;
    state.selectedQuantity = 1;
    document.getElementById("modalQuantity").textContent = "1";
    updateQuantityButtons();
  }
}

function removeFromCart(cartKey) {
  const item = state.cart.find((i) => i.cartKey === cartKey);
  if (!item) return;

  state.cart = state.cart.filter((i) => i.cartKey !== cartKey);
  updateCartCount();
  updateCartDisplay();
  saveCart();
  showNotification(
    `تم إزالة ${item.name} (${item.size}, ${item.color}) من السلة`
  );
}

function updateQuantity(cartKey, delta) {
  const item = state.cart.find((i) => i.cartKey === cartKey);
  if (!item) return;

  const newQuantity = item.quantity + delta;

  if (newQuantity <= 0) {
    removeFromCart(cartKey);
    return;
  }

  if (newQuantity > 10) {
    showNotification("الحد الأقصى للكمية هو 10");
    return;
  }

  item.quantity = newQuantity;
  updateCartCount();
  updateCartDisplay();
  saveCart();
}

function updateCartCount() {
  const count = state.cart.reduce((sum, item) => sum + item.quantity, 0);
  document.getElementById("cartCount").textContent = count;
}

function updateCartDisplay() {
  const container = document.getElementById("cartItems");
  const emptyMessage = document.getElementById("emptyCartMessage");

  if (state.cart.length === 0) {
    emptyMessage.style.display = "block";
    container.querySelectorAll(".cart-item").forEach((item) => item.remove());
    document.getElementById("cartTotal").textContent = "0 ج.م";
    return;
  }

  emptyMessage.style.display = "none";
  container.innerHTML = "";

  let total = 0;
  state.cart.forEach((item) => {
    total += item.price * item.quantity;
    const cartItem = createCartItem(item);
    container.appendChild(cartItem);
  });

  document.getElementById("cartTotal").textContent = `${total.toFixed(2)} ج.م`;
}

function createCartItem(item) {
  const div = document.createElement("div");
  div.className = "cart-item";
  div.innerHTML = `
    <div class="cart-item-image">
      <img src="${item.image}" alt="${item.name}" loading="lazy">
    </div>
    <div class="cart-item-details">
      <div class="cart-item-name">${item.name}</div>
      <div class="cart-item-tailor">${item.tailor}</div>
      <div class="cart-item-variation">
        <span>المقاس: ${item.size}</span>
        <span>اللون: ${item.color}</span>
      </div>
      <div class="cart-item-price">${(item.price * item.quantity).toFixed(
        2
      )} ج.م</div>
      <div class="cart-item-actions">
        <div class="quantity-controls">
          <button class="quantity-btn" data-action="decrease" data-key="${
            item.cartKey
          }">
            <i class="fas fa-minus"></i>
          </button>
          <span class="quantity-value">${item.quantity}</span>
          <button class="quantity-btn" data-action="increase" data-key="${
            item.cartKey
          }">
            <i class="fas fa-plus"></i>
          </button>
        </div>
        <button class="remove-item-btn" data-key="${
          item.cartKey
        }" title="إزالة من السلة">
          <i class="fas fa-trash"></i>
        </button>
      </div>
    </div>
  `;

  return div;
}

function saveCart() {
  localStorage.setItem("cart", JSON.stringify(state.cart));
}

function openCart() {
  document.getElementById("cartSidebar").classList.add("active");
  document.getElementById("cartOverlay").classList.add("active");
  document.body.style.overflow = "hidden";
  updateCartDisplay();
}

function closeCart() {
  document.getElementById("cartSidebar").classList.remove("active");
  document.getElementById("cartOverlay").classList.remove("active");
  document.body.style.overflow = "";
}

// ============================================================================
// FILTERS & SEARCH
// ============================================================================
function applyFilters() {
  let filtered = [...state.products];

  // Category filter
  const selectedCategories = Array.from(
    document.querySelectorAll('input[name="category"]:checked')
  ).map((cb) => cb.id.replace("cat-", ""));

  if (!selectedCategories.includes("all") && selectedCategories.length > 0) {
    filtered = filtered.filter((p) => selectedCategories.includes(p.category));
  }

  // Price filter
  const selectedPriceRanges = Array.from(
    document.querySelectorAll('input[name="price-range"]:checked')
  ).map((cb) => cb.id);

  if (selectedPriceRanges.length > 0) {
    filtered = filtered.filter((p) => {
      return selectedPriceRanges.some((range) => {
        if (range === "price-100") return p.price < 100;
        if (range === "price-500") return p.price >= 100 && p.price <= 500;
        if (range === "price-1000") return p.price > 500 && p.price <= 1000;
        if (range === "price-1000plus") return p.price > 1000;
        return false;
      });
    });
  }

  // Rating filter
  const selectedRatings = Array.from(
    document.querySelectorAll('input[name="rating"]:checked')
  ).map((cb) => parseInt(cb.id.replace("rating-", "")));

  if (selectedRatings.length > 0) {
    filtered = filtered.filter((p) =>
      selectedRatings.some((rating) => p.rating >= rating)
    );
  }

  state.filteredProducts = filtered;
  applySorting();
}

function applySorting() {
  const sortValue = document.getElementById("sort").value;
  let sorted = [...state.filteredProducts];

  switch (sortValue) {
    case "newest":
      sorted.reverse();
      break;
    case "price-low":
      sorted.sort((a, b) => a.price - b.price);
      break;
    case "price-high":
      sorted.sort((a, b) => b.price - a.price);
      break;
    case "popular":
    default:
      sorted.sort((a, b) => b.rating - a.rating);
      break;
  }

  renderProducts(sorted);
}

function handleSearch() {
  const searchTerm = document
    .getElementById("searchInput")
    .value.toLowerCase()
    .trim();

  if (!searchTerm) {
    renderProducts(state.filteredProducts);
    return;
  }

  const results = state.filteredProducts.filter(
    (product) =>
      product.name.toLowerCase().includes(searchTerm) ||
      product.description.toLowerCase().includes(searchTerm) ||
      product.tailor.name.toLowerCase().includes(searchTerm)
  );

  renderProducts(results);
}

// ============================================================================
// UTILITY FUNCTIONS
// ============================================================================
function getCategoryName(category) {
  const categories = {
    men: "ملابس رجالية",
    women: "ملابس نسائية",
    kids: "ملابس أطفال",
    accessories: "إكسسوارات",
  };
  return categories[category] || category;
}

function getMaterialLabel(key) {
  const labels = {
    type: "نوع القماش",
    composition: "التركيبة",
    weight: "الوزن",
    care: "طريقة العناية",
  };
  return labels[key] || key;
}

function showNotification(message) {
  const notification = document.createElement("div");
  notification.textContent = message;
  notification.style.cssText = `
    position: fixed;
    top: 100px;
    right: 20px;
    background-color: var(--success-color);
    color: white;
    padding: var(--spacing-md) var(--spacing-lg);
    border-radius: var(--radius-md);
    box-shadow: var(--shadow-lg);
    z-index: 3000;
    animation: slideIn 0.3s ease-out;
    font-size: var(--font-size-sm);
    max-width: 300px;
  `;

  document.body.appendChild(notification);

  setTimeout(() => {
    notification.style.animation = "slideOut 0.3s ease-in";
    setTimeout(() => document.body.removeChild(notification), 300);
  }, 3000);
}

function updateQuantityButtons() {
  const decreaseBtn = document.getElementById("decreaseQty");
  const increaseBtn = document.getElementById("increaseQty");

  if (decreaseBtn) decreaseBtn.disabled = state.selectedQuantity <= 1;
  if (increaseBtn) increaseBtn.disabled = state.selectedQuantity >= 10;
}

// ============================================================================
// EVENT LISTENERS
// ============================================================================
function attachEventListeners() {
  // Product card events (delegated)
  document.getElementById("productsGrid").addEventListener("click", (e) => {
    const viewBtn = e.target.closest(".view-details");
    const addBtn = e.target.closest(".add-to-cart");

    if (viewBtn) {
      e.preventDefault();
      openProductModal(parseInt(viewBtn.dataset.id));
    }
    if (addBtn) {
      e.preventDefault();
      addToCart(parseInt(addBtn.dataset.id), false);
    }
  });

  // Modal events
  document
    .getElementById("modalClose")
    .addEventListener("click", closeProductModal);
  document.getElementById("productModal").addEventListener("click", (e) => {
    if (e.target.id === "productModal") closeProductModal();
  });

  document
    .getElementById("modalAddToCart")
    .addEventListener("click", function () {
      const productId = parseInt(this.dataset.id);
      addToCart(productId, true);
    });

  // Quantity controls in modal
  document.getElementById("decreaseQty")?.addEventListener("click", () => {
    if (state.selectedQuantity > 1) {
      state.selectedQuantity--;
      document.getElementById("modalQuantity").textContent =
        state.selectedQuantity;
      updateQuantityButtons();
    }
  });

  document.getElementById("increaseQty")?.addEventListener("click", () => {
    if (state.selectedQuantity < 10) {
      state.selectedQuantity++;
      document.getElementById("modalQuantity").textContent =
        state.selectedQuantity;
      updateQuantityButtons();
    }
  });

  // Cart events
  document.getElementById("cartIcon").addEventListener("click", openCart);
  document.getElementById("cartClose").addEventListener("click", closeCart);
  document.getElementById("cartOverlay").addEventListener("click", closeCart);
  document
    .getElementById("continueShoppingBtn")
    .addEventListener("click", closeCart);

  // Cart item actions (delegated)
  document.getElementById("cartItems").addEventListener("click", (e) => {
    const qtyBtn = e.target.closest(".quantity-btn");
    const removeBtn = e.target.closest(".remove-item-btn");

    if (qtyBtn) {
      const { action, key } = qtyBtn.dataset;
      updateQuantity(key, action === "increase" ? 1 : -1);
    }
    if (removeBtn) {
      removeFromCart(removeBtn.dataset.key);
    }
  });

  document.getElementById("checkoutBtn").addEventListener("click", () => {
    if (state.cart.length === 0) {
      showNotification("سلة التسوق فارغة");
      return;
    }
    showNotification("جاري تحويلك إلى صفحة الدفع...");
    setTimeout(() => (window.location.href = "checkout.html"), 1500);
  });

  // Search
  document
    .getElementById("searchInput")
    .addEventListener("input", handleSearch);

  // Sort
  document.getElementById("sort").addEventListener("change", applySorting);

  // Filters
  document.querySelectorAll('input[type="checkbox"]').forEach((checkbox) => {
    checkbox.addEventListener("change", applyFilters);
  });

  // Clear filters
  document.querySelectorAll(".clear-btn").forEach((btn) => {
    btn.addEventListener("click", function () {
      const section = this.closest(".filter-section");
      section
        .querySelectorAll('input[type="checkbox"]')
        .forEach((cb) => (cb.checked = false));
      applyFilters();
    });
  });

  // Filter actions
  document
    .querySelector(".filter-actions .btn-primary")
    .addEventListener("click", applyFilters);
  document
    .querySelector(".filter-actions .btn-outline")
    .addEventListener("click", () => {
      document
        .querySelectorAll('input[type="checkbox"]')
        .forEach((cb) => (cb.checked = false));
      document.getElementById("cat-all").checked = true;
      document.getElementById("searchInput").value = "";
      state.filteredProducts = [...state.products];
      renderProducts(state.products);
    });

  // Mobile navigation
  const navToggle = document.querySelector(".nav-toggle");
  const navMenu = document.getElementById("nav-menu");

  if (navToggle && navMenu) {
    navToggle.addEventListener("click", () => {
      const isExpanded = navToggle.getAttribute("aria-expanded") === "true";
      navToggle.setAttribute("aria-expanded", !isExpanded);
      navMenu.style.display = isExpanded ? "none" : "flex";
      navMenu.style.position = "absolute";
      navMenu.style.top = "100%";
      navMenu.style.right = "0";
      navMenu.style.backgroundColor = "var(--background-white)";
      navMenu.style.padding = "var(--spacing-lg)";
      navMenu.style.boxShadow = "var(--shadow-md)";
      navMenu.style.flexDirection = "column";
      navMenu.style.minWidth = "200px";
    });
  }

  // Keyboard navigation
  document.addEventListener("keydown", (e) => {
    if (e.key === "Escape") {
      if (document.getElementById("productModal").style.display === "block") {
        closeProductModal();
      }
      if (document.getElementById("cartSidebar").classList.contains("active")) {
        closeCart();
      }
    }
  });
}

// Add CSS animations
const style = document.createElement("style");
style.textContent = `
  @keyframes slideIn {
    from { transform: translateX(100%); opacity: 0; }
    to { transform: translateX(0); opacity: 1; }
  }
  @keyframes slideOut {
    from { transform: translateX(0); opacity: 1; }
    to { transform: translateX(100%); opacity: 0; }
  }
`;
document.head.appendChild(style);

// ============================================================================
// INITIALIZATION
// ============================================================================
document.addEventListener("DOMContentLoaded", () => {
  updateCartCount();
  loadProducts();
  attachEventListeners();
});
