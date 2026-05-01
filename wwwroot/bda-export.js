// BDA Export Helpers

window.bdaGetItem = function (key) { return localStorage.getItem(key); };
window.bdaSetItem = function (key, value) { localStorage.setItem(key, value); };

// ── Hindi transliteration via Google Input Tools ────────────────────
window.bdaHindi = (function () {
    const TRIGGERS = new Set([' ', 'Enter', '.', ',', '?', '!', ';', ':']);

    async function translit(word) {
        try {
            const url = 'https://inputtools.google.com/request?text=' +
                encodeURIComponent(word) +
                '&itc=hi-t-i0-und&num=1&cp=0&cs=1&ie=utf-8&oe=utf-8';
            const r = await fetch(url);
            const j = await r.json();
            if (j && j[0] === 'SUCCESS' && j[1] && j[1][0] && j[1][0][1] && j[1][0][1][0]) {
                return j[1][0][1][0];
            }
        } catch (e) { console.warn('Hindi transliteration failed', e); }
        return null;
    }

    function attach(textareaId) {
        const ta = document.getElementById(textareaId);
        if (!ta || ta._bdaHindiAttached) return;
        ta._bdaHindiAttached = true;

        ta.addEventListener('keydown', async function (e) {
            if (ta.dataset.hindi !== 'true') return;
            if (!TRIGGERS.has(e.key)) return;

            const pos = ta.selectionStart;
            const before = ta.value.slice(0, pos);
            const after = ta.value.slice(pos);

            const m = before.match(/([a-zA-Z][a-zA-Z'-]*)$/);
            if (!m) return;

            const word = m[1];
            e.preventDefault();

            const sep = e.key === 'Enter' ? '\n' : e.key;
            // Optimistic insert with original word, then replace once API responds
            const placeholder = before + sep + after;
            ta.value = placeholder;
            const cursorAfter = before.length + sep.length;
            ta.setSelectionRange(cursorAfter, cursorAfter);

            const hindi = await translit(word);
            if (hindi) {
                const newBefore = before.slice(0, -word.length) + hindi;
                ta.value = newBefore + sep + after;
                const newPos = newBefore.length + sep.length;
                ta.setSelectionRange(newPos, newPos);
            }
            ta.dispatchEvent(new Event('input', { bubbles: true }));
        });
    }

    function setMode(textareaId, on) {
        const ta = document.getElementById(textareaId);
        if (ta) ta.dataset.hindi = on ? 'true' : 'false';
    }

    return { attach: attach, setMode: setMode };
})();

window.bdaDownloadBase64 = function (filename, base64) {
    const bytes = atob(base64);
    const ab = new ArrayBuffer(bytes.length);
    const ia = new Uint8Array(ab);
    for (let i = 0; i < bytes.length; i++) ia[i] = bytes.charCodeAt(i);
    const blob = new Blob([ab]);
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = filename;
    document.body.appendChild(a); a.click();
    document.body.removeChild(a); URL.revokeObjectURL(url);
};

window.bdaDownloadText = function (filename, text) {
    const blob = new Blob(['﻿' + text], { type: 'text/csv;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = filename;
    document.body.appendChild(a); a.click();
    document.body.removeChild(a); URL.revokeObjectURL(url);
};

window.bdaExportPdf = function (title, subtitle, headers, rows) {
    const generated = new Date().toLocaleString('en-IN');
    const date      = new Date().toISOString().slice(0, 10);

    const thCells = headers.map(h =>
        `<th>${h}</th>`).join('');

    const trRows = rows.map((row, i) =>
        `<tr class="${i % 2 === 1 ? 'alt' : ''}">${row.map(c => `<td>${c ?? ''}</td>`).join('')}</tr>`
    ).join('');

    const html = `<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<title>${title}</title>
<style>
  @page { size: A4 landscape; margin: 12mm; }
  * { box-sizing: border-box; font-family: 'Segoe UI', Arial, sans-serif; }
  body { margin: 0; font-size: 8pt; color: #111; }
  .report-header { margin-bottom: 10px; }
  .report-header h2 { margin: 0 0 2px; font-size: 14pt; }
  .report-header p  { margin: 0; font-size: 8pt; color: #555; }
  table { width: 100%; border-collapse: collapse; }
  th {
    background: #FF8C00; color: #fff; font-weight: 700;
    padding: 5px 6px; text-align: left; font-size: 7.5pt;
    border: 1px solid #e06000;
  }
  td { padding: 4px 6px; border: 1px solid #e0e0e0; vertical-align: top; font-size: 7.5pt; }
  tr.alt td { background: #FFF8F0; }
  @media print { button { display: none; } }
</style>
</head>
<body>
<div class="report-header">
  <h2>${title}</h2>
  <p>${subtitle}</p>
  <p>Generated: ${generated}</p>
</div>
<table>
  <thead><tr>${thCells}</tr></thead>
  <tbody>${trRows}</tbody>
</table>
</body>
</html>`;

    const win = window.open('', '_blank');
    win.document.write(html);
    win.document.close();
    win.focus();
    setTimeout(() => win.print(), 800);
};
