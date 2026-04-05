(() => {
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
    editorForm.addEventListener("submit", () => {
      selectionSyncers.forEach((sync) => sync());
    });
  }

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
