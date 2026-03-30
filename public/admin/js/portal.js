;(async () => {
  const refs = {
    openAdminButton: document.getElementById("openAdminButton"),
    portalLogoutButton: document.getElementById("portalLogoutButton")
  };

  if (!window.AdminAuth || !(await window.AdminAuth.isAuthenticated())) {
    window.location.replace("./");
    return;
  }

  refs.openAdminButton.addEventListener("click", () => {
    window.location.href = "./editor.html";
  });

  refs.portalLogoutButton.addEventListener("click", async () => {
    await window.AdminAuth.logout();
    window.location.replace("./");
  });
})();
