;(async () => {
  const refs = {
    loginForm: document.getElementById("loginForm"),
    loginButton: document.getElementById("loginButton"),
    loginId: document.getElementById("loginId"),
    loginPassword: document.getElementById("loginPassword"),
    passwordVisibilityButton: document.getElementById("passwordVisibilityButton"),
    passwordVisibilityIcon: document.getElementById("passwordVisibilityIcon"),
    loginMessage: document.getElementById("loginMessage")
  };

  function setLoginMessage(message, type = "") {
    refs.loginMessage.textContent = message;
    refs.loginMessage.className = "status-message";

    if (type) {
      refs.loginMessage.classList.add(`is-${type}`);
    }
  }

  function setPasswordVisibility(isVisible) {
    refs.loginPassword.type = isVisible ? "text" : "password";
    refs.passwordVisibilityButton.setAttribute("aria-pressed", String(isVisible));
    refs.passwordVisibilityButton.setAttribute(
      "aria-label",
      isVisible ? "パスワードを隠す" : "パスワードを表示"
    );
    refs.passwordVisibilityIcon.className = isVisible
      ? "fa-regular fa-eye"
      : "fa-regular fa-eye-slash";
  }

  async function handleLoginAttempt() {
    const result = window.AdminAuth.validateCredentials(
      refs.loginId.value,
      refs.loginPassword.value
    );

    if (!result.ok) {
      setLoginMessage(result.message, "error");

      if (result.clearPassword) {
        refs.loginPassword.value = "";
        setPasswordVisibility(false);
        refs.loginPassword.focus();
      }

      return;
    }

    refs.loginButton.disabled = true;
    setLoginMessage("ログイン中です。", "warning");

    try {
      await window.AdminAuth.login(refs.loginId.value, refs.loginPassword.value);
      window.location.replace("./portal.html");
    } catch (error) {
      const message =
        (error && error.body && error.body.message) ||
        "ログインに失敗しました。ID またはパスワードを確認してください。";

      setLoginMessage(message, "error");
      refs.loginPassword.value = "";
      setPasswordVisibility(false);
      refs.loginPassword.focus();
    } finally {
      refs.loginButton.disabled = false;
    }
  }

  if (!window.AdminAuth || !window.PortfolioDataSource) {
    setLoginMessage("認証モジュールの読み込みに失敗しました。", "error");
    return;
  }

  if (await window.AdminAuth.isAuthenticated()) {
    window.location.replace("./portal.html");
    return;
  }

  refs.passwordVisibilityButton.addEventListener("click", (event) => {
    event.preventDefault();
    setPasswordVisibility(refs.loginPassword.type === "password");
  });

  refs.loginForm.addEventListener("submit", (event) => {
    event.preventDefault();
    void handleLoginAttempt();
  });

  refs.loginButton.addEventListener("click", (event) => {
    event.preventDefault();
    void handleLoginAttempt();
  });

  setPasswordVisibility(false);
  setLoginMessage("ID とパスワードを入力してください。");
  refs.loginId.focus();
})();
