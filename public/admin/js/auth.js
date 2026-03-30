(() => {
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

    return { ok: true };
  }

  function getApiPath(key) {
    const config =
      (window.PORTFOLIO_APP_CONFIG &&
        window.PORTFOLIO_APP_CONFIG.dataSource &&
        window.PORTFOLIO_APP_CONFIG.dataSource.api) ||
      {};

    return config[key];
  }

  async function login(loginId, password) {
    return window.PortfolioDataSource.requestJson(getApiPath("loginPath"), {
      method: "POST",
      body: JSON.stringify({
        id: String(loginId || "").trim(),
        password: String(password || "").trim()
      })
    });
  }

  async function logout() {
    try {
      await window.PortfolioDataSource.requestJson(getApiPath("logoutPath"), {
        method: "POST"
      });
    } catch (error) {
      console.warn("Failed to logout from server session.", error);
    }
  }

  async function isAuthenticated() {
    try {
      const response = await window.PortfolioDataSource.requestJson(getApiPath("sessionPath"));
      return Boolean(response && response.authenticated);
    } catch (error) {
      if (error && error.status === 401) {
        return false;
      }

      console.warn("Failed to check admin session.", error);
      return false;
    }
  }

  window.AdminAuth = {
    validateCredentials,
    hasFullWidthCharacters,
    login,
    logout,
    isAuthenticated
  };
})();
