const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const tm = require('vscode-textmate');
const onig = require('vscode-oniguruma');
(async () => {
  await onig.loadWASM(fs.readFileSync(require.resolve('vscode-oniguruma/release/onig.wasm')).buffer);
  const registry = new tm.Registry({ onigLib: Promise.resolve({
    createOnigScanner: patterns => new onig.OnigScanner(patterns),
    createOnigString: text => new onig.OnigString(text)
  }), loadGrammar: async () => JSON.parse(fs.readFileSync(process.argv[2] || path.join(__dirname, '../obj/verification/expressif.tmLanguage.json'), 'utf8')) });
  const grammar = await registry.loadGrammar('source.expressif');
  const samples = JSON.parse(fs.readFileSync(path.join(__dirname, 'samples.json'), 'utf8'));
  for (const sample of samples) {
    let state = tm.INITIAL;
    const tokens = [];
    for (const line of sample.source.split('\n')) {
      const result = grammar.tokenizeLine(line, state); state = result.ruleStack;
      tokens.push(...result.tokens.map(token => ({text: line.slice(token.startIndex, token.endIndex), scopes: token.scopes})));
    }
    const operators = tokens.filter(t => t.scopes.includes('keyword.operator.expressif'));
    if (sample.quoted) {
      assert(!operators.some(t => t.text.includes('~')), sample.source);
      assert(tokens.some(t => t.text.includes('~') && t.scopes.some(s => s.startsWith('string.quoted.'))), sample.source);
    } else {
      assert.equal(operators.filter(t => t.text === '~').length, (sample.source.match(/~/g) || []).length, sample.source);
    }
    for (const operator of sample.operators || []) assert(operators.some(t => t.text === operator), `${sample.source}: ${operator}`);
    for (const name of sample.names) assert(tokens.some(t => t.text === name && t.scopes.some(s => s.startsWith('support.function.'))), `${sample.source}: ${name}`);
  }
  console.log(`TextMate: ${samples.length} tokenization cases passed`);
})().catch(error => { console.error(error); process.exitCode = 1; });
