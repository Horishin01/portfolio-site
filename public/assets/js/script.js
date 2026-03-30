;(async () => {
let data = window.portfolioData;

if (window.loadPortfolioData) {
  try {
    const result = await window.loadPortfolioData({ scope: "public" });
    data = result.data || data;

    if (result.warning) {
      console.warn(result.warning);
    }
  } catch (error) {
    console.warn("Failed to load portfolio data from configured source.", error);
  }
}

if (!data) {
  console.error("portfolioData is not defined.");
}

const profileData = (data && data.profile) || {};
const profileSectionData = (data && data.profileSection) || {};
const careerSectionData = (data && data.careerSection) || {};
const skillsSectionData = (data && data.skillsSection) || {};
const worksSectionData = (data && data.worksSection) || {};
const personalSectionData = (data && data.personalSection) || {};
const contactData = (data && data.contact) || {};

const stripListMarker = (value) => String(value || "").replace(/^[\s*+-]+/, "").trim();

const normalizeListItems = (value) => {
  if (Array.isArray(value)) {
    return value.map(stripListMarker).filter(Boolean);
  }

  if (typeof value === "string") {
    return value
      .split("\n")
      .map(stripListMarker)
      .filter(Boolean);
  }

  return [];
};

const normalizeParagraphs = (value) => {
  if (Array.isArray(value)) {
    return value.map((item) => String(item || "").trim()).filter(Boolean);
  }

  if (typeof value === "string") {
    return value
      .trim()
      .split(/\n\s*\n/)
      .map((item) => item.replace(/\n+/g, " ").trim())
      .filter(Boolean);
  }

  return [];
};

const clearChildren = (target) => {
  if (!target) {
    return;
  }

  while (target.firstChild) {
    target.removeChild(target.firstChild);
  }
};

const text = (id, value) => {
  const target = document.getElementById(id);

  if (target && value !== undefined && value !== null) {
    target.textContent = value;
  }
};

const isExternalLink = (href) => /^https?:\/\//.test(href);

const applyLinkBehavior = (target, href) => {
  if (!target || !href) {
    return;
  }

  if (isExternalLink(href)) {
    target.target = "_blank";
    target.rel = "noreferrer";
    return;
  }

  target.removeAttribute("target");
  target.removeAttribute("rel");
};

const createList = (items, className) => {
  const list = document.createElement("ul");
  list.className = className;

  normalizeListItems(items).forEach((item) => {
    const li = document.createElement("li");
    li.textContent = item;
    list.appendChild(li);
  });

  return list;
};

const createParagraphs = (items) => {
  const fragment = document.createDocumentFragment();

  normalizeParagraphs(items).forEach((item) => {
    const paragraph = document.createElement("p");
    paragraph.textContent = item;
    fragment.appendChild(paragraph);
  });

  return fragment;
};

const initialsFromName = (name) =>
  String(name || "")
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0])
    .join("")
    .toUpperCase()
    .slice(0, 2);

document.documentElement.lang = (data && data.locale) || "ja";

if (data && data.siteTitle) {
  document.title = data.siteTitle;
}

const descriptionMeta = document.getElementById("metaDescription");
if (descriptionMeta && data && data.metaDescription) {
  descriptionMeta.setAttribute("content", data.metaDescription);
}

text("brandName", profileData.name);
text("brandRole", profileData.role);
text("brandMark", profileData.shortName || initialsFromName(profileData.name));
text("heroEyebrow", profileData.heroEyebrow);
text("heroTitle", profileData.heroTitle);
text("heroSummary", profileData.summary);
text("profileHeading", profileSectionData.heading);
text("profileIntro", profileSectionData.intro);
text("profileLead", profileSectionData.lead);
text("focusHeading", profileSectionData.focusHeading);
text("certificationsHeading", profileSectionData.certificationsHeading);
text("careerHeading", careerSectionData.heading);
text("careerIntro", careerSectionData.intro);
text("skillsHeading", skillsSectionData.heading);
text("skillsIntro", skillsSectionData.intro);
text("worksHeading", worksSectionData.heading);
text("worksIntro", worksSectionData.intro);
text("personalHeading", personalSectionData.heading);
text("personalIntro", personalSectionData.intro);
text("contactHeading", contactData.heading);
text("contactNote", contactData.note);
text("footerName", profileData.name);
text("footerRole", data && data.footerRole);
text("heroImageFallbackMark", profileData.shortName || initialsFromName(profileData.name));

