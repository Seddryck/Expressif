---
layout: home
title: Expressif
nav_exclude: true
permalink: /
---

<header class="landing-header">
  <a class="landing-brand" href="{{ '/' | relative_url }}" aria-label="Expressif home">
    <img class="landing-brand__logo" src="{{ '/assets/images/expressif-logo.png' | relative_url }}" alt="Expressif">
    <span>expressif</span>
  </a>
  <nav class="landing-nav" aria-label="Main navigation">
    <a href="{{ '/getting-started/' | relative_url }}">Docs</a><a href="{{ '/download/' | relative_url }}">Download</a><a href="{{ '/language/' | relative_url }}">Language</a><a href="{{ '/tooling/' | relative_url }}">Tooling</a><a href="https://github.com/Seddryck/Expressif">GitHub</a>
  </nav>
  <a class="landing-button landing-button--small" href="{{ '/getting-started/' | relative_url }}">Get started</a>
</header>

<main>
  <section class="landing-hero">
    <div class="landing-hero__copy">
      <p class="landing-eyebrow">An expression language for everyone</p>
      <h1>A language for <span>expressions</span> that matter.</h1>
      <p class="landing-hero__intro">Expressif helps you express data, logic and transformations in a human-friendly way. Clear, predictable and powerful.</p>
      <div class="landing-actions"><a class="landing-button" href="{{ '/getting-started/' | relative_url }}">Read the docs</a><a class="landing-text-link" href="{{ '/language/' | relative_url }}">Learn the language <span aria-hidden="true">→</span></a></div>
    </div>
    <div class="code-window" aria-label="Example Expressif code">
      <div class="code-window__bar"><span>examples/</span><span>customers.expr</span><span aria-hidden="true">×</span></div>
      <div class="code-window__body">
        <div class="code-window__files"><span class="is-active">▤&nbsp;&nbsp; customers.expr</span><span>▱&nbsp;&nbsp; revenue.expr</span><span>▱&nbsp;&nbsp; inventory.expr</span><span>▱&nbsp;&nbsp; README.md</span></div>
        <pre class="code-window__code"><code><i>1</i>  <em>// Filter active customers and</em>
<i>2</i>  <em>// summarize orders</em>
<i>3</i>  @customers
<i>4</i>  <b>|</b> filter(.active)
<i>5</i>  <b>|&gt;</b> record(
<i>6</i>      id: .id,
<i>7</i>      name: .name <b>|</b> upper,
<i>8</i>      total := .orders <b>|&gt;</b> .amount <b>|</b> sum
<i>9</i>    )
<i>10</i> <b>|</b> first-elements(10)</code></pre>
      </div>
      <div class="code-window__status"><span>✓&nbsp; Valid expression</span><span>Ln 10, Col 10</span></div>
    </div>
  </section>

  <section class="feature-strip" aria-label="Expressif qualities">
    <article><div class="feature-icon">◎</div><h2>Readable</h2><p>Easy to read<br>and write.</p></article>
    <article><div class="feature-icon">{a}</div><h2>Predictable</h2><p>Consistent syntax<br>and behavior.</p></article>
    <article><div class="feature-icon">◌</div><h2>Composable</h2><p>Build complex<br>logic step by step.</p></article>
    <article><div class="feature-icon">⚙</div><h2>Extensible</h2><p>Add your own<br>functions and types.</p></article>
  </section>

  <section class="runtimes-section">
    <div class="landing-section-heading"><p class="landing-eyebrow">Meet your stack where it is</p><h2>One language. Many environments.</h2><p>Run Expressif where your data already lives.</p></div>
    <div class="runtime-grid">
      <a class="runtime-card" href="{{ '/cli/' | relative_url }}"><span class="runtime-icon runtime-icon--cli">&gt;_</span><h3>CLI</h3><p>Evaluate expressions everywhere</p></a>
      <a class="runtime-card" href="{{ '/dotnet-sdk/' | relative_url }}"><span class="runtime-icon runtime-icon--dotnet">.NET</span><h3>.NET</h3><p>High performance runtime</p></a>
      <a class="runtime-card" href="{{ '/tooling/' | relative_url }}"><img class="runtime-icon runtime-icon--python" src="{{ '/assets/images/python-logo.svg' | relative_url }}" alt=""><h3>Python</h3><span class="runtime-status">In development</span><p>Seamless Python integration</p></a>
      <a class="runtime-card" href="https://duckdb.org/"><img class="runtime-icon runtime-icon--duckdb" src="{{ '/assets/images/duckdb-icon.svg' | relative_url }}" alt=""><h3>DuckDB</h3><span class="runtime-status">In development</span><p>Run expressions close to your data</p></a>
    </div>
  </section>

  <section class="open-section">
    <div class="open-section__intro"><p class="landing-eyebrow">Built in public</p><h2>Open by nature</h2><p>Open source, community driven and designed to be extended.</p></div>
    <div class="open-section__links">
      <a href="https://github.com/Seddryck/Expressif"><span>◉</span><strong>GitHub</strong><small>Contribute and give feedback</small></a>
      <a href="{{ '/getting-started/' | relative_url }}"><span>▤</span><strong>Documentation</strong><small>Everything you need to know</small></a>
      <a href="https://github.com/Seddryck/Expressif/discussions"><span>◌</span><strong>Community</strong><small>Join the conversation</small></a>
    </div>
  </section>
</main>

<footer class="landing-footer">
  <div class="landing-footer__brand"><div class="landing-brand landing-brand--light"><img class="landing-brand__logo" src="{{ '/assets/images/expressif-logo-reversed.png' | relative_url }}" alt="Expressif"><span>expressif</span></div><p>An expression language for data,<br>logic and transformation.</p><small>© 2026 Expressif</small></div>
  <div><h2>Language</h2><a href="{{ '/language/' | relative_url }}">Overview</a><a href="{{ '/language/' | relative_url }}">Syntax</a><a href="{{ '/functions/' | relative_url }}">Functions</a><a href="{{ '/predicates/' | relative_url }}">Predicates</a></div>
  <div><h2>Runtimes</h2><a href="{{ '/dotnet-sdk/' | relative_url }}">.NET</a><a href="{{ '/cli/' | relative_url }}">CLI</a><a href="{{ '/extending/' | relative_url }}">Extensibility</a></div>
  <div><h2>Ecosystem</h2><a href="{{ '/tooling/' | relative_url }}">Tooling</a><a href="{{ '/extending/' | relative_url }}">Integrations</a><a href="https://github.com/Seddryck/Expressif/discussions">Community</a></div>
  <div><h2>Resources</h2><a href="{{ '/getting-started/' | relative_url }}">Documentation</a><a href="https://github.com/Seddryck/Expressif">GitHub</a><a href="https://github.com/Seddryck/Expressif/issues">Issues</a></div>
</footer>
