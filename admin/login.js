(() => {
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

  function handleLoginAttempt() {
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

    window.AdminAuth.login();
    window.location.replace("./portal.html");
  }

  if (!window.AdminAuth) {
    setLoginMessage("認証モジュールの読み込みに失敗しました。", "error");
    return;
  }

  if (window.AdminAuth.isAuthenticated()) {
    window.location.replace("./portal.html");
    return;
  }

  refs.passwordVisibilityButton.addEventListener("click", (event) => {
    event.preventDefault();
    setPasswordVisibility(refs.loginPassword.type === "password");
  });

  refs.loginForm.addEventListener("submit", (event) => {
    event.preventDefault();
    handleLoginAttempt();
  });

  refs.loginButton.addEventListener("click", (event) => {
    event.preventDefault();
    handleLoginAttempt();
  });

  setPasswordVisibility(false);
  setLoginMessage("ID とパスワードを入力してください。");
  refs.loginId.focus();
})();
