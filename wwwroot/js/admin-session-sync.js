(() => {
  const channelName = "portfolio-admin-session";
  const storageKey = "portfolio-admin-session-event";
  const sourceId = `${Date.now().toString(36)}-${Math.random().toString(36).slice(2)}`;
  const body = document.body;
  const context = body?.dataset.adminContext || "public";
  const loginPath = "/admin/login";
  let channel = null;

  const isAdminPath = () => window.location.pathname.toLowerCase().startsWith("/admin");
  const isLoginPath = () => window.location.pathname.toLowerCase() === loginPath;

  const hidePublicAdminSession = () => {
    document.querySelectorAll(".admin-session-bar").forEach((element) => {
      if (element instanceof HTMLElement) {
        element.hidden = true;
      }
    });
  };

  const handleRemoteEvent = (eventName) => {
    if (eventName === "logout") {
      hidePublicAdminSession();
      if (isAdminPath() && !isLoginPath()) {
        window.location.assign(loginPath);
      }
      return;
    }

    if (eventName === "account-updated") {
      if (isAdminPath() || document.querySelector(".admin-session-bar")) {
        window.location.reload();
      }
    }
  };

  const publish = (eventName) => {
    if (!eventName) {
      return;
    }

    const payload = {
      eventName,
      sourceId,
      issuedAt: Date.now()
    };

    if (channel) {
      channel.postMessage(payload);
    }

    try {
      localStorage.setItem(storageKey, JSON.stringify(payload));
      localStorage.removeItem(storageKey);
    } catch {
      // localStorage can be unavailable in strict privacy modes.
    }
  };

  if ("BroadcastChannel" in window) {
    channel = new BroadcastChannel(channelName);
    channel.addEventListener("message", (message) => {
      const payload = message.data;
      if (!payload || payload.sourceId === sourceId) {
        return;
      }

      handleRemoteEvent(payload.eventName);
    });
  }

  window.addEventListener("storage", (event) => {
    if (event.key !== storageKey || !event.newValue) {
      return;
    }

    try {
      const payload = JSON.parse(event.newValue);
      if (!payload || payload.sourceId === sourceId) {
        return;
      }

      handleRemoteEvent(payload.eventName);
    } catch {
      return;
    }
  });

  document.querySelectorAll("form[data-admin-session-event-submit]").forEach((form) => {
    form.addEventListener("submit", () => {
      publish(form.getAttribute("data-admin-session-event-submit") || "");
    });
  });

  const serverEvent = body?.dataset.adminSessionEvent || "";
  if (serverEvent) {
    publish(serverEvent);
  }

  if (context === "public" && !document.querySelector(".admin-session-bar")) {
    return;
  }
})();
