(function () {
    function initToothChart(canvasId, chartDataJson) {
        var canvas = document.getElementById(canvasId);
        if (!canvas || !chartDataJson || typeof Chart === 'undefined') return;

        var data = JSON.parse(chartDataJson);
        if (!data.labels || !data.datasets) return;

        new Chart(canvas, {
            type: 'bar',
            data: data,
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'bottom', labels: { boxWidth: 12, font: { size: 11 } } },
                    tooltip: { callbacks: { label: function (ctx) { return ctx.dataset.label + ': ' + ctx.parsed.y.toFixed(1) + ' mm'; } } }
                },
                scales: {
                    x: { stacked: true, title: { display: true, text: 'Tooth (FDI)' } },
                    y: { stacked: true, beginAtZero: true, title: { display: true, text: 'mm' } }
                }
            }
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        var el = document.getElementById('gsToothChart');
        if (el) initToothChart('gsToothChart', el.dataset.chart);
    });
})();