const heroSection = document.getElementById("heroSection");
const heroVisualCard = document.getElementById("heroVisualCard");
const heroImage = document.getElementById("heroImage");
const heroImagePlaceholder = document.getElementById("heroImagePlaceholder");

const showHeroImageFallback = () => {
  if (heroSection) {
    heroSection.classList.remove("has-hero-image");
  }

  if (heroVisualCard) {
    heroVisualCard.classList.remove("has-image");
  }

  if (heroImage) {
    heroImage.hidden = true;
    heroImage.removeAttribute("src");
  }

  if (heroImagePlaceholder) {
    heroImagePlaceholder.hidden = false;
  }
};

if (heroImage) {
  heroImage.addEventListener("error", showHeroImageFallback);
}

if (profileData.heroImageSrc && heroImage && heroVisualCard && heroImagePlaceholder) {
  if (heroSection) {
    heroSection.classList.add("has-hero-image");
  }

  heroVisualCard.classList.add("has-image");
  heroImage.hidden = false;
  heroImagePlaceholder.hidden = true;
  heroImage.alt = profileData.heroImageAlt || `${profileData.name || "Profile"} image`;
  heroImage.src = profileData.heroImageSrc;
} else {
  showHeroImageFallback();
}

const heroTags = document.getElementById("heroTags");
if (heroTags) {
  clearChildren(heroTags);
  normalizeListItems(profileData.tags).forEach((tag) => {
    const item = document.createElement("li");
    item.textContent = tag;
    heroTags.appendChild(item);
  });
}

const heroHighlights = document.getElementById("heroHighlights");
if (heroHighlights) {
  clearChildren(heroHighlights);
  (profileData.highlights || []).forEach((entry) => {
    const article = document.createElement("article");
    const label = document.createElement("span");
    label.textContent = entry.label;
    const value = document.createElement("strong");
    value.textContent = entry.value;
    article.appendChild(label);
    article.appendChild(value);
    heroHighlights.appendChild(article);
  });
}

const profileBody = document.getElementById("profileBody");
if (profileBody) {
  clearChildren(profileBody);
  profileBody.appendChild(createParagraphs(profileSectionData.body || profileSectionData.paragraphs));
}

const profileFocusList = document.getElementById("profileFocusList");
if (profileFocusList) {
  clearChildren(profileFocusList);
  normalizeListItems(profileSectionData.focusItems).forEach((item) => {
    const li = document.createElement("li");
    li.textContent = item;
    profileFocusList.appendChild(li);
  });
}

const certificationList = document.getElementById("certificationList");
if (certificationList) {
  clearChildren(certificationList);
  normalizeListItems(profileSectionData.certifications).forEach((item) => {
    const li = document.createElement("li");
    li.textContent = item;
    certificationList.appendChild(li);
  });
}

const careerTimeline = document.getElementById("careerTimeline");
if (careerTimeline) {
  clearChildren(careerTimeline);
  (careerSectionData.items || []).forEach((entry) => {
    const article = document.createElement("article");
    article.className = "timeline-item reveal";

    const side = document.createElement("div");
    side.className = "timeline-side";
    const period = document.createElement("span");
    period.className = "timeline-period";
    period.textContent = entry.period || "";
    side.appendChild(period);

    if (entry.category) {
      const category = document.createElement("span");
      category.className = "timeline-kind";
      category.textContent = entry.category;
      side.appendChild(category);
    }

    const content = document.createElement("div");
    content.className = "timeline-content";

    const organization = document.createElement("h4");
    organization.textContent = entry.organization || "";
    content.appendChild(organization);

    if (entry.title) {
      const title = document.createElement("p");
      title.className = "timeline-role";
      title.textContent = entry.title;
      content.appendChild(title);
    }

    if (entry.description) {
      const description = document.createElement("p");
      description.textContent = entry.description;
      content.appendChild(description);
    }

    if (normalizeListItems(entry.highlights).length > 0) {
      content.appendChild(createList(entry.highlights, "timeline-highlights"));
    }

    article.appendChild(side);
    article.appendChild(content);
    careerTimeline.appendChild(article);
  });
}

