// Video error handling
let videoAttempted = false;
let fallbackShown = false;

function handleVideoError() {
  console.log("Handling video error - showing fallback");
  showVideoFallback();
}

function showVideoFallback() {
  if (fallbackShown) return;

  const video = document.getElementById("heroVideo");
  const fallback = document.getElementById("heroVideoFallback");
  const hero = document.querySelector(".hero");

  console.log("Showing video fallback");

  if (video) {
    video.style.display = "none";
    video.pause();
  }

  if (fallback) {
    fallback.style.display = "flex";
    fallback.setAttribute("aria-hidden", "false");
  }

  // Add class to hero for styling
  if (hero) {
    hero.classList.add("no-video");
  }

  fallbackShown = true;
}

function hideVideoFallback() {
  const fallback = document.getElementById("heroVideoFallback");
  const video = document.getElementById("heroVideo");
  const hero = document.querySelector(".hero");

  if (fallback) {
    fallback.style.display = "none";
    fallback.setAttribute("aria-hidden", "true");
  }

  if (video) {
    video.style.display = "block";
    video.classList.add("loaded");
  }

  if (hero) {
    hero.classList.remove("no-video");
  }
}

function initializeVideo() {
  const video = document.getElementById("heroVideo");

  if (!video) {
    console.log("Video element not found - showing fallback");
    showVideoFallback();
    return;
  }

  console.log("Initializing video...");

  let loadTimeout;
  let hasLoaded = false;

  // Set a timeout to show fallback if video doesn't load
  loadTimeout = setTimeout(() => {
    if (!hasLoaded && !fallbackShown) {
      console.log("Video load timeout - showing fallback");
      showVideoFallback();
    }
  }, 5000); // Increased timeout to 5 seconds

  // Video loaded successfully
  video.addEventListener("loadeddata", function () {
    console.log("Video loaded successfully");
    hasLoaded = true;
    clearTimeout(loadTimeout);
    hideVideoFallback();
  });

  video.addEventListener("canplay", function () {
    console.log("Video can play");
    hasLoaded = true;
    clearTimeout(loadTimeout);
    hideVideoFallback();
  });

  // Video error handling
  video.addEventListener("error", function (e) {
    console.log("Video error:", e);
    clearTimeout(loadTimeout);
    showVideoFallback();
  });

  // Handle source errors
  const sources = video.querySelectorAll("source");
  let failedSources = 0;

  sources.forEach((source, index) => {
    source.addEventListener("error", function () {
      console.log(`Source ${index + 1} failed:`, source.src);
      failedSources++;

      // If all sources failed, show fallback
      if (failedSources >= sources.length && !hasLoaded) {
        console.log("All video sources failed");
        clearTimeout(loadTimeout);
        showVideoFallback();
      }
    });
  });

  // Try to load the video
  video.load();

  // Attempt to play (in case autoplay is blocked)
  const playPromise = video.play();
  if (playPromise !== undefined) {
    playPromise.catch((error) => {
      console.log("Autoplay was prevented:", error);
      // Video might still be working, just autoplay was blocked
      // Don't show fallback just because autoplay failed
    });
  }
}

// Initialize when DOM is ready
if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", initializeVideo);
} else {
  initializeVideo();
}
