(() => {
  const STORAGE_KEY = window.PORTFOLIO_STORAGE_KEY || "portfolio-site.admin-data";
  const LAST_SAVED_KEY = `${STORAGE_KEY}.last-saved-at`;
  const mergePortfolioData =
    window.mergePortfolioData || ((baseValue, overrideValue) => overrideValue ?? baseValue);
  const defaultData = deepClone(window.defaultPortfolioData || {});
  let state = deepClone(mergePortfolioData(defaultData, window.portfolioData || defaultData));
  let isDirty = false;

  const refs = {
    app: document.getElementById("adminApp"),
    adminShell: document.getElementById("adminShell"),
    status: document.getElementById("statusMessage"),
    saveButton: document.getElementById("saveButton"),
    exportButton: document.getElementById("exportButton"),
    importButton: document.getElementById("importButton"),
    resetButton: document.getElementById("resetButton"),
    clearButton: document.getElementById("clearButton"),
    backToPortalButton: document.getElementById("backToPortalButton"),
    logoutButton: document.getElementById("logoutButton"),
    importFile: document.getElementById("importFile")
  };

  function deepClone(value) {
    return JSON.parse(JSON.stringify(value));
  }

  function createElement(tagName, options = {}) {
    const element = document.createElement(tagName);

    if (options.className) {
      element.className = options.className;
    }

    if (options.text) {
      element.textContent = options.text;
    }

    if (options.html) {
      element.innerHTML = options.html;
    }

    if (options.type) {
      element.type = options.type;
    }

    if (options.placeholder) {
      element.placeholder = options.placeholder;
    }

    if (options.rows) {
      element.rows = options.rows;
    }

    if (options.href) {
      element.href = options.href;
    }

    if (options.value !== undefined) {
      element.value = options.value;
    }

    return element;
  }

  function getAtPath(target, path) {
    return path.split(".").reduce((current, key) => current && current[key], target);
  }

  function setAtPath(target, path, value) {
    const keys = path.split(".");
    let current = target;

    keys.slice(0, -1).forEach((key) => {
      if (!current[key] || typeof current[key] !== "object") {
        current[key] = {};
      }

      current = current[key];
    });

    current[keys[keys.length - 1]] = value;
  }

  function asArray(value, fallback = []) {
    return Array.isArray(value) ? value : fallback;
  }

  function setStatus(message, type = "") {
    refs.status.textContent = message;
    refs.status.className = "status-message";

    if (type) {
      refs.status.classList.add(`is-${type}`);
    }
  }

  function markDirty(message = "未保存の変更があります。") {
    isDirty = true;
    updateSaveButtonState();
    setStatus(message, "warning");
  }

  function updateSaveButtonState() {
    refs.saveButton.disabled = !isDirty;
    refs.saveButton.setAttribute("aria-disabled", String(!isDirty));
  }

  function readLastSavedAt() {
    return window.localStorage.getItem(LAST_SAVED_KEY);
  }

  function writeLastSavedAt(value) {
    window.localStorage.setItem(LAST_SAVED_KEY, value);
  }

  function formatSavedAt(value) {
    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
      return "";
    }

    return new Intl.DateTimeFormat("ja-JP", {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
      second: "2-digit"
    }).format(date);
  }

  function textField(label, path, options = {}) {
    const wrapper = createElement("div", {
      className: `field${options.fullWidth ? " field-full" : ""}`
    });
    const labelElement = createElement("label", { text: label });
    const input = createElement(options.multiline ? "textarea" : "input", {
      type: options.multiline ? undefined : options.type || "text",
      rows: options.multiline ? options.rows || 5 : undefined,
      placeholder: options.placeholder || "",
      value: getAtPath(state, path) || ""
    });

    input.addEventListener("input", () => {
      setAtPath(state, path, input.value);
      markDirty();
    });

    wrapper.appendChild(labelElement);
    wrapper.appendChild(input);

    if (options.help) {
      wrapper.appendChild(createElement("p", { className: "field-help", text: options.help }));
    }

    return wrapper;
  }

  function createSection(title, description) {
    const section = createElement("section", { className: "admin-section" });
    const header = createElement("div", { className: "section-header" });
    header.appendChild(createElement("p", { className: "section-eyebrow", text: "Editor" }));
    header.appendChild(createElement("h2", { text: title }));

    if (description) {
      header.appendChild(createElement("p", { className: "section-description", text: description }));
    }

    section.appendChild(header);
    return section;
  }

  function createFieldGrid(singleColumn = false) {
    return createElement("div", {
      className: `field-grid${singleColumn ? " is-single" : ""}`
    });
  }

  function bindObjectField(parent, item, key, label, options = {}) {
    const wrapper = createElement("div", { className: "field" });
    const labelElement = createElement("label", { text: label });
    const input = createElement(options.multiline ? "textarea" : "input", {
      type: options.multiline ? undefined : options.type || "text",
      rows: options.multiline ? options.rows || 5 : undefined,
      value: item[key] || "",
      placeholder: options.placeholder || ""
    });

    input.addEventListener("input", () => {
      item[key] = input.value;
      markDirty();
    });

    wrapper.appendChild(labelElement);
    wrapper.appendChild(input);

    if (options.help) {
      wrapper.appendChild(createElement("p", { className: "field-help", text: options.help }));
    }

    parent.appendChild(wrapper);
  }

  function addCollection(section, config) {
    const wrapper = createElement("div", { className: "collection" });
    const header = createElement("div", { className: "collection-header" });
    const texts = createElement("div");
    texts.appendChild(createElement("h3", { text: config.title }));

    if (config.description) {
      texts.appendChild(
        createElement("p", { className: "section-description", text: config.description })
      );
    }

    const addButton = createElement("button", {
      className: "button button-secondary",
      type: "button",
      text: config.addLabel
    });

    addButton.addEventListener("click", () => {
      const items = asArray(config.getItems()).slice();
      items.push(config.createItem());
      config.setItems(items);
      markDirty();
      render();
    });

    header.appendChild(texts);
    header.appendChild(addButton);
    wrapper.appendChild(header);

    const list = createElement("div", { className: "item-list" });
    const items = asArray(config.getItems());

    if (items.length === 0) {
      list.appendChild(
        createElement("p", {
          className: "empty-message",
          text: "まだ項目がありません。"
        })
      );
    }

    items.forEach((item, index) => {
      const card = createElement("article", { className: "item-card" });
      const cardHeader = createElement("div", { className: "item-card-header" });
      const headingText =
        typeof config.itemTitle === "function"
          ? config.itemTitle(item, index)
          : `${config.itemTitle} ${index + 1}`;

      cardHeader.appendChild(createElement("h3", { text: headingText }));

      const actions = createElement("div", { className: "inline-actions" });
      const removeButton = createElement("button", {
        className: "button button-danger",
        type: "button",
        text: "削除"
      });

      removeButton.addEventListener("click", () => {
        const nextItems = asArray(config.getItems()).slice();
        nextItems.splice(index, 1);
        config.setItems(nextItems);
        markDirty();
        render();
      });

      actions.appendChild(removeButton);
      cardHeader.appendChild(actions);
      card.appendChild(cardHeader);
      config.renderItem(card, item, index);
      list.appendChild(card);
    });

    wrapper.appendChild(list);
    section.appendChild(wrapper);
  }

  function renderBasicSection() {
    const section = createSection("基本設定", "サイトタイトル、固定情報、ヒーロー表示を編集します。");
    const grid = createFieldGrid();

    grid.appendChild(textField("ブラウザタイトル", "siteTitle"));
    grid.appendChild(textField("メタディスクリプション", "metaDescription"));
    grid.appendChild(textField("名前", "profile.name"));
    grid.appendChild(textField("イニシャル", "profile.shortName"));
    grid.appendChild(textField("肩書き", "profile.role"));
    grid.appendChild(textField("Hero 上部テキスト", "profile.heroEyebrow"));
    grid.appendChild(textField("Hero タイトル", "profile.heroTitle"));
    grid.appendChild(textField("Hero 概要", "profile.summary", { multiline: true, rows: 5, fullWidth: true }));
    grid.appendChild(
      textField("タグ一覧", "profile.tags", {
        multiline: true,
        rows: 6,
        help: "1 行 1 項目で入力します。",
        fullWidth: true
      })
    );

    section.appendChild(grid);

    addCollection(section, {
      title: "Hero ハイライト",
      description: "右側カードに表示する項目です。",
      addLabel: "ハイライト追加",
      itemTitle: (_, index) => `ハイライト ${index + 1}`,
      getItems: () => state.profile.highlights,
      setItems: (items) => {
        state.profile.highlights = items;
      },
      createItem: () => ({ label: "", value: "" }),
      renderItem: (card, item) => {
        const fields = createFieldGrid();
        bindObjectField(fields, item, "label", "ラベル");
        bindObjectField(fields, item, "value", "値");
        card.appendChild(fields);
      }
    });

    return section;
  }

  function renderProfileSection() {
    const section = createSection("プロフィール", "本文や資格、経歴セクションを編集します。");
    const grid = createFieldGrid();

    grid.appendChild(textField("見出し", "profileSection.heading"));
    grid.appendChild(textField("概要文", "profileSection.intro", { multiline: true, rows: 4 }));
    grid.appendChild(textField("リード文", "profileSection.lead", { multiline: true, rows: 4 }));
    grid.appendChild(
      textField("本文", "profileSection.body", {
        multiline: true,
        rows: 8,
        help: "段落を分けたいときは 1 行空けます。"
      })
    );
    grid.appendChild(textField("Focus 見出し", "profileSection.focusHeading"));
    grid.appendChild(
      textField("Focus 項目", "profileSection.focusItems", {
        multiline: true,
        rows: 8,
        help: "1 行 1 項目で入力します。"
      })
    );
    grid.appendChild(textField("資格見出し", "profileSection.certificationsHeading"));
    grid.appendChild(
      textField("資格一覧", "profileSection.certifications", {
        multiline: true,
        rows: 5,
        help: "1 行 1 項目で入力します。"
      })
    );
    grid.appendChild(textField("Career 見出し", "profileSection.timelineHeading"));
    grid.appendChild(textField("Career 概要", "profileSection.timelineIntro", { multiline: true, rows: 4 }));

    section.appendChild(grid);

    addCollection(section, {
      title: "Career 項目",
      description: "タイムラインに表示する経歴です。",
      addLabel: "経歴追加",
      itemTitle: (item, index) => item.title || `経歴 ${index + 1}`,
      getItems: () => state.profileSection.timeline,
      setItems: (items) => {
        state.profileSection.timeline = items;
      },
      createItem: () => ({
        period: "",
        organization: "",
        title: "",
        description: "",
        highlights: ""
      }),
      renderItem: (card, item) => {
        const grid = createFieldGrid();
        bindObjectField(grid, item, "period", "期間");
        bindObjectField(grid, item, "organization", "所属");
        bindObjectField(grid, item, "title", "タイトル");
        bindObjectField(grid, item, "description", "説明", { multiline: true, rows: 4 });
        bindObjectField(grid, item, "highlights", "箇条書き", {
          multiline: true,
          rows: 6,
          help: "1 行 1 項目で入力します。"
        });
        card.appendChild(grid);
      }
    });

    return section;
  }

  function renderSkillsSection() {
    const section = createSection("スキル", "スキルカテゴリと各技術項目を編集します。");
    const grid = createFieldGrid();
    grid.appendChild(textField("見出し", "skillsSection.heading"));
    grid.appendChild(textField("概要文", "skillsSection.intro", { multiline: true, rows: 4 }));
    section.appendChild(grid);

    addCollection(section, {
      title: "スキルカテゴリ",
      description: "カテゴリごとに技術一覧を持てます。",
      addLabel: "カテゴリ追加",
      itemTitle: (item, index) => item.title || `カテゴリ ${index + 1}`,
      getItems: () => state.skillsSection.categories,
      setItems: (items) => {
        state.skillsSection.categories = items;
      },
      createItem: () => ({ title: "", summary: "", items: [] }),
      renderItem: (card, item) => {
        const grid = createFieldGrid();
        bindObjectField(grid, item, "title", "カテゴリ名");
        bindObjectField(grid, item, "summary", "概要", { multiline: true, rows: 4 });
        card.appendChild(grid);

        const nested = createElement("div", { className: "collection" });
        const nestedHeader = createElement("div", { className: "collection-header" });
        nestedHeader.appendChild(createElement("h4", { text: "技術項目" }));

        const addButton = createElement("button", {
          className: "button button-secondary",
          type: "button",
          text: "技術追加"
        });

        addButton.addEventListener("click", () => {
          item.items = asArray(item.items);
          item.items.push({ name: "", experience: "", note: "" });
          markDirty();
          render();
        });

        nestedHeader.appendChild(addButton);
        nested.appendChild(nestedHeader);

        const list = createElement("div", { className: "nested-list" });

        asArray(item.items).forEach((skillItem, skillIndex) => {
          const skillCard = createElement("div", { className: "nested-item" });
          const header = createElement("div", { className: "item-card-header" });
          header.appendChild(createElement("h4", { text: skillItem.name || `技術 ${skillIndex + 1}` }));

          const remove = createElement("button", {
            className: "button button-danger",
            type: "button",
            text: "削除"
          });

          remove.addEventListener("click", () => {
            item.items.splice(skillIndex, 1);
            markDirty();
            render();
          });

          header.appendChild(remove);
          skillCard.appendChild(header);

          const skillGrid = createFieldGrid();
          bindObjectField(skillGrid, skillItem, "name", "技術名");
          bindObjectField(skillGrid, skillItem, "experience", "経験レベル");
          bindObjectField(skillGrid, skillItem, "note", "補足", { multiline: true, rows: 3 });
          skillCard.appendChild(skillGrid);
          list.appendChild(skillCard);
        });

        nested.appendChild(list);
        card.appendChild(nested);
      }
    });

    return section;
  }

  function renderWorksSection() {
    const section = createSection("実績", "業務実績のカードを編集します。");
    const grid = createFieldGrid();
    grid.appendChild(textField("見出し", "worksSection.heading"));
    grid.appendChild(textField("概要文", "worksSection.intro", { multiline: true, rows: 4 }));
    section.appendChild(grid);

    addCollection(section, {
      title: "実績一覧",
      description: "WORKS セクションに表示する項目です。",
      addLabel: "実績追加",
      itemTitle: (item, index) => item.title || `実績 ${index + 1}`,
      getItems: () => state.worksSection.items,
      setItems: (items) => {
        state.worksSection.items = items;
      },
      createItem: () => ({
        title: "",
        year: "",
        type: "",
        role: "",
        summary: "",
        responsibilities: "",
        outcomes: "",
        stack: ""
      }),
      renderItem: (card, item) => {
        const workGrid = createFieldGrid();
        bindObjectField(workGrid, item, "title", "タイトル");
        bindObjectField(workGrid, item, "year", "年");
        bindObjectField(workGrid, item, "type", "種別");
        bindObjectField(workGrid, item, "role", "役割");
        bindObjectField(workGrid, item, "summary", "概要", { multiline: true, rows: 4 });
        bindObjectField(workGrid, item, "responsibilities", "担当内容", {
          multiline: true,
          rows: 6,
          help: "1 行 1 項目で入力します。"
        });
        bindObjectField(workGrid, item, "outcomes", "成果", {
          multiline: true,
          rows: 5,
          help: "1 行 1 項目で入力します。"
        });
        bindObjectField(workGrid, item, "stack", "使用技術", {
          multiline: true,
          rows: 5,
          help: "1 行 1 項目で入力します。"
        });
        card.appendChild(workGrid);
      }
    });

    return section;
  }

  function renderPersonalSection() {
    const section = createSection("個人活動", "PERSONAL セクションを編集します。");
    const grid = createFieldGrid();
    grid.appendChild(textField("見出し", "personalSection.heading"));
    grid.appendChild(textField("概要文", "personalSection.intro", { multiline: true, rows: 4 }));
    section.appendChild(grid);

    addCollection(section, {
      title: "個人活動一覧",
      description: "業務外での検証、学習、発信を管理します。",
      addLabel: "個人活動追加",
      itemTitle: (item, index) => item.title || `個人活動 ${index + 1}`,
      getItems: () => state.personalSection.items,
      setItems: (items) => {
        state.personalSection.items = items;
      },
      createItem: () => ({
        category: "",
        title: "",
        summary: "",
        points: "",
        stack: ""
      }),
      renderItem: (card, item) => {
        const personalGrid = createFieldGrid();
        bindObjectField(personalGrid, item, "category", "カテゴリ");
        bindObjectField(personalGrid, item, "title", "タイトル");
        bindObjectField(personalGrid, item, "summary", "概要", { multiline: true, rows: 4 });
        bindObjectField(personalGrid, item, "points", "内容", {
          multiline: true,
          rows: 6,
          help: "1 行 1 項目で入力します。"
        });
        bindObjectField(personalGrid, item, "stack", "使用技術", {
          multiline: true,
          rows: 5,
          help: "1 行 1 項目で入力します。"
        });
        card.appendChild(personalGrid);
      }
    });

    return section;
  }

  function renderContactSection() {
    const section = createSection("連絡先", "メール、フッター、外部リンクを編集します。");
    const grid = createFieldGrid();
    grid.appendChild(textField("見出し", "contact.heading"));
    grid.appendChild(textField("概要文", "contact.note", { multiline: true, rows: 4 }));
    grid.appendChild(textField("メールアドレス", "contact.email"));
    grid.appendChild(textField("フッター文言", "footerRole"));
    section.appendChild(grid);

    addCollection(section, {
      title: "リンク一覧",
      description: "CONTACT に表示する外部リンクです。",
      addLabel: "リンク追加",
      itemTitle: (item, index) => item.label || `リンク ${index + 1}`,
      getItems: () => state.contact.links,
      setItems: (items) => {
        state.contact.links = items;
      },
      createItem: () => ({ label: "", href: "" }),
      renderItem: (card, item) => {
        const contactGrid = createFieldGrid();
        bindObjectField(contactGrid, item, "label", "表示名");
        bindObjectField(contactGrid, item, "href", "URL");
        card.appendChild(contactGrid);
      }
    });

    return section;
  }

  function buildAdminLayout() {
    const fragment = document.createDocumentFragment();
    fragment.appendChild(renderBasicSection());
    fragment.appendChild(renderProfileSection());
    fragment.appendChild(renderSkillsSection());
    fragment.appendChild(renderWorksSection());
    fragment.appendChild(renderPersonalSection());
    fragment.appendChild(renderContactSection());
    return fragment;
  }

  function render() {
    refs.app.replaceChildren(buildAdminLayout());
  }

  function save() {
    try {
      window.localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
      const savedAt = new Date().toISOString();
      writeLastSavedAt(savedAt);
      window.portfolioData = mergePortfolioData(defaultData, state);
      isDirty = false;
      updateSaveButtonState();
      setStatus(
        `保存しました。最終保存: ${formatSavedAt(savedAt)}。トップページを開き直すと反映されます。`,
        "success"
      );
    } catch (error) {
      console.error(error);
      setStatus("保存に失敗しました。", "warning");
    }
  }

  function resetToDefault() {
    state = deepClone(defaultData);
    markDirty("初期値を読み込みました。保存すると反映されます。");
    render();
  }

  function clearStoredData() {
    window.localStorage.removeItem(STORAGE_KEY);
    window.localStorage.removeItem(LAST_SAVED_KEY);
    state = deepClone(defaultData);
    isDirty = false;
    window.portfolioData = deepClone(defaultData);
    updateSaveButtonState();
    render();
    setStatus("保存済みデータを削除しました。現在は初期値を表示しています。", "success");
  }

  function exportJson() {
    const blob = new Blob([JSON.stringify(state, null, 2)], { type: "application/json" });
    const url = URL.createObjectURL(blob);
    const anchor = createElement("a", { href: url });
    anchor.download = "portfolio-data.json";
    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
    URL.revokeObjectURL(url);
    setStatus("JSON を出力しました。", "success");
  }

  function importJson(file) {
    const reader = new FileReader();

    reader.addEventListener("load", () => {
      try {
        const parsed = JSON.parse(String(reader.result || "{}"));

        if (!parsed || typeof parsed !== "object" || Array.isArray(parsed)) {
          throw new Error("Invalid JSON shape.");
        }

        state = deepClone(mergePortfolioData(defaultData, parsed));
        markDirty("JSON を読み込みました。保存すると反映されます。");
        render();
      } catch (error) {
        console.error(error);
        setStatus("JSON の読み込みに失敗しました。", "warning");
      }
    });

    reader.readAsText(file);
  }

  function moveToPortal() {
    if (isDirty) {
      const shouldContinue = window.confirm("未保存の変更があります。ポータルへ戻りますか。");

      if (!shouldContinue) {
        return;
      }
    }

    window.location.href = "./portal.html";
  }

  function logout() {
    if (isDirty) {
      const shouldContinue = window.confirm("未保存の変更があります。ログアウトしますか。");

      if (!shouldContinue) {
        return;
      }
    }

    if (window.AdminAuth) {
      window.AdminAuth.logout();
    }

    window.location.replace("./");
  }

  if (!window.AdminAuth || !window.AdminAuth.isAuthenticated()) {
    window.location.replace("./");
    return;
  }

  refs.saveButton.addEventListener("click", save);
  refs.exportButton.addEventListener("click", exportJson);
  refs.importButton.addEventListener("click", () => refs.importFile.click());
  refs.backToPortalButton.addEventListener("click", moveToPortal);
  refs.logoutButton.addEventListener("click", logout);
  refs.resetButton.addEventListener("click", () => {
    if (window.confirm("初期値を読み込みます。現在の編集中の内容は失われます。よろしいですか。")) {
      resetToDefault();
    }
  });
  refs.clearButton.addEventListener("click", () => {
    if (window.confirm("保存済みデータを削除します。よろしいですか。")) {
      clearStoredData();
    }
  });
  refs.importFile.addEventListener("change", (event) => {
    const [file] = event.target.files || [];

    if (file) {
      importJson(file);
    }

    event.target.value = "";
  });

  window.addEventListener("beforeunload", (event) => {
    if (!isDirty) {
      return;
    }

    event.preventDefault();
    event.returnValue = "";
  });

  window.addEventListener("keydown", (event) => {
    const isSaveShortcut = (event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "s";

    if (!isSaveShortcut) {
      return;
    }

    event.preventDefault();

    if (isDirty) {
      save();
    }
  });

  render();
  updateSaveButtonState();

  const lastSavedAt = formatSavedAt(readLastSavedAt());

  if (lastSavedAt) {
    setStatus(`管理画面を開きました。最終保存: ${lastSavedAt}`, "success");
  } else {
    setStatus("管理画面を開きました。編集内容は保存すると反映されます。", "success");
  }
})();
