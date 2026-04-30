(() => {
  const editorScrollKey = "portfolio-admin-edit-scroll";
  const restoreEditorScrollPosition = () => {
    const editorShell = document.getElementById("adminShell");
    if (!document.body.contains(document.getElementById("editorForm"))) {
      return;
    }

    const rawValue = editorShell?.getAttribute("data-restore-scroll-y") || sessionStorage.getItem(editorScrollKey);
    if (!rawValue) {
      return;
    }

    sessionStorage.removeItem(editorScrollKey);

    const saved = Number.parseInt(rawValue, 10);
    if (!Number.isFinite(saved) || saved < 0) {
      return;
    }

    if ("scrollRestoration" in history) {
      history.scrollRestoration = "manual";
    }

    const restore = () => {
      window.scrollTo({
        top: saved,
        behavior: "auto"
      });
      requestAnimationFrame(() => {
        window.scrollTo({
          top: saved,
          behavior: "auto"
        });
        editorShell?.classList.remove("restore-scroll-pending");
      });
    };

    if (document.readyState === "complete") {
      restore();
      return;
    }

    window.addEventListener("load", restore, { once: true });
  };

  restoreEditorScrollPosition();

  const bindSectionNavigationState = () => {
    const navLinks = Array.from(document.querySelectorAll(".section-nav-link"));
    if (navLinks.length === 0) {
      return;
    }

    const sectionEntries = navLinks
      .map((link) => {
        const href = link.getAttribute("href");
        if (!href || !href.startsWith("#")) {
          return null;
        }

        const section = document.getElementById(href.slice(1));
        return section instanceof HTMLElement ? { link, section } : null;
      })
      .filter(Boolean);

    if (sectionEntries.length === 0) {
      return;
    }

    const activate = (activeSectionId) => {
      sectionEntries.forEach(({ link, section }) => {
        const isActive = section.id === activeSectionId;
        link.classList.toggle("is-active", isActive);
        if (isActive) {
          link.setAttribute("aria-current", "true");
          return;
        }

        link.removeAttribute("aria-current");
      });
    };

    const getCurrentSectionId = () => {
      const markerY = Math.max(120, window.innerHeight * 0.28);
      let current = sectionEntries[0].section.id;

      sectionEntries.forEach(({ section }) => {
        const rect = section.getBoundingClientRect();
        if (rect.top <= markerY) {
          current = section.id;
        }
      });

      return current;
    };

    let ticking = false;
    const update = () => {
      ticking = false;
      activate(getCurrentSectionId());
    };

    const requestUpdate = () => {
      if (ticking) {
        return;
      }

      ticking = true;
      requestAnimationFrame(update);
    };

    navLinks.forEach((link) => {
      link.addEventListener("click", () => {
        const href = link.getAttribute("href");
        if (href?.startsWith("#")) {
          activate(href.slice(1));
        }
      });
    });

    window.addEventListener("scroll", requestUpdate, { passive: true });
    window.addEventListener("resize", requestUpdate);
    update();
  };

  bindSectionNavigationState();

  const bindImagePreview = ({ cardId, mediaId, fileInputId, srcInputId, altInputId, defaultAlt }) => {
    const card = document.getElementById(cardId);
    const media = document.getElementById(mediaId);
    const fileInput = document.getElementById(fileInputId);
    const srcInput = document.getElementById(srcInputId);
    const altInput = altInputId ? document.getElementById(altInputId) : null;
    let objectUrl = null;

    const resolveAlt = () => {
      if (altInput instanceof HTMLInputElement && altInput.value.trim()) {
        return altInput.value.trim();
      }

      return defaultAlt;
    };

    const setPreview = (src) => {
      if (!(card instanceof HTMLElement) || !(media instanceof HTMLImageElement)) {
        return;
      }

      if (src && src.trim()) {
        media.src = src.trim();
        media.alt = resolveAlt();
        media.hidden = false;
        card.classList.add("has-image");
        return;
      }

      media.hidden = true;
      media.removeAttribute("src");
      card.classList.remove("has-image");
    };

    const releaseObjectUrl = () => {
      if (!objectUrl) {
        return;
      }

      URL.revokeObjectURL(objectUrl);
      objectUrl = null;
    };

    if (srcInput instanceof HTMLInputElement) {
      srcInput.addEventListener("input", () => {
        if (fileInput instanceof HTMLInputElement && fileInput.files && fileInput.files.length > 0) {
          return;
        }

        setPreview(srcInput.value);
      });
    }

    if (altInput instanceof HTMLInputElement) {
      altInput.addEventListener("input", () => {
        if (media instanceof HTMLImageElement && !media.hidden) {
          media.alt = resolveAlt();
        }
      });
    }

    if (fileInput instanceof HTMLInputElement) {
      fileInput.addEventListener("change", () => {
        releaseObjectUrl();

        const file = fileInput.files?.[0];
        if (!file) {
          setPreview(srcInput instanceof HTMLInputElement ? srcInput.value : "");
          return;
        }

        objectUrl = URL.createObjectURL(file);
        setPreview(objectUrl);
      });
    }

    window.addEventListener("beforeunload", releaseObjectUrl);
  };

  bindImagePreview({
    cardId: "faviconPreviewCard",
    mediaId: "faviconPreviewMedia",
    fileInputId: "faviconFile",
    srcInputId: "faviconSrcInput",
    defaultAlt: "Favicon preview"
  });

  bindImagePreview({
    cardId: "heroImagePreviewCard",
    mediaId: "heroImagePreviewMedia",
    fileInputId: "heroImageFile",
    srcInputId: "heroImageSrcInput",
    altInputId: "heroImageAltInput",
    defaultAlt: "Hero image preview"
  });

  document.querySelectorAll("[id^='personalImagePreviewCard-']").forEach((card) => {
    const index = card.id.replace("personalImagePreviewCard-", "");

    bindImagePreview({
      cardId: `personalImagePreviewCard-${index}`,
      mediaId: `personalImagePreviewMedia-${index}`,
      fileInputId: `personalImageFile-${index}`,
      srcInputId: `personalImageSrcInput-${index}`,
      altInputId: `personalImageAltInput-${index}`,
      defaultAlt: "Personal activity image preview"
    });
  });

  const selectionSyncers = [];
  const selectionGroups = document.querySelectorAll("[data-selection-group]");

  selectionGroups.forEach((group) => {
    const targetId = group.getAttribute("data-selection-target");
    if (!targetId) {
      return;
    }

    const target = document.getElementById(targetId);
    if (!(target instanceof HTMLInputElement || target instanceof HTMLTextAreaElement)) {
      return;
    }

    const sync = () => {
      const selected = [];
      const checkboxes = group.querySelectorAll('input[type="checkbox"]');

      checkboxes.forEach((checkbox) => {
        if (!(checkbox instanceof HTMLInputElement)) {
          return;
        }

        const wrapper = checkbox.closest(".selection-chip");
        if (wrapper) {
          wrapper.classList.toggle("is-selected", checkbox.checked);
        }

        if (checkbox.checked && checkbox.value.trim()) {
          selected.push(checkbox.value.trim());
        }
      });

      target.value = selected.join("\n");

      const summary = document.querySelector(`[data-selection-summary-for="${targetId}"]`);
      if (summary) {
        summary.textContent = String(selected.length);
      }
    };

    group.addEventListener("change", (event) => {
      if (event.target instanceof HTMLInputElement && event.target.type === "checkbox") {
        sync();
      }
    });

    selectionSyncers.push(sync);
    sync();
  });

  const editorForm = document.getElementById("editorForm");
  if (editorForm instanceof HTMLFormElement) {
    editorForm.addEventListener("submit", (event) => {
      selectionSyncers.forEach((sync) => sync());
      const scrollInput = document.getElementById("editorScrollY");
      if (scrollInput instanceof HTMLInputElement) {
        scrollInput.value = String(window.scrollY);
      }

      const submitter = event.submitter instanceof HTMLElement
        ? event.submitter
        : document.activeElement instanceof HTMLElement
          ? document.activeElement
          : null;

      if (submitter?.getAttribute("name") === "editorAction") {
        sessionStorage.setItem(editorScrollKey, String(window.scrollY));
      }
    });
  }

  document.querySelectorAll("[data-current-year-select]").forEach((select) => {
    if (!(select instanceof HTMLSelectElement)) {
      return;
    }

    const monthTargetId = select.getAttribute("data-current-month-target");
    const monthSelect = monthTargetId ? document.getElementById(monthTargetId) : null;
    if (!(monthSelect instanceof HTMLSelectElement)) {
      return;
    }

    const syncCurrentState = () => {
      const isCurrent = select.value === "現在";
      monthSelect.disabled = isCurrent;
      monthSelect.closest(".career-period-controls")?.classList.toggle("is-current", isCurrent);
      if (isCurrent) {
        monthSelect.value = "";
      }
    };

    select.addEventListener("change", syncCurrentState);
    syncCurrentState();
  });

  document.querySelectorAll("[data-character-count]").forEach((field) => {
    if (!(field instanceof HTMLInputElement || field instanceof HTMLTextAreaElement)) {
      return;
    }

    const counter = field.parentElement?.querySelector(".character-counter");
    if (!counter) {
      return;
    }

    const limit = Number.parseInt(field.getAttribute("data-character-limit") || "", 10);
    const updateCounter = () => {
      const count = field.value.length;
      counter.textContent = Number.isFinite(limit)
        ? `${count} / ${limit} 文字目安`
        : `${count} 文字`;
      counter.classList.toggle("is-over", Number.isFinite(limit) && count > limit);
    };

    field.addEventListener("input", updateCounter);
    updateCounter();
  });

  document.querySelectorAll("[data-personal-tabs]").forEach((scope) => {
    if (!(scope instanceof HTMLElement)) {
      return;
    }

    const tabs = Array.from(scope.querySelectorAll("[data-personal-tab]"));
    const panels = Array.from(scope.querySelectorAll("[data-personal-tab-panel]"));
    const collection = scope.querySelector("[data-personal-item-collection]");
    const collectionEyebrow = scope.querySelector("[data-personal-collection-eyebrow]");
    const collectionTitle = scope.querySelector("[data-personal-collection-title]");
    const storageKey = scope.getAttribute("data-personal-tab-storage-key") || "portfolio-admin-personal-tab";
    const defaultTab = "index";
    const collectionLabels = {
      all: ["全て表示", "個人活動の全ての編集項目を表示"],
      listing: ["一覧カード", "トップページに並ぶカードだけを編集"],
      detail: ["個別ページ", "カード遷移後の詳細ページだけを編集"],
      shared: ["共通素材", "一覧カードと個別ページで共有する素材だけを編集"]
    };

    const activateTab = (tabName) => {
      const resolvedTabName = tabs.some((tab) => tab.getAttribute("data-personal-tab") === tabName)
        ? tabName
        : defaultTab;

      tabs.forEach((tab) => {
        const isActive = tab.getAttribute("data-personal-tab") === resolvedTabName;
        tab.classList.toggle("is-active", isActive);
        tab.setAttribute("aria-selected", String(isActive));
      });

      panels.forEach((panel) => {
        const isActive = resolvedTabName === "all" || panel.getAttribute("data-personal-tab-panel") === resolvedTabName;
        panel.classList.toggle("is-active", isActive);
        panel.setAttribute("aria-hidden", String(!isActive));
      });

      if (collection instanceof HTMLElement) {
        collection.classList.toggle("is-hidden", resolvedTabName === "index");
      }

      const labels = collectionLabels[resolvedTabName];
      if (labels && collectionEyebrow && collectionTitle) {
        collectionEyebrow.textContent = labels[0];
        collectionTitle.textContent = labels[1];
      }

      scope.setAttribute("data-active-personal-tab", resolvedTabName);
      sessionStorage.setItem(storageKey, resolvedTabName);
    };

    tabs.forEach((tab) => {
      tab.addEventListener("click", (event) => {
        event.preventDefault();
        activateTab(tab.getAttribute("data-personal-tab") || defaultTab);
      });
    });

    const syncStickyTop = () => {
      const commandHeader = document.querySelector(".editor-command-header");
      const statusBar = document.querySelector(".editor-status-bar");
      const visibleBottoms = [commandHeader, statusBar]
        .filter((element) => element instanceof HTMLElement)
        .map((element) => element.getBoundingClientRect())
        .filter((rect) => rect.bottom > 0 && rect.top < window.innerHeight)
        .map((rect) => rect.bottom);

      const stickyTop = visibleBottoms.length > 0
        ? Math.max(...visibleBottoms) + 8
        : 12;

      scope.style.setProperty("--personal-tab-sticky-top", `${stickyTop}px`);
    };

    syncStickyTop();
    window.addEventListener("scroll", syncStickyTop, { passive: true });
    window.addEventListener("resize", syncStickyTop);

    activateTab(sessionStorage.getItem(storageKey) || defaultTab);
  });

  const toggleButtons = document.querySelectorAll("[data-password-toggle]");

  toggleButtons.forEach((button) => {
    button.addEventListener("click", () => {
      const targetId = button.getAttribute("data-password-target");
      if (!targetId) {
        return;
      }

      const input = document.getElementById(targetId);
      if (!(input instanceof HTMLInputElement)) {
        return;
      }

      const isPassword = input.type === "password";
      input.type = isPassword ? "text" : "password";
      button.setAttribute("aria-pressed", String(isPassword));

      const icon = button.querySelector("i");
      if (icon) {
        icon.className = isPassword ? "fa-regular fa-eye" : "fa-regular fa-eye-slash";
      }
    });
  });

  const confirmForms = document.querySelectorAll("form[data-confirm]");
  confirmForms.forEach((form) => {
    form.addEventListener("submit", (event) => {
      const message = form.getAttribute("data-confirm");
      if (message && !window.confirm(message)) {
        event.preventDefault();
      }
    });
  });
})();
