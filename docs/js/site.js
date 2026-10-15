(() => {
  const shell = document.querySelector(".invite-shell");
  const openingScreen = document.getElementById("openingScreen");
  const openInvite = document.getElementById("openInvite");
  const musicButton = document.getElementById("musicButton");
  const weddingAudio = document.getElementById("weddingAudio");
  const rsvpForm = document.getElementById("rsvpForm");
  const formStatus = document.getElementById("formStatus");
  const photoDialog = document.getElementById("photoDialog");
  const photoDialogImage = document.getElementById("photoDialogImage");
  const googleMapFrame = document.getElementById("googleMapFrame");
  const googleSheetsUrl = window.weddingRsvpConfig?.googleSheetsUrl?.trim() || "";
  const autoScrollSpeed = Number(shell?.dataset.autoScrollSpeed) || 230;
  const autoScrollDelay = Number(shell?.dataset.autoScrollDelay) || 250;
  let autoScrollFrame;
  let autoScrollDelayTimer;
  let lastAutoScrollTime = 0;

  document.body.classList.add("no-scroll");

  const updateFloatingNav = () => {
    document.body.classList.toggle("nav-visible", window.scrollY > 160);
  };

  if (new URLSearchParams(window.location.search).has("preview")) {
    openingScreen?.classList.add("is-hidden");
    document.body.classList.remove("no-scroll");
    updateFloatingNav();
  }

  const startMusic = async () => {
    if (!weddingAudio) {
      return;
    }

    try {
      weddingAudio.volume = 0.72;
      await weddingAudio.play();
      musicButton?.classList.add("is-playing");
      musicButton?.classList.remove("missing-audio");
    } catch {
      musicButton?.classList.add("missing-audio");
      musicButton?.setAttribute("title", "Dat file nhac tai wwwroot/media/my-love.mp3");
    }
  };

  const stopMusic = () => {
    if (weddingAudio) {
      weddingAudio.pause();
    }

    musicButton?.classList.remove("is-playing");
  };

  const stopAutoScroll = () => {
    window.clearTimeout(autoScrollDelayTimer);
    autoScrollDelayTimer = undefined;
    document.documentElement.classList.remove("auto-scrolling");

    if (autoScrollFrame) {
      window.cancelAnimationFrame(autoScrollFrame);
      autoScrollFrame = undefined;
    }
  };

  const startAutoScroll = () => {
    stopAutoScroll();
    document.documentElement.classList.add("auto-scrolling");
    lastAutoScrollTime = 0;

    const scrollStep = (timestamp) => {
      const scroller = document.scrollingElement || document.documentElement;

      if (document.hidden) {
        lastAutoScrollTime = timestamp;
        autoScrollFrame = window.requestAnimationFrame(scrollStep);
        return;
      }

      if (!lastAutoScrollTime) {
        lastAutoScrollTime = timestamp;
      }

      const elapsed = Math.min(timestamp - lastAutoScrollTime, 120);
      const speedPixelsPerSecond = autoScrollSpeed;
      const maxScrollTop = scroller.scrollHeight - window.innerHeight;

      if (scroller.scrollTop < maxScrollTop - 2) {
        scroller.scrollTop = Math.min(maxScrollTop, scroller.scrollTop + (elapsed / 1000) * speedPixelsPerSecond);
        lastAutoScrollTime = timestamp;
        autoScrollFrame = window.requestAnimationFrame(scrollStep);
      } else {
        stopAutoScroll();
      }
    };

    autoScrollDelayTimer = window.setTimeout(() => {
      autoScrollFrame = window.requestAnimationFrame(scrollStep);
    }, autoScrollDelay);
  };

  const openCard = () => {
    openingScreen?.classList.add("is-hidden");
    document.body.classList.remove("no-scroll");
    document.body.classList.remove("nav-visible");
    window.scrollTo({ top: 0, behavior: "smooth" });
    startMusic();
    startAutoScroll();
  };

  const updateCountdown = () => {
    if (!shell) {
      return;
    }

    const target = new Date(shell.dataset.weddingDate).getTime();
    const distance = Math.max(target - Date.now(), 0);
    const days = Math.floor(distance / (1000 * 60 * 60 * 24));
    const hours = Math.floor((distance / (1000 * 60 * 60)) % 24);
    const minutes = Math.floor((distance / (1000 * 60)) % 60);
    const seconds = Math.floor((distance / 1000) % 60);
    const values = { days, hours, minutes, seconds };

    Object.entries(values).forEach(([id, value]) => {
      const element = document.getElementById(id);
      if (element) {
        element.textContent = String(value).padStart(2, "0");
      }

      const partyElement = document.getElementById(`party${id.charAt(0).toUpperCase()}${id.slice(1)}`);
      if (partyElement) {
        partyElement.textContent = String(value).padStart(2, "0");
      }
    });
  };

  openInvite?.addEventListener("click", openCard);

  musicButton?.addEventListener("click", () => {
    if (weddingAudio && !weddingAudio.paused) {
      stopMusic();
    } else {
      startMusic();
    }
  });

  googleMapFrame?.addEventListener("load", () => {
    window.setTimeout(() => {
      googleMapFrame.closest(".map-frame")?.classList.add("is-loaded");
    }, 600);
  });

  ["wheel", "touchstart", "keydown", "mousedown"].forEach((eventName) => {
    window.addEventListener(eventName, stopAutoScroll, {
      passive: true
    });
  });

  document.querySelectorAll(".floating-nav a, a[href^='#']").forEach((link) => {
    link.addEventListener("click", stopAutoScroll);
  });

  window.addEventListener("scroll", updateFloatingNav, {
    passive: true
  });

  document.querySelectorAll("[data-open-gift]").forEach((button) => {
    button.addEventListener("click", () => {
      document.getElementById(button.dataset.openGift || "")?.showModal();
    });
  });

  document.querySelectorAll("[data-close-gift]").forEach((button) => {
    button.addEventListener("click", () => {
      button.closest("dialog")?.close();
    });
  });

  document.querySelectorAll("[data-gallery]").forEach((button) => {
    button.addEventListener("click", () => {
      if (!photoDialog || !photoDialogImage) {
        return;
      }

      photoDialogImage.src = button.dataset.gallery || "";
      photoDialog.showModal();
    });
  });

  document.querySelector("[data-close-photo]")?.addEventListener("click", () => {
    photoDialog?.close();
  });

  rsvpForm?.addEventListener("submit", async (event) => {
    event.preventDefault();
    const data = new FormData(rsvpForm);
    const guest = data.get("guest")?.toString().trim() || "Bạn";

    if (googleSheetsUrl) {
      const submission = new URLSearchParams({
        guest,
        attending: data.get("attending")?.toString() || "",
        message: data.get("message")?.toString().trim() || "",
        website: data.get("website")?.toString() || ""
      });

      try {
        await fetch(googleSheetsUrl, {
          method: "POST",
          mode: "no-cors",
          body: submission
        });
        formStatus.textContent = "Đã gửi phản hồi thành công. Cảm ơn bạn!";
        rsvpForm.reset();
      } catch {
        formStatus.textContent = "Không gửi được phản hồi. Vui lòng kiểm tra kết nối mạng rồi thử lại.";
      }
      return;
    }

    if (window.location.hostname.endsWith("github.io") || window.location.protocol === "file:") {
      formStatus.textContent = "Biểu mẫu chưa được kết nối Google Sheets nên chưa lưu phản hồi. Vui lòng cấu hình URL Web App trước.";
      return;
    }

    try {
      const response = await fetch(rsvpForm.action, {
        method: "POST",
        body: data,
        headers: {
          "X-Requested-With": "XMLHttpRequest"
        }
      });
      const result = await response.json();

      if (!response.ok || !result.success) {
        formStatus.textContent = result.message || "Chưa thể lưu phản hồi. Vui lòng thử lại.";
        return;
      }

      formStatus.textContent = result.message;
      rsvpForm.reset();
    } catch {
      formStatus.textContent = "Không thể kết nối để lưu phản hồi. Vui lòng thử lại.";
    }
  });

  updateCountdown();
  window.setInterval(updateCountdown, 1000);
})();
