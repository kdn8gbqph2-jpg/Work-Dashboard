// BDA Export Helpers

window.bdaGetItem  = function (key)        { return localStorage.getItem(key); };
window.bdaSetItem  = function (key, value) { localStorage.setItem(key, value); };
window.bdaGetValue = function (id)         { return document.getElementById(id)?.value ?? ''; };

// ── Hindi Phonetic Transliteration ────────────────────────────────────────
// Attach to a textarea by element-id; on Space/Enter the last typed word is
// converted from phonetic Roman to Devanagari.  Fires a native 'input' event
// afterwards so Blazor's @bind:event="oninput" picks up the new value.
window.bdaHindi = (function () {
    'use strict';

    const HALANT = '्'; // ् virama

    // Consonants – longest patterns checked first (ORDER IS CRITICAL)
    const CONSONANTS = [
        ['ksh','क्ष'],['gya','ज्ञ'],['shr','श्र'],
        ['chh','छ'], ['Ch', 'छ'],
        ['kh', 'ख'],['gh', 'घ'],['ch', 'च'],['jh', 'झ'],
        ['Th', 'ठ'],['Dh', 'ढ'],['th', 'थ'],['dh', 'ध'],
        ['ph', 'फ'],['bh', 'भ'],['Sh', 'ष'],['sh', 'श'],
        ['ng', 'ङ'],['nj', 'ञ'],
        ['k',  'क'],['g',  'ग'],['c',  'च'],['j',  'ज'],
        ['T',  'ट'],['D',  'ड'],['N',  'ण'],
        ['t',  'त'],['d',  'द'],['n',  'न'],
        ['p',  'प'],['b',  'ब'],['m',  'म'],
        ['y',  'य'],['R',  'ड़'],['r',  'र'],
        ['L',  'ळ'],['l',  'ल'],
        ['v',  'व'],['w',  'व'],
        ['s',  'स'],['h',  'ह'],
        ['f',  'फ'],['z',  'ज'],['q',  'क'],
    ];

    // Vowel matras (after a consonant) – longest first
    const MATRAS = [
        ['aa','ा'],['ee','ी'],['ii','ी'],['oo','ू'],['uu','ू'],
        ['ai','ै'],['au','ौ'],['ao','ौ'],['ae','ै'],
        ['A', 'ा'],['E', 'े'],['I', 'ी'],['O', 'ो'],['U', 'ू'],
        ['e', 'े'],['i', 'ि'],['o', 'ो'],['u', 'ु'],
        ['a', ''],   // inherent 'a' → no matra symbol
    ];

    // Standalone vowels (at word start or after another vowel)
    const VOWELS = [
        ['aa','आ'],['ee','ई'],['ii','ई'],['oo','ऊ'],['uu','ऊ'],
        ['ai','ऐ'],['au','औ'],['ao','औ'],['ae','ऐ'],
        ['A', 'आ'],['E', 'ए'],['I', 'ई'],['O', 'ओ'],['U', 'ऊ'],
        ['e', 'ए'],['i', 'इ'],['o', 'ओ'],['u', 'उ'],
        ['a', 'अ'],
    ];

    const SPECIAL = {
        'M':'ं','H':'ः','~':'ँ','.':'।',
        '0':'०','1':'१','2':'२','3':'३','4':'४',
        '5':'५','6':'६','7':'७','8':'८','9':'९',
    };

    function matchAt(str, pos, table) {
        for (const [rom, dev] of table)
            if (str.startsWith(rom, pos)) return [rom, dev];
        return null;
    }

    function transliterate(word) {
        if (!word) return word;
        let out = '', i = 0, prevCons = false;

        while (i < word.length) {
            // Special symbols
            if (SPECIAL[word[i]]) {
                out += SPECIAL[word[i]];
                prevCons = false;
                i++;
                continue;
            }

            // Try consonant
            const cons = matchAt(word, i, CONSONANTS);
            if (cons) {
                const [cRom, cDev] = cons;
                const afterC = i + cRom.length;
                const vol = matchAt(word, afterC, MATRAS);

                if (prevCons) out += HALANT;
                out += cDev;

                if (vol) {
                    out += vol[1]; // matra (empty string for inherent 'a')
                    i = afterC + vol[0].length;
                    prevCons = false;
                } else {
                    i = afterC;
                    prevCons = true; // next char will decide if halant is needed
                }
                continue;
            }

            // Try standalone vowel
            const vol = matchAt(word, i, VOWELS);
            if (vol) {
                out += prevCons
                    ? (matchAt(word, i, MATRAS) || vol)[1]  // matra form
                    : vol[1];                                 // standalone form
                i += vol[0].length;
                prevCons = false;
                continue;
            }

            // Passthrough (punctuation, unknown chars)
            prevCons = false;
            out += word[i++];
        }
        return out;
    }

    // ── Attach / Detach ────────────────────────────────────────────────────
    const _attached = new Map();

    function attach(id) {
        if (_attached.has(id)) return;
        const el = document.getElementById(id);
        if (!el) return;

        function onKeydown(e) {
            if (e.key !== ' ' && e.key !== 'Enter') return;
            const val = el.value;
            const cursor = el.selectionStart;

            // Walk back to find the start of the current word
            let start = cursor - 1;
            while (start > 0 && val[start - 1] !== ' ' && val[start - 1] !== '\n') start--;

            const word = val.substring(start, cursor);
            if (!word.trim()) return;

            const converted = transliterate(word);
            if (converted === word) return;

            e.preventDefault();
            const sep = e.key === 'Enter' ? '\n' : ' ';
            el.value = val.substring(0, start) + converted + sep + val.substring(cursor);
            const newPos = start + converted.length + 1;
            el.setSelectionRange(newPos, newPos);
            // Notify Blazor
            el.dispatchEvent(new Event('input', { bubbles: true }));
        }

        el.addEventListener('keydown', onKeydown);
        _attached.set(id, onKeydown);
    }

    function detach(id) {
        const handler = _attached.get(id);
        if (!handler) return;
        const el = document.getElementById(id);
        if (el) el.removeEventListener('keydown', handler);
        _attached.delete(id);
    }

    return { attach, detach, transliterate };
})();

