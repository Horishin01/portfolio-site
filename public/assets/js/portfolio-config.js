window.PORTFOLIO_APP_CONFIG = window.PORTFOLIO_APP_CONFIG || {};

window.PORTFOLIO_APP_CONFIG.dataSource = Object.assign(
  {
    mode: "api",
    api: {
      baseUrl: "",
      publicDataPath: "/api/portfolio",
      adminDataPath: "/api/admin/portfolio",
      loginPath: "/api/admin/login",
      logoutPath: "/api/admin/logout",
      sessionPath: "/api/admin/session",
      saveMethod: "PUT",
      clearMethod: "DELETE",
      timeoutMs: 8000,
      withCredentials: true
    }
  },
  window.PORTFOLIO_APP_CONFIG.dataSource || {}
);
