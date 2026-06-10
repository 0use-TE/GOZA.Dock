(function () {
  const versionSelect = document.getElementById('goza-version');
  const langSelect = document.getElementById('goza-lang');
  if (!versionSelect || !langSelect) return;

  const rel = document.querySelector('meta[name="docfx:rel"]')?.content ?? '';
  const path = window.location.pathname;

  /** @type {{ default: string, versions: { id: string, label: string }[], pages: string[] }} */
  let config = {
    default: '1.0.2',
    versions: [
      { id: '1.0.2', label: '1.0.2 (latest)' },
      { id: '1.0.1', label: '1.0.1' },
      { id: '1.0.0', label: '1.0.0' },
    ],
    pages: [
      'getting-started',
      'crystal-avalonia',
      'architecture',
      'aot-compatibility',
      'recipes',
      'release-notes',
      'introduction',
      'index',
    ],
  };

  function loadConfig() {
    return fetch(rel + 'public/goza-versions.json')
      .then((r) => (r.ok ? r.json() : config))
      .then((data) => {
        config = data;
        populateVersionSelect();
      })
      .catch(() => populateVersionSelect());
  }

  function populateVersionSelect() {
    versionSelect.replaceChildren();
    for (const v of config.versions) {
      const opt = document.createElement('option');
      opt.value = v.id;
      opt.textContent = v.label;
      versionSelect.appendChild(opt);
    }
  }

  function currentLang() {
    return path.includes('/zh-CN/') ? 'zh-CN' : 'en';
  }

  function currentVersion() {
    const match = path.match(/\/docs\/(\d+\.\d+\.\d+)\//);
    if (match) return match[1];
    return config.default;
  }

  function currentPage() {
    const match = path.match(/\/docs\/\d+\.\d+\.\d+\/(?:zh-CN\/)?([^/]+)\.html/i);
    if (match && config.pages.includes(match[1])) return match[1];
    return 'getting-started';
  }

  function buildDocUrl(version, lang, page) {
    const prefix =
      lang === 'zh-CN' ? `docs/${version}/zh-CN/` : `docs/${version}/`;
    return rel + prefix + page + '.html';
  }

  function syncControls() {
    versionSelect.value = currentVersion();
    langSelect.value = currentLang();
  }

  loadConfig().then(() => {
    syncControls();

    versionSelect.addEventListener('change', () => {
      window.location.href = buildDocUrl(
        versionSelect.value,
        langSelect.value,
        currentPage()
      );
    });

    langSelect.addEventListener('change', () => {
      window.location.href = buildDocUrl(
        versionSelect.value,
        langSelect.value,
        currentPage()
      );
    });
  });
})();
