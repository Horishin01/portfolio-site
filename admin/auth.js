(() => {
  const ADMIN_SESSION_KEY = "portfolio-site.admin-session";
  const ADMIN_CREDENTIALS = {
    id: "admin",
    password: "0000"
  };

  function hasFullWidthCharacters(value) {
    return /[^\x20-\x7E]/.test(String(value || ""));
  }

  function validateCredentials(loginId, password) {
    const normalizedId = String(loginId || "").trim();
    const normalizedPassword = String(password || "").trim();

    if (!normalizedId || !normalizedPassword) {
      return { ok: false, message: "ID とパスワードを入力してください。" };
    }

    if (hasFullWidthCharacters(normalizedId) || hasFullWidthCharacters(normalizedPassword)) {
      return {
        ok: false,
        message: "ID とパスワードは半角で入力してください。",
        clearPassword: true
      };
    }

    if (
      normalizedId !== ADMIN_CREDENTIALS.id ||
      normalizedPassword !== ADMIN_CREDENTIALS.password
    ) {
      return {
        ok: false,
        message: "ID またはパスワードが違います。",
        clearPassword: true
      };
    }

    return { ok: true };
  }

  function login() {
    window.sessionStorage.setItem(ADMIN_SESSION_KEY, "authenticated");
  }

  function logout() {
    window.sessionStorage.removeItem(ADMIN_SESSION_KEY);
  }

  function isAuthenticated() {
    return window.sessionStorage.getItem(ADMIN_SESSION_KEY) === "authenticated";
  }

  window.AdminAuth = {
    validateCredentials,
    hasFullWidthCharacters,
    login,
    logout,
    isAuthenticated
  };
})();
