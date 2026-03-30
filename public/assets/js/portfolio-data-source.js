(() => {
  const DEFAULT_CONFIG = {
    dataSource: {
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
    }
  };

  function isPlainObject(value) {
    return Boolean(value) && typeof value === "object" && !Array.isArray(value);
  }

  function deepMerge(baseValue, overrideValue) {
    if (Array.isArray(baseValue)) {
      return Array.isArray(overrideValue) ? overrideValue.slice() : baseValue.slice();
    }

    if (isPlainObject(baseValue)) {
      const result = {};
      const overrideObject = isPlainObject(overrideValue) ? overrideValue : {};
      const keys = new Set([
        ...Object.keys(baseValue),
        ...Object.keys(overrideObject)
      ]);

      keys.forEach((key) => {
        result[key] = deepMerge(baseValue[key], overrideObject[key]);
      });

      return result;
    }

    return overrideValue ?? baseValue;
  }

  function resolveConfig() {
    return deepMerge(DEFAULT_CONFIG, window.PORTFOLIO_APP_CONFIG || {});
  }

  function getApiConfig() {
    const config = resolveConfig();

    if (!config.dataSource || config.dataSource.mode !== "api") {
      throw new Error("Portfolio app is configured without API mode.");
    }

    return config.dataSource.api || {};
  }

  function getModeLabel() {
    return "Remote API / DB";
  }

  function normalizeLoadedData(rawData, helpers) {
    const {
      defaultData,
      mergePortfolioData,
      normalizePortfolioData
    } = helpers;

    if (!isPlainObject(rawData)) {
      return normalizePortfolioData(defaultData);
    }

    return normalizePortfolioData(
      mergePortfolioData(defaultData, rawData),
      rawData
    );
  }

  function extractEnvelope(payload) {
    if (isPlainObject(payload) && isPlainObject(payload.data)) {
      return {
        data: payload.data,
        savedAt:
          payload.updatedAt ||
          payload.savedAt ||
          payload.lastSavedAt ||
          ""
      };
    }

    return {
      data: payload,
      savedAt: ""
    };
  }

  function buildApiUrl(baseUrl, path) {
    if (!path) {
      throw new Error("API path is not configured.");
    }

    if (/^https?:\/\//.test(path)) {
      return path;
    }

    if (!baseUrl) {
      return path;
    }

    return new URL(path, baseUrl).toString();
  }

  async function requestJson(path, options = {}) {
    const apiConfig = getApiConfig();
    const url = buildApiUrl(apiConfig.baseUrl, path);
    const controller = new AbortController();
    const timeoutMs = Number(apiConfig.timeoutMs) || 8000;
    const timeoutId = window.setTimeout(() => controller.abort(), timeoutMs);

    try {
      const response = await window.fetch(url, {
        method: options.method || "GET",
        headers: Object.assign(
          { Accept: "application/json" },
          options.body ? { "Content-Type": "application/json" } : {},
          options.headers || {}
        ),
        body: options.body,
        credentials: apiConfig.withCredentials ? "include" : "same-origin",
        signal: controller.signal
      });

      const rawText = await response.text();
      const parsedBody = rawText ? JSON.parse(rawText) : null;

      if (!response.ok) {
        const error = new Error(
          (parsedBody && parsedBody.message) || `Request failed with status ${response.status}.`
        );
        error.status = response.status;
        error.body = parsedBody;
        throw error;
      }

      return parsedBody;
    } finally {
      window.clearTimeout(timeoutId);
    }
  }

  async function loadPortfolioData({
    scope = "public",
    defaultData,
    mergePortfolioData,
    normalizePortfolioData
  }) {
    const apiConfig = getApiConfig();
    const path =
      scope === "admin" ? apiConfig.adminDataPath : apiConfig.publicDataPath;
    const payload = await requestJson(path);
    const envelope = extractEnvelope(payload);

    return {
      data: normalizeLoadedData(envelope.data, {
        defaultData,
        mergePortfolioData,
        normalizePortfolioData
      }),
      savedAt: envelope.savedAt || "",
      mode: "api",
      modeLabel: getModeLabel()
    };
  }

  async function savePortfolioData(data) {
    const apiConfig = getApiConfig();
    const payload = await requestJson(apiConfig.adminDataPath, {
      method: apiConfig.saveMethod || "PUT",
      body: JSON.stringify({ data })
    });
    const envelope = extractEnvelope(payload);

    return {
      savedAt: envelope.savedAt || new Date().toISOString(),
      mode: "api",
      modeLabel: getModeLabel()
    };
  }

  async function clearPortfolioData() {
    const apiConfig = getApiConfig();

    await requestJson(apiConfig.adminDataPath, {
      method: apiConfig.clearMethod || "DELETE"
    });

    return {
      mode: "api",
      modeLabel: getModeLabel()
    };
  }

  function describeDataSource() {
    return {
      mode: "api",
      modeLabel: getModeLabel(),
      description: "公開ページと管理画面は API 経由で DB を参照します。"
    };
  }

  window.PORTFOLIO_APP_CONFIG = resolveConfig();
  window.PortfolioDataSource = {
    resolveConfig,
    requestJson,
    loadPortfolioData,
    savePortfolioData,
    clearPortfolioData,
    describeDataSource,
    getModeLabel
  };
})();
