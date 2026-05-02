// BDA Export Helpers

window.bdaGetItem = function (key) { return localStorage.getItem(key); };
window.bdaSetItem = function (key, value) { localStorage.setItem(key, value); };


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
