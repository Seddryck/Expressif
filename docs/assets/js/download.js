(() => {
  "use strict";

  const root = document.getElementById("download-app");
  if (!root) return;

  const repository = root.dataset.repository;
  const githubApi = `https://api.github.com/repos/${repository}/releases`;
  const state = { releases: [], release: null, section: "cli", method: "tool", choices: {}, packages: [], packageVersions: [], packageVersion: null, packagesStatus: "idle" };

  const escapeHtml = (value) => String(value ?? "").replace(/[&<>'"]/g, (character) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", "'": "&#39;", '"': "&quot;" }[character]));
  const versionOf = (release) => release.tag_name.replace(/^v/i, "");
  const distinct = (items, key) => [...new Map(items.map((item) => [item[key], item[key]])).values()];
  const label = { win: "Windows", linux: "Linux", x64: "x64", arm64: "Arm64", glibc: "Standard Linux / glibc", musl: "Alpine / musl", setup: "Installer", portable: "Portable" };
  const downloadIcon = '<svg class="download-icon" aria-hidden="true" viewBox="0 0 16 16"><path d="M8 1v9m0 0 3.5-3.5M8 10 4.5 6.5M2 12v2h12v-2" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"/></svg>';

  function parseVersion(version) {
    const match = /^(\d+)\.(\d+)\.(\d+)(?:\.(\d+))?(?:-([0-9A-Za-z.-]+))?/.exec(String(version).replace(/^v/i, ""));
    return match ? { major: Number(match[1]), minor: Number(match[2]), patch: Number(match[3]), revision: Number(match[4] || 0), prerelease: match[5] || "" } : null;
  }

  function compareVersions(left, right) {
    const a = parseVersion(left);
    const b = parseVersion(right);
    if (!a || !b) return String(right).localeCompare(String(left), undefined, { numeric: true });
    for (const key of ["major", "minor", "patch", "revision"]) if (a[key] !== b[key]) return b[key] - a[key];
    if (!a.prerelease && b.prerelease) return -1;
    if (a.prerelease && !b.prerelease) return 1;
    return b.prerelease.localeCompare(a.prerelease, undefined, { numeric: true });
  }

  function parseAsset(asset, version) {
    const name = asset.name.toLowerCase();
    const escapedVersion = version.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    const cli = name.match(new RegExp(`^expressif-${escapedVersion}-(net\\d+\\.\\d+)-(win|linux)(-musl)?-(x64|arm64)(-setup)?\\.(zip|tar\\.gz|exe)$`));
    if (cli) return { kind: "cli", framework: cli[1], os: cli[2], libc: cli[2] === "linux" ? (cli[3] ? "musl" : "glibc") : null, arch: cli[4], packageType: cli[5] ? "setup" : "portable", asset };
    const syntax = name.match(new RegExp(`^expressif-syntax-${escapedVersion}-(vscode|textmate|rouge|notepadpp)\\.(vsix|json|zip|xml)$`));
    if (syntax) return { kind: "syntax", target: syntax[1], asset };
    if (name === `expressif-syntax-${version}.vsix` || name === `expressif-syntax-highlighting-${version}.vsix`) return { kind: "syntax", target: "vscode", asset };
    if (name === `expressif-${version}.tmlanguage.json`) return { kind: "syntax", target: "textmate", asset };
    if (name === `expressif-${version}-rouge.zip`) return { kind: "syntax", target: "rouge", asset };
    if (name === `expressif-${version}-notepadpp-udl.xml`) return { kind: "syntax", target: "notepadpp", asset };
    const implementationManifest = name.match(new RegExp(`^expressif-conformance-([a-z][a-z0-9-]*)-${escapedVersion}\\.(ya?ml|json|zip)$`));
    if (implementationManifest) return { kind: "conformance", implementation: implementationManifest[1], asset };
    const conformanceSuite = name.match(/^expressif-conformance-\d+(?:\.\d+){2}(?:[-+][a-z0-9.-]+)?\.(zip|ya?ml|json)$/);
    if (conformanceSuite) return { kind: "conformance", implementation: null, asset };
    if (name === `expressif.conformance.dotnet.${version}.yaml`) return { kind: "conformance", implementation: "dotnet", asset };
    if (/^expressif\.conformance\.\d+(?:\.\d+){2}\.zip$/.test(name)) return { kind: "conformance", implementation: null, asset };
    return null;
  }

  function parsedAssets() {
    const version = versionOf(state.release);
    return state.release.assets.map((asset) => parseAsset(asset, version)).filter(Boolean);
  }

  function button(value, group, text = label[value] || value) {
    const selected = group === "method" ? state.method : state.choices[group];
    return `<button class="download-option" type="button" data-choice="${group}" data-value="${escapeHtml(value)}" aria-pressed="${selected === value}">${escapeHtml(text)}</button>`;
  }

  function reconcileCli() {
    let candidates = parsedAssets().filter((item) => item.kind === "cli");
    for (const key of ["os", "arch", "libc", "framework", "packageType"]) {
      const available = distinct(candidates, key).filter((value) => value !== null);
      if (!available.includes(state.choices[key])) state.choices[key] = available[0] || null;
      if (state.choices[key]) candidates = candidates.filter((item) => item[key] === state.choices[key]);
    }
    return candidates;
  }

  function choiceStep(title, key, candidates, description = "") {
    const values = distinct(candidates, key).filter((value) => value !== null);
    if (values.length < 2) return "";
    return `<div class="download-step"><div class="download-step__label">${title}</div><div class="download-options">${values.map((value) => button(value, key, key === "framework" ? value.replace("net", ".NET ").replace(".0", "") : undefined)).join("")}</div>${description ? `<p class="download-help">${description}</p>` : ""}</div>`;
  }

  function renderNative() {
    const all = parsedAssets().filter((item) => item.kind === "cli");
    if (!all.length) return `<div class="download-notice">No native or portable CLI assets are available for this release.</div>`;
    reconcileCli();
    let candidates = all;
    let html = "";
    for (const [title, key, description] of [
      ["Platform", "os", ""], ["Architecture", "arch", ""],
      ["Linux runtime", "libc", "Standard Linux works for Ubuntu, Debian, Fedora, RHEL, and most mainstream distributions. Choose musl for Alpine Linux."],
      [".NET target", "framework", ""], ["Package type", "packageType", ""]
    ]) {
      html += choiceStep(title, key, candidates, key === "libc" ? description : "");
      if (state.choices[key]) candidates = candidates.filter((item) => item[key] === state.choices[key]);
    }
    const selected = candidates[0];
    if (!selected) return html + `<div class="download-notice">No download matches these choices.</div>`;
    const friendly = [label[selected.os], label[selected.arch], selected.libc === "musl" ? "musl" : null, selected.framework.replace("net", ".NET ")].filter(Boolean).join(" · ");
    return html + `<div class="download-card download-card--recommended"><span class="download-badge">Recommended download</span><h3>Expressif ${escapeHtml(versionOf(state.release))} — ${escapeHtml(friendly)}</h3><a class="download-action" href="${escapeHtml(selected.asset.browser_download_url)}">${downloadIcon}<span>Download ${escapeHtml(label[selected.packageType].toLowerCase())}</span></a><div class="download-filename">${escapeHtml(selected.asset.name)}</div></div>`;
  }

  function commandBlock(command) {
    return `<div class="download-command"><code>${escapeHtml(command)}</code><button class="download-copy" type="button" data-copy="${escapeHtml(command)}">Copy</button></div>`;
  }

  function renderPackageCard(item) {
    const command = commandBlock(`dotnet add package ${item.id} --version ${item.version}`);
    return `<article class="download-card download-package"><h3>${escapeHtml(item.id)}</h3><p class="download-package__description">${escapeHtml(item.description)}</p>${command}<p><a href="${escapeHtml(item.url)}">View on NuGet</a></p></article>`;
  }

  function renderSyntaxCard(item) {
    const details = syntaxDetails[item.target];
    return `<article class="download-card"><h3>${details[0]}</h3><p>${details[1]}</p><a class="download-action" href="${escapeHtml(item.asset.browser_download_url)}">${downloadIcon}<span>Download</span></a><div class="download-filename">${escapeHtml(item.asset.name)}</div></article>`;
  }

  function renderConformanceCard(item) {
    const title = item.implementation ? `${escapeHtml(item.implementation)} implementation manifest` : "Complete suite";
    return `<article class="download-card"><h3>${title}</h3><a class="download-action" href="${escapeHtml(item.asset.browser_download_url)}">${downloadIcon}<span>Download</span></a><div class="download-filename">${escapeHtml(item.asset.name)}</div></article>`;
  }

  function toolPackages() { return state.packages.filter((item) => item.isTool && item.version.toLowerCase() === versionOf(state.release).toLowerCase()); }
  function renderTool() {
    if (state.packagesStatus === "loading" || state.packagesStatus === "idle") return `<div class="download-loading">Loading matching .NET tools from NuGet…</div>`;
    if (state.packagesStatus === "error") return `<div class="download-notice download-notice--error">NuGet package information could not be loaded. Try <a href="https://www.nuget.org/packages?q=Expressif">NuGet search</a> or refresh this page.</div>`;
    const packages = toolPackages();
    if (!packages.length) return `<div class="download-notice">No .NET tool package matching version ${escapeHtml(versionOf(state.release))} was found on NuGet. Try the native / portable distribution or open <a href="https://www.nuget.org/packages?q=Expressif">NuGet search</a>.</div>`;
    return `<div class="download-grid">${packages.map((item, index) => `<article class="download-card download-package ${index === 0 ? "download-card--recommended" : ""}">${index === 0 ? '<span class="download-badge">Recommended</span>' : ""}<h3>${escapeHtml(item.id)}</h3><p class="download-package__description">${escapeHtml(item.description)}</p>${commandBlock(`dotnet tool install --global ${item.id} --version ${item.version}`)}<p><a href="${escapeHtml(item.url)}">View on NuGet</a></p></article>`).join("")}</div>`;
  }

  function renderCli() {
    return `<h2>Expressif CLI</h2><p>Run Expressif from a terminal, scripts, or pipelines.</p><h3>How do you want to install it?</h3><div class="download-options">${button("tool", "method", ".NET global tool")}${button("native", "method", "Native / portable")}</div>${state.method === "tool" ? renderTool() : renderNative()}`;
  }

  function renderPackages() {
    if (state.packagesStatus === "loading" || state.packagesStatus === "idle") return `<h2>.NET packages</h2><div class="download-loading">Loading matching packages from NuGet…</div>`;
    if (state.packagesStatus === "error") return `<h2>.NET packages</h2><div class="download-notice download-notice--error">NuGet package information could not be loaded. Try <a href="https://www.nuget.org/packages?q=Expressif">NuGet search</a> or refresh this page.</div>`;
    const packages = state.packages.filter((item) => !item.isTool && item.version === state.packageVersion);
    const versionOptions = state.packageVersions.map((version) => `<option value="${escapeHtml(version)}" ${version === state.packageVersion ? "selected" : ""}>${escapeHtml(version)}</option>`).join("");
    const versionPicker = state.packageVersions.length > 1 ? `<div class="download-package-version"><label for="download-package-version">Package version</label><select id="download-package-version" data-package-version>${versionOptions}</select><p class="download-help">Patch releases are primarily bug fixes and may be published only to NuGet, without a corresponding GitHub Release. The latest available patch in the ${escapeHtml(versionOf(state.release).split(".").slice(0, 2).join("."))}.x line is selected automatically.</p></div>` : "";
    const packageContent = packages.length ? `<div class="download-grid">${packages.map(renderPackageCard).join("")}</div>` : `<div class="download-notice">No Expressif library package matching version ${escapeHtml(state.packageVersion || versionOf(state.release))} was found on NuGet.</div>`;
    return `<h2>.NET packages</h2><p>Use Expressif from a .NET application. These packages are independent of operating system and architecture.</p>${versionPicker}${packageContent}`;
  }

  const syntaxDetails = { vscode: ["VS Code", "Install Expressif highlighting in Visual Studio Code."], textmate: ["TextMate", "Use the TextMate grammar in compatible editors."], rouge: ["Rouge", "Add Expressif highlighting to Rouge-based sites."], notepadpp: ["Notepad++", "Import the user-defined language into Notepad++."] };
  function renderSyntax() {
    const assets = parsedAssets().filter((item) => item.kind === "syntax");
    const content = assets.length ? `<div class="download-grid">${assets.map(renderSyntaxCard).join("")}</div>` : '<div class="download-notice">No recognized syntax-highlighting assets are available for this release.</div>';
    return `<h2>Syntax highlighting</h2><p>These packages add Expressif language highlighting. They do not install the Expressif runtime or CLI.</p>${content}`;
  }

  function renderConformance() {
    const assets = parsedAssets().filter((item) => item.kind === "conformance");
    const content = assets.length ? `<div class="download-grid">${assets.map(renderConformanceCard).join("")}</div>` : '<div class="download-notice">No recognized conformance assets are available for this release.</div>';
    return `<h2>Conformance assets</h2><p>Use these assets when implementing or validating another Expressif runtime against the official behaviour.</p>${content}`;
  }

  function render() {
    if (!state.release) return;
    const releaseOptions = state.releases.map((release) => `<option value="${escapeHtml(release.id)}" ${release.id === state.release.id ? "selected" : ""}>${escapeHtml(versionOf(release))}${release.prerelease ? " — prerelease" : release === state.releases.find((item) => !item.prerelease) ? " — latest stable" : ""}</option>`).join("");
    const section = { cli: renderCli, packages: renderPackages, syntax: renderSyntax, conformance: renderConformance }[state.section]();
    root.innerHTML = `<div class="download-toolbar"><div class="download-toolbar__row"><label>Version <select data-version>${releaseOptions}</select></label><div class="download-release-links"><a href="${escapeHtml(state.release.html_url)}">Release notes</a><a href="https://github.com/${repository}/releases/tag/${encodeURIComponent(state.release.tag_name)}">GitHub release</a></div></div></div><div class="download-tabs" role="tablist" aria-label="What do you want to use?">${[["cli", "Expressif CLI"], ["packages", ".NET packages"], ["syntax", "Syntax highlighting"], ["conformance", "Conformance assets"]].map(([key, text]) => `<button class="download-tab ${key === "conformance" ? "download-tab--secondary" : ""}" type="button" role="tab" data-section="${key}" aria-selected="${state.section === key}">${text}</button>`).join("")}</div><section class="download-panel" role="tabpanel">${section}</section>`;
  }

  async function loadPackages() {
    const requestedVersion = versionOf(state.release);
    state.packagesStatus = "loading";
    state.packages = [];
    state.packageVersions = [];
    state.packageVersion = null;
    render();
    try {
      const service = await fetch("https://api.nuget.org/v3/index.json").then(checkResponse).then((response) => response.json());
      const searchUrl = service.resources.find((resource) => resource["@type"].split("/").includes("SearchQueryService"))["@id"];
      const results = await fetch(`${searchUrl}?q=${encodeURIComponent("Expressif")}&prerelease=true&take=30`).then(checkResponse).then((response) => response.json());
      const candidates = results.data.filter((item) => /^expressif(?:[.-]|$)/i.test(item.id));
      const packages = await Promise.all(candidates.map(async (item) => {
        const registrationUrl = `https://api.nuget.org/v3/registration5-gz-semver2/${item.id.toLowerCase()}/index.json`;
        const registration = await fetch(registrationUrl).then(checkResponse).then((response) => response.json());
        const pages = await Promise.all(registration.items.map((page) => page.items ? page : fetch(page["@id"]).then(checkResponse).then((response) => response.json())));
        const leaves = pages.flatMap((page) => page.items || []);
        const requested = parseVersion(requestedVersion);
        return leaves.map((leaf) => leaf.catalogEntry).filter((entry) => {
          const candidate = parseVersion(entry.version);
          return candidate?.major === requested?.major && candidate?.minor === requested?.minor;
        }).map((entry) => {
          const packageTypes = (entry.packageTypes || item.packageTypes || []).map((type) => type.name);
          if (packageTypes.includes("DotnetToolRidPackage")) return null;
          return { id: entry.id, version: entry.version, description: entry.description || item.description || "Expressif package", isTool: packageTypes.includes("DotnetTool"), url: `https://www.nuget.org/packages/${encodeURIComponent(entry.id)}/${encodeURIComponent(entry.version)}` };
        }).filter(Boolean);
      }));
      if (requestedVersion !== versionOf(state.release)) return;
      state.packages = packages.flat().sort((a, b) => a.id.localeCompare(b.id));
      state.packageVersions = [...new Set(state.packages.filter((item) => !item.isTool).map((item) => item.version))].sort(compareVersions);
      state.packageVersion = state.packageVersions[0] || null;
      state.packagesStatus = "ready";
    } catch (error) {
      state.packagesStatus = "error";
      console.error("Unable to load NuGet packages", error);
    }
    render();
  }

  function checkResponse(response) { if (!response.ok) throw new Error(`${response.status} ${response.statusText}`); return response; }

  root.addEventListener("click", async (event) => {
    const section = event.target.closest("[data-section]");
    if (section) { state.section = section.dataset.section; render(); return; }
    const choice = event.target.closest("[data-choice]");
    if (choice) {
      if (choice.dataset.choice === "method") state.method = choice.dataset.value;
      else state.choices[choice.dataset.choice] = choice.dataset.value;
      render(); return;
    }
    const copy = event.target.closest("[data-copy]");
    if (copy) { await navigator.clipboard.writeText(copy.dataset.copy); copy.textContent = "Copied"; }
  });

  root.addEventListener("change", (event) => {
    if (event.target.matches("[data-package-version]")) { state.packageVersion = event.target.value; render(); return; }
    if (event.target.matches("[data-version]")) {
      state.release = state.releases.find((release) => String(release.id) === event.target.value);
      state.choices = {};
      loadPackages();
    }
  });

  Promise.all([
    fetch(`${githubApi}?per_page=100`).then(checkResponse).then((response) => response.json()),
    fetch(`${githubApi}/latest`).then(checkResponse).then((response) => response.json())
  ]).then(([releases, latest]) => {
    state.releases = releases.filter((release) => !release.draft);
    state.release = state.releases.find((release) => release.id === latest.id) || state.releases[0];
    render();
    loadPackages();
  }).catch((error) => {
    console.error("Unable to load GitHub releases", error);
    root.innerHTML = `<div class="download-notice download-notice--error">Current release information could not be loaded. Try the <a href="https://github.com/${repository}/releases">GitHub releases page</a> or refresh this page.</div>`;
  });
})();
