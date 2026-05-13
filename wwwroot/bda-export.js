// BDA Export Helpers

window.bdaGetItem  = function (key)        { return localStorage.getItem(key); };
window.bdaSetItem  = function (key, value) { localStorage.setItem(key, value); };
window.bdaGetValue = function (id)         { return document.getElementById(id)?.value ?? ''; };

// ── Hindi Transliteration via Google Input Tools ─────────────────────────
// On Space/Enter, the last typed Roman word is sent to Google Input Tools
// and replaced with the best Devanagari suggestion.
window.bdaHindi = (function () {
    'use strict';

    // Call Google Input Tools transliteration endpoint.
    // Returns the best Hindi suggestion, or null on failure (offline, CORS, etc.)
    async function googleTransliterate(word) {
        try {
            const url = 'https://inputtools.google.com/request?' +
                'text=' + encodeURIComponent(word) +
                '&itc=hi-t-i0-und&num=1&cp=0&cs=1&ie=utf-8&oe=utf-8';
            const res = await fetch(url);
            if (!res.ok) return null;
            const data = await res.json();
            // Response: ["SUCCESS", [["word", ["suggestion", ...], ...]]]
            if (data[0] === 'SUCCESS' && data[1]?.[0]?.[1]?.[0])
                return data[1][0][1][0];
        } catch (_) {}
        return null;
    }

    // ── Attach / Detach ───────────────────────────────────────────────────
    const _attached = new Map();

    function attach(id) {
        if (_attached.has(id)) return;
        const el = document.getElementById(id);
        if (!el) return;

        async function onKeydown(e) {
            if (e.key !== ' ' && e.key !== 'Enter') return;

            const val    = el.value;
            const cursor = el.selectionStart;

            // Find start of the current word
            let start = cursor - 1;
            while (start > 0 && val[start - 1] !== ' ' && val[start - 1] !== '\n') start--;

            const word = val.substring(start, cursor).trim();
            // Skip empty words or words already in Devanagari
            if (!word || /[ऀ-ॿ]/.test(word)) return;

            // Prevent default BEFORE the first await so the browser honours it
            e.preventDefault();
            const sep = e.key === 'Enter' ? '\n' : ' ';

            const converted = await googleTransliterate(word) ?? word;

            el.value = val.substring(0, start) + converted + sep + val.substring(cursor);
            const newPos = start + converted.length + 1;
            el.setSelectionRange(newPos, newPos);
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

    return { attach, detach };
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

// ── Mobile nav (hamburger off-canvas sidebar) ───────────────────────────
// Injects a hamburger toggle into every .jen-topbar that appears in the DOM.
// CSS controls visibility (hidden on desktop, shown <= 992px).
(function () {
    'use strict';

    function inject(topbar) {
        if (!topbar || topbar.dataset.mobileNavReady === '1') return;
        topbar.dataset.mobileNavReady = '1';

        const firstChild = topbar.firstElementChild;
        if (!firstChild) return;

        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'mobile-nav-toggle';
        btn.setAttribute('aria-label', 'Toggle navigation');
        btn.innerHTML = '<i class="bi bi-list"></i>';
        btn.addEventListener('click', function (e) {
            e.stopPropagation();
            document.body.classList.toggle('sidebar-open');
        });

        const wrap = document.createElement('div');
        wrap.className = 'topbar-title-group';
        topbar.insertBefore(wrap, firstChild);
        wrap.appendChild(btn);
        wrap.appendChild(firstChild);

        if (!document.querySelector('.mobile-sidebar-backdrop')) {
            const bd = document.createElement('div');
            bd.className = 'mobile-sidebar-backdrop';
            bd.addEventListener('click', function () {
                document.body.classList.remove('sidebar-open');
            });
            document.body.appendChild(bd);
        }
    }

    function scan(root) {
        (root || document).querySelectorAll('.jen-topbar').forEach(inject);
    }

    function init() {
        scan();
        const obs = new MutationObserver(function (mutations) {
            for (const m of mutations) {
                for (const n of m.addedNodes) {
                    if (n.nodeType !== 1) continue;
                    if (n.matches && n.matches('.jen-topbar')) inject(n);
                    if (n.querySelectorAll) n.querySelectorAll('.jen-topbar').forEach(inject);
                }
            }
        });
        obs.observe(document.body, { childList: true, subtree: true });

        // Close sidebar when a nav item is tapped on mobile
        document.addEventListener('click', function (e) {
            const navItem = e.target.closest('.jen-sidebar .nav-item');
            if (navItem && document.body.classList.contains('sidebar-open')) {
                document.body.classList.remove('sidebar-open');
            }
        });

        // Close on Escape
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && document.body.classList.contains('sidebar-open')) {
                document.body.classList.remove('sidebar-open');
            }
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