// ── amCharts 5 Overview Charts ────────────────────────────────────────
window.bdaCharts = (function () {
    // Registry of active amCharts roots so we can dispose before re-render
    const _roots = {};

    function dispose(divId) {
        if (_roots[divId]) {
            _roots[divId].dispose();
            delete _roots[divId];
        }
    }

    // Horizontal bar chart — Category Wise
    function renderCategoryBar(divId, data) {
        dispose(divId);
        const el = document.getElementById(divId);
        if (!el) return;

        const root = am5.Root.new(divId);
        _roots[divId] = root;
        root.setThemes([am5themes_Animated.new(root)]);

        const chart = root.container.children.push(
            am5xy.XYChart.new(root, { panX: false, panY: false, layout: root.verticalLayout })
        );

        // Sort descending by amount
        const sorted = [...data].sort((a, b) => b.amount - a.amount);

        const yRenderer = am5xy.AxisRendererY.new(root, { minGridDistance: 20 });
        yRenderer.labels.template.setAll({ fontSize: 12, maxWidth: 90, oversizedBehavior: "truncate" });

        const yAxis = chart.yAxes.push(am5xy.CategoryAxis.new(root, {
            categoryField: "label",
            renderer: yRenderer
        }));
        yAxis.data.setAll(sorted);

        const xAxis = chart.xAxes.push(am5xy.ValueAxis.new(root, {
            renderer: am5xy.AxisRendererX.new(root, {}),
            numberFormat: "#,###.##"
        }));

        const series = chart.series.push(am5xy.ColumnSeries.new(root, {
            xAxis, yAxis,
            valueXField: "amount",
            categoryYField: "label",
        }));
        series.columns.template.setAll({
            cornerRadiusTR: 4, cornerRadiusBR: 4,
            fillOpacity: 0.9,
            templateField: "columnSettings",
            tooltipText: "{categoryY}\n₹{valueX} L  ({count} works)",
            tooltipY: am5.percent(50)
        });

        // Assign colours cycling through a palette
        const palette = ["#f97316","#3b82f6","#10b981","#8b5cf6","#ef4444","#eab308","#06b6d4","#ec4899","#84cc16"];
        sorted.forEach((d, i) => d.columnSettings = { fill: am5.color(palette[i % palette.length]) });

        series.data.setAll(sorted);
        series.appear(800);
        chart.appear(800, 100);
    }

    // Donut chart — Fund Source
    function renderFundDonut(divId, data) {
        dispose(divId);
        const el = document.getElementById(divId);
        if (!el) return;

        const root = am5.Root.new(divId);
        _roots[divId] = root;
        root.setThemes([am5themes_Animated.new(root)]);

        const chart = root.container.children.push(
            am5percent.PieChart.new(root, { innerRadius: am5.percent(55), layout: root.horizontalLayout })
        );

        const series = chart.series.push(am5percent.PieSeries.new(root, {
            valueField: "amount",
            categoryField: "label",
            tooltip: am5.Tooltip.new(root, { labelText: "{category}: ₹{value} L" })
        }));
        series.labels.template.set("forceHidden", true);
        series.ticks.template.set("forceHidden", true);
        series.data.setAll(data);

        const legend = chart.children.push(am5.Legend.new(root, {
            centerY: am5.percent(50), y: am5.percent(50), layout: root.verticalLayout
        }));
        legend.labels.template.setAll({ fontSize: 12 });
        legend.valueLabels.template.set("forceHidden", true);
        legend.data.setAll(series.dataItems);

        series.appear(800);
        chart.appear(800, 100);
    }

    // Semi-donut (half pie) — Scheme / CM Budget splits
    function renderSemiDonut(divId, data) {
        dispose(divId);
        const el = document.getElementById(divId);
        if (!el) return;

        const root = am5.Root.new(divId);
        _roots[divId] = root;
        root.setThemes([am5themes_Animated.new(root)]);

        const chart = root.container.children.push(
            am5percent.PieChart.new(root, {
                startAngle: 180, endAngle: 360,
                innerRadius: am5.percent(50),
                layout: root.verticalLayout,
                paddingBottom: 0
            })
        );

        const series = chart.series.push(am5percent.PieSeries.new(root, {
            startAngle: 180, endAngle: 360,
            valueField: "amount",
            categoryField: "label",
            tooltip: am5.Tooltip.new(root, { labelText: "{category}: ₹{value} L ({count} works)" })
        }));
        series.labels.template.setAll({ radius: 8, fontSize: 12 });
        series.ticks.template.set("forceHidden", true);
        series.data.setAll(data);

        const legend = chart.children.push(am5.Legend.new(root, {
            centerX: am5.percent(50), x: am5.percent(50)
        }));
        legend.labels.template.setAll({ fontSize: 12 });
        legend.valueLabels.template.set("forceHidden", true);
        legend.data.setAll(series.dataItems);

        series.appear(800);
        chart.appear(800, 100);
    }

    return { renderCategoryBar, renderFundDonut, renderSemiDonut };
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
