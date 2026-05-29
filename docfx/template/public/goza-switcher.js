(function () {
  const versionSelect = document.getElementById('goza-version');
  const langSelect = document.getElementById('goza-lang');
  if (!versionSelect || !langSelect) return;

  const rel = document.querySelector('meta[name="docfx:rel"]')?.content ?? '';
  const path = window.location.pathname;

  const docPages = new Set([
    'getting-started',
    'crystal-avalonia',
    'architecture',
    'aot-compatibility',
    'recipes',
    'introduction',
    'index',
  ]);

  function currentLang() {
    return path.includes('/zh-CN/') ? 'zh-CN' : 'en';
  }

  function currentVersion() {
    return path.includes('/v1.0/') ? 'v1.0' : 'v1.0';
  }

  function currentPage() {
    const match = path.match(/\/docs\/v1\.0\/(?:zh-CN\/)?([^/]+)\.html/i);
    if (match && docPages.has(match[1])) return match[1];
    return 'getting-started';
  }

  function buildDocUrl(version, lang, page) {
    if (version !== 'v1.0') return rel + 'index.html';
    const prefix = lang === 'zh-CN' ? 'docs/v1.0/zh-CN/' : 'docs/v1.0/';
    return rel + prefix + page + '.html';
  }

  versionSelect.value = currentVersion();
  langSelect.value = currentLang();

  versionSelect.addEventListener('change', () => {
    const v = versionSelect.value;
    if (v === 'v1.0') {
      window.location.href = buildDocUrl(v, langSelect.value, currentPage());
    } else {
      versionSelect.value = currentVersion();
    }
  });

  langSelect.addEventListener('change', () => {
    window.location.href = buildDocUrl(versionSelect.value, langSelect.value, currentPage());
  });
})();
