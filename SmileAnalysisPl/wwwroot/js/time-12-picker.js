(function (window) {
    'use strict';

    function pad2(n) {
        return n < 10 ? '0' + n : String(n);
    }

    function parse24(value) {
        if (!value) return null;
        var parts = value.split(':');
        var h = parseInt(parts[0], 10);
        var m = parseInt(parts[1], 10);
        if (isNaN(h) || isNaN(m)) return null;
        return { h: h, m: m };
    }

    function to24(h12, m, ampm) {
        var h = parseInt(h12, 10);
        if (isNaN(h)) h = 12;
        if (ampm === 'AM') {
            if (h === 12) h = 0;
        } else {
            if (h !== 12) h += 12;
        }
        return pad2(h) + ':' + pad2(m);
    }

    function to12Parts(h24, m24) {
        var ampm = h24 >= 12 ? 'PM' : 'AM';
        var h12 = h24 % 12;
        if (h12 === 0) h12 = 12;
        return { h12: String(h12), m: pad2(m24), ampm: ampm };
    }

    function syncFromSelects(container) {
        var inputId = container.getAttribute('data-time-input');
        var input = inputId ? document.getElementById(inputId) : null;
        if (!input) return;

        var hourEl = container.querySelector('.cc-time-12-hour');
        var minEl = container.querySelector('.cc-time-12-minute');
        var ampmEl = container.querySelector('.cc-time-12-ampm');
        if (!hourEl || !minEl || !ampmEl) return;

        input.value = to24(hourEl.value, parseInt(minEl.value, 10), ampmEl.value) + ':00';
    }

    function syncToSelects(container) {
        var inputId = container.getAttribute('data-time-input');
        var input = inputId ? document.getElementById(inputId) : null;
        if (!input) return;

        var parsed = parse24(input.value);
        if (!parsed) return;

        var parts = to12Parts(parsed.h, parsed.m);
        var hourEl = container.querySelector('.cc-time-12-hour');
        var minEl = container.querySelector('.cc-time-12-minute');
        var ampmEl = container.querySelector('.cc-time-12-ampm');
        if (hourEl) hourEl.value = parts.h12;
        if (minEl) minEl.value = parts.m;
        if (ampmEl) ampmEl.value = parts.ampm;
    }

    function syncAll(root) {
        (root || document).querySelectorAll('.cc-time-12').forEach(syncFromSelects);
    }

    function initTime12Pickers(root) {
        var scope = root || document;
        scope.querySelectorAll('.cc-time-12').forEach(function (container) {
            syncToSelects(container);
            container.querySelectorAll('select').forEach(function (sel) {
                sel.addEventListener('change', function () {
                    syncFromSelects(container);
                });
            });
        });

        scope.querySelectorAll('form').forEach(function (form) {
            if (!form.querySelector('.cc-time-12') || form.dataset.time12Bound === '1')
                return;
            form.dataset.time12Bound = '1';
            form.addEventListener('submit', function () {
                syncAll(form);
            });
        });
    }

    window.initTime12Pickers = initTime12Pickers;
})(window);