const skillCategoryList = document.getElementById("skillCategoryList");
if (skillCategoryList) {
  clearChildren(skillCategoryList);
  (skillsSectionData.categories || []).forEach((category) => {
    const article = document.createElement("article");
    article.className = "skill-card reveal";

    const head = document.createElement("div");
    head.className = "skill-card-head";

    const title = document.createElement("h3");
    title.textContent = category.title;

    const summary = document.createElement("p");
    summary.textContent = category.summary;

    head.appendChild(title);
    head.appendChild(summary);

    const table = document.createElement("div");
    table.className = "skill-table";

    const tableHead = document.createElement("div");
    tableHead.className = "skill-table-row skill-table-head";
    tableHead.innerHTML = "<span>技術</span><span>経験</span><span>補足</span>";
    table.appendChild(tableHead);

    (category.items || []).forEach((item) => {
      const row = document.createElement("div");
      row.className = "skill-table-row";

      const name = document.createElement("strong");
      name.textContent = item.name;

      const experience = document.createElement("span");
      experience.className = "skill-experience";
      experience.textContent = item.experience;

      const note = document.createElement("p");
      note.textContent = item.note;

      row.appendChild(name);
      row.appendChild(experience);
      row.appendChild(note);
      table.appendChild(row);
    });

    article.appendChild(head);
    article.appendChild(table);
    skillCategoryList.appendChild(article);
  });
}

const worksList = document.getElementById("worksList");
if (worksList) {
  clearChildren(worksList);
  (worksSectionData.items || []).forEach((work) => {
    const article = document.createElement("article");
    article.className = "work-card reveal";

    const summary = document.createElement("div");
    summary.className = "work-summary";
    summary.innerHTML = `
      <div class="work-meta">
        <span>${work.type}</span>
        <span>${work.year}</span>
      </div>
      <h3>${work.title}</h3>
      <p class="work-role">${work.role}</p>
      <p>${work.summary}</p>
    `;

    const detail = document.createElement("div");
    detail.className = "work-detail";

    const responsibilities = document.createElement("section");
    responsibilities.className = "work-block";
    responsibilities.innerHTML = "<span class='work-block-title'>担当内容</span>";
    responsibilities.appendChild(createList(work.responsibilities, "bullet-list"));

    const outcomes = document.createElement("section");
    outcomes.className = "work-block";
    outcomes.innerHTML = "<span class='work-block-title'>成果</span>";
    outcomes.appendChild(createList(work.outcomes, "bullet-list"));

    const stack = document.createElement("section");
    stack.className = "work-block";
    stack.innerHTML = "<span class='work-block-title'>使用技術</span>";
    stack.appendChild(createList(work.stack, "stack-list"));

    detail.appendChild(responsibilities);
    detail.appendChild(outcomes);
    detail.appendChild(stack);

    article.appendChild(summary);
    article.appendChild(detail);
    worksList.appendChild(article);
  });
}

const personalList = document.getElementById("personalList");
if (personalList) {
  clearChildren(personalList);
  (personalSectionData.items || []).forEach((item) => {
    const article = document.createElement("article");
    article.className = "personal-card reveal";

    const meta = document.createElement("p");
    meta.className = "personal-meta";
    meta.textContent = item.category;

    const title = document.createElement("h3");
    title.textContent = item.title;

    const summary = document.createElement("p");
    summary.className = "personal-summary";
    summary.textContent = item.summary;

    article.appendChild(meta);
    article.appendChild(title);
    article.appendChild(summary);
    article.appendChild(createList(item.points, "bullet-list"));
    article.appendChild(createList(item.stack, "stack-list"));

    personalList.appendChild(article);
  });
}

const contactMail = document.getElementById("contactMail");
if (contactMail) {
  if (contactData.email) {
    contactMail.textContent = contactData.email;
    contactMail.href = `mailto:${contactData.email}`;
  } else {
    contactMail.hidden = true;
  }
}

const contactLinks = document.getElementById("contactLinks");
if (contactLinks) {
  clearChildren(contactLinks);
  (contactData.links || []).forEach((entry) => {
    const anchor = document.createElement("a");
    anchor.href = entry.href;
    anchor.textContent = entry.label;
    applyLinkBehavior(anchor, entry.href);
    contactLinks.appendChild(anchor);
  });
}

const revealElements = document.querySelectorAll(".reveal");
document.documentElement.classList.add("js-enabled");

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
