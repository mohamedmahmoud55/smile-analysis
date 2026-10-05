(function () {
    'use strict';

    function reindexRows(container) {
        var rows = container.querySelectorAll('.rx-medication-row');
        rows.forEach(function (row, index) {
            row.dataset.index = String(index);
            var label = row.querySelector('.rx-row-label');
            if (label) label.textContent = 'Medication ' + (index + 1);

            row.querySelectorAll('input, select, textarea').forEach(function (input) {
                var name = input.getAttribute('name');
                if (!name) return;
                input.setAttribute('name', name.replace(/Items\[\d+\]/, 'Items[' + index + ']'));
                var id = input.getAttribute('id');
                if (id) input.setAttribute('id', id.replace(/Items_\d+__/, 'Items_' + index + '__'));
            });

            row.querySelectorAll('[data-valmsg-for]').forEach(function (span) {
                var valFor = span.getAttribute('data-valmsg-for');
                if (valFor) span.setAttribute('data-valmsg-for', valFor.replace(/Items\[\d+\]/, 'Items[' + index + ']'));
            });

            var removeBtn = row.querySelector('.rx-remove-btn');
            if (removeBtn) removeBtn.disabled = rows.length <= 1;
        });
    }

    function initPrescriptionForm() {
        var container = document.getElementById('medications-container');
        var addBtn = document.getElementById('add-medication-btn');
        var template = document.getElementById('medication-row-template');
        var form = document.getElementById('prescription-form');
        var saveBtn = document.getElementById('save-prescription-btn');

        if (!container || !addBtn || !template) return;

        addBtn.addEventListener('click', function () {
            var index = container.querySelectorAll('.rx-medication-row').length;
            var html = template.innerHTML
                .replace(/__INDEX__/g, String(index))
                .replace(/__NUMBER__/g, String(index + 1));
            var wrapper = document.createElement('div');
            wrapper.innerHTML = html.trim();
            var row = wrapper.firstElementChild;
            container.appendChild(row);
            bindRemove(row);
            reindexRows(container);
        });

        function bindRemove(row) {
            var btn = row.querySelector('.rx-remove-btn');
            if (!btn) return;
            btn.addEventListener('click', function () {
                if (container.querySelectorAll('.rx-medication-row').length <= 1) return;
                row.remove();
                reindexRows(container);
            });
        }

        container.querySelectorAll('.rx-medication-row').forEach(bindRemove);

        if (form && saveBtn) {
            form.addEventListener('submit', function () {
                var text = saveBtn.querySelector('.rx-btn-text');
                var loading = saveBtn.querySelector('.rx-btn-loading');
                if (text) text.classList.add('d-none');
                if (loading) loading.classList.remove('d-none');
                saveBtn.disabled = true;
            });
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initPrescriptionForm);
    } else {
        initPrescriptionForm();
    }
})();
