(() => {
  const revealElements = document.querySelectorAll(".reveal");
  document.documentElement.classList.add("js-enabled");
  const navToggle = document.getElementById("siteNavToggle");
  const siteNav = document.getElementById("siteNav");

  if (navToggle instanceof HTMLButtonElement && siteNav instanceof HTMLElement) {
    const mobileNavQuery = window.matchMedia("(max-width: 900px)");

    const closeNav = () => {
      navToggle.setAttribute("aria-expanded", "false");
      siteNav.classList.remove("is-open");
    };

    navToggle.addEventListener("click", () => {
      const isOpen = navToggle.getAttribute("aria-expanded") === "true";
      navToggle.setAttribute("aria-expanded", String(!isOpen));
      siteNav.classList.toggle("is-open", !isOpen);
    });

    siteNav.querySelectorAll("a").forEach((link) => {
      link.addEventListener("click", () => {
        if (mobileNavQuery.matches) {
          closeNav();
        }
      });
    });

    window.addEventListener("keydown", (event) => {
      if (event.key === "Escape") {
        closeNav();
      }
    });

    mobileNavQuery.addEventListener("change", (event) => {
      if (!event.matches) {
        closeNav();
      }
    });
  }

  if ("IntersectionObserver" in window) {
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add("is-visible");
          }
        });
      },
      { threshold: 0.18 }
    );

    revealElements.forEach((element) => observer.observe(element));
  } else {
    revealElements.forEach((element) => element.classList.add("is-visible"));
  }

  const sections = Array.from(document.querySelectorAll("main section[id]"));
  const navLinks = Array.from(document.querySelectorAll(".site-nav a"));

  if (sections.length === 0 || navLinks.length === 0) {
    return;
  }

  const activateNav = () => {
    const currentSection = sections.find((section) => {
      const rect = section.getBoundingClientRect();
      return rect.top <= 140 && rect.bottom >= 140;
    });

    navLinks.forEach((navLink) => {
      const isCurrent = currentSection && navLink.getAttribute("href") === `#${currentSection.id}`;
      navLink.classList.toggle("is-active", Boolean(isCurrent));
    });
  };

  activateNav();
  window.addEventListener("scroll", activateNav, { passive: true });
  window.addEventListener("resize", activateNav);
})();
