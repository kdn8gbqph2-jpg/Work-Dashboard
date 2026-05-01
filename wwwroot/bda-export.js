// BDA Export Helpers

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
    const { jsPDF } = window.jspdf;
    const doc = new jsPDF({ orientation: 'landscape', unit: 'mm', format: 'a4' });

    doc.setFontSize(13);
    doc.setFont('helvetica', 'bold');
    doc.text(title, 14, 14);

    doc.setFontSize(8);
    doc.setFont('helvetica', 'normal');
    doc.text(subtitle, 14, 20);
    doc.text('Generated: ' + new Date().toLocaleString('en-IN'), 14, 25);

    doc.autoTable({
        head: [headers],
        body: rows,
        startY: 30,
        styles: { fontSize: 6.5, cellPadding: 1.8, overflow: 'linebreak' },
        headStyles: { fillColor: [255, 140, 0], textColor: 255, fontStyle: 'bold', fontSize: 7 },
        alternateRowStyles: { fillColor: [255, 251, 240] },
        columnStyles: { 2: { cellWidth: 55 } },
        margin: { left: 10, right: 10 }
    });

    doc.save(title.replace(/\s+/g, '_') + '_' + new Date().toISOString().slice(0, 10) + '.pdf');
};
