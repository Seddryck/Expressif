---
title: Brand colors
layout: docs
nav_order: 9
permalink: /brand-colors/
description: The official Expressif color palette, semantic roles, and implementation values.
---

<div class="brand-colors" markdown="1">

<p class="brand-colors__intro">A warm, grounded palette inspired by coffee and balanced with crisp slate neutrals. Use these colors consistently to keep Expressif interfaces recognizable, readable, and focused.</p>

<section class="brand-colors__section" aria-labelledby="core-colors">
  <h2 class="brand-colors__section-heading" id="core-colors">Core colors</h2>
  <p class="brand-colors__section-copy">The primary brown carries the brand. Accent adds warmth, link blue signals interaction, and ink anchors text and dark surfaces.</p>

  <div class="brand-colors__grid">
    <article class="brand-color">
      <div class="brand-color__swatch" style="--swatch: #a66a3f"><span class="brand-color__role">Primary</span></div>
      <div class="brand-color__body"><div><p class="brand-color__name">Expressif Brown</p><p class="brand-color__usage">Primary actions and brand emphasis</p></div><button class="brand-color__copy" type="button" data-color="#A66A3F" aria-label="Copy Expressif Brown hex code">#A66A3F</button></div>
    </article>
    <article class="brand-color">
      <div class="brand-color__swatch" style="--swatch: #d4a373"><span class="brand-color__role">Accent</span></div>
      <div class="brand-color__body"><div><p class="brand-color__name">Coffee Accent</p><p class="brand-color__usage">Highlights and active indicators</p></div><button class="brand-color__copy" type="button" data-color="#D4A373" aria-label="Copy Coffee Accent hex code">#D4A373</button></div>
    </article>
    <article class="brand-color">
      <div class="brand-color__swatch" style="--swatch: #2563eb"><span class="brand-color__role">Interactive</span></div>
      <div class="brand-color__body"><div><p class="brand-color__name">Link Blue</p><p class="brand-color__usage">Links and keyboard focus</p></div><button class="brand-color__copy" type="button" data-color="#2563EB" aria-label="Copy Link Blue hex code">#2563EB</button></div>
    </article>
    <article class="brand-color">
      <div class="brand-color__swatch" style="--swatch: #0f172a"><span class="brand-color__role">Ink</span></div>
      <div class="brand-color__body"><div><p class="brand-color__name">Deep Ink</p><p class="brand-color__usage">Body text, navigation, and dark surfaces</p></div><button class="brand-color__copy" type="button" data-color="#0F172A" aria-label="Copy Deep Ink hex code">#0F172A</button></div>
    </article>
  </div>
</section>

<section class="brand-colors__section" aria-labelledby="coffee-scale">
  <h2 class="brand-colors__section-heading" id="coffee-scale">Coffee scale</h2>
  <p class="brand-colors__section-copy">Use lighter values for subtle backgrounds and borders; reserve Coffee 300 for strong, warm emphasis.</p>
  <div class="brand-colors__scale" aria-label="Coffee color scale">
    <div class="brand-scale-color" style="--swatch: #faf6f1"><span class="brand-scale-color__name">Coffee 000</span><span class="brand-scale-color__hex">#FAF6F1</span></div>
    <div class="brand-scale-color" style="--swatch: #ead8c5"><span class="brand-scale-color__name">Coffee 100</span><span class="brand-scale-color__hex">#EAD8C5</span></div>
    <div class="brand-scale-color" style="--swatch: #cda472"><span class="brand-scale-color__name">Coffee 200</span><span class="brand-scale-color__hex">#CDA472</span></div>
    <div class="brand-scale-color" style="--swatch: #9a633d; --scale-ink: #ffffff"><span class="brand-scale-color__name">Coffee 300</span><span class="brand-scale-color__hex">#9A633D</span></div>
  </div>
</section>

<section class="brand-colors__section" aria-labelledby="slate-scale">
  <h2 class="brand-colors__section-heading" id="slate-scale">Slate scale</h2>
  <p class="brand-colors__section-copy">Slate provides the neutral foundation for backgrounds, borders, secondary text, navigation, and code surfaces.</p>
  <div class="brand-colors__scale brand-colors__scale--slate" aria-label="Slate color scale">
    <div class="brand-scale-color" style="--swatch: #f8fafc"><span class="brand-scale-color__name">Slate 050</span><span class="brand-scale-color__hex">#F8FAFC</span></div>
    <div class="brand-scale-color" style="--swatch: #f1f5f9"><span class="brand-scale-color__name">Slate 100</span><span class="brand-scale-color__hex">#F1F5F9</span></div>
    <div class="brand-scale-color" style="--swatch: #e2e8f0"><span class="brand-scale-color__name">Slate 200</span><span class="brand-scale-color__hex">#E2E8F0</span></div>
    <div class="brand-scale-color" style="--swatch: #cbd5e1"><span class="brand-scale-color__name">Slate 300</span><span class="brand-scale-color__hex">#CBD5E1</span></div>
    <div class="brand-scale-color" style="--swatch: #64748b; --scale-ink: #ffffff"><span class="brand-scale-color__name">Slate 500</span><span class="brand-scale-color__hex">#64748B</span></div>
    <div class="brand-scale-color" style="--swatch: #334155; --scale-ink: #ffffff"><span class="brand-scale-color__name">Slate 700</span><span class="brand-scale-color__hex">#334155</span></div>
    <div class="brand-scale-color" style="--swatch: #0f172a; --scale-ink: #ffffff"><span class="brand-scale-color__name">Slate 900</span><span class="brand-scale-color__hex">#0F172A</span></div>
  </div>
</section>

<section class="brand-colors__section" aria-labelledby="implementation">
  <h2 class="brand-colors__section-heading" id="implementation">Implementation</h2>
  <p class="brand-colors__section-copy">Reference the shared Sass tokens instead of repeating literal values in components.</p>

<div class="language-scss highlighter-rouge"><div class="highlight"><pre class="highlight"><code><span class="c1">// Brand</span>
<span class="nv">$principal-light-color</span>: <span class="mh">#a66a3f</span>;
<span class="nv">$accent-color</span>: <span class="mh">#d4a373</span>;
<span class="nv">$link-color</span>: <span class="mh">#2563eb</span>;
<span class="nv">$principal-dark-color</span>: <span class="mh">#0f172a</span>;

<span class="c1">// Example</span>
<span class="nc">.primary-action</span> {
  <span class="nl">color</span>: <span class="mh">#ffffff</span>;
  <span class="nl">background</span>: <span class="nv">$principal-light-color</span>;
}</code></pre></div></div>
</section>

<aside class="brand-colors__note"><strong>Accessibility:</strong> Use white text on Link Blue, Slate 500, Slate 700, and Slate 900. Expressif Brown supports white for large or bold labels; use Coffee 300 when normal-size white text needs stronger contrast. Use Deep Ink on the lighter coffee and slate tones.</aside>

</div>

<script>
document.addEventListener("click", function (event) {
  var button = event.target.closest(".brand-color__copy");
  if (!button) return;

  var color = button.getAttribute("data-color");
  function showConfirmation() {
    var original = button.textContent;
    button.textContent = "Copied!";
    window.setTimeout(function () { button.textContent = original; }, 1400);
  }

  if (navigator.clipboard && navigator.clipboard.writeText) {
    navigator.clipboard.writeText(color).then(showConfirmation);
  }
});
</script>
